namespace KioscoLaFamilia.Datos;

public static class ProductoRepo
{
    private const string Campos =
        "id, codigo_barras, nombre, categoria, precio_costo, precio_venta, unidad, stock, stock_minimo, controla_stock, venta_rapida, activo";

    private static Producto Mapear(DataRow r) => new Producto
    {
        Id = r.Int("id"),
        CodigoBarras = r.Str("codigo_barras"),
        Nombre = r.Str("nombre"),
        Categoria = r.Str("categoria"),
        PrecioCosto = r.Dec("precio_costo"),
        PrecioVenta = r.Dec("precio_venta"),
        Unidad = r.Str("unidad") == "" ? "UNIDAD" : r.Str("unidad"),
        Stock = r.Dec("stock"),
        StockMinimo = r.Dec("stock_minimo"),
        ControlaStock = r.Bool("controla_stock"),
        VentaRapida = r.Bool("venta_rapida"),
        Activo = r.Bool("activo")
    };

    private static List<Producto> Lista(string sql, params (string, object)[] ps)
    {
        var lista = new List<Producto>();
        foreach (DataRow r in Db.Tabla(sql, ps).Rows) lista.Add(Mapear(r));
        return lista;
    }

    public static Producto ObtenerPorId(int id) =>
        Lista($"SELECT {Campos} FROM productos WHERE id = @id", ("@id", id)).FirstOrDefault();

    public static Producto ObtenerPorCodigo(string codigo) =>
        Lista($"SELECT {Campos} FROM productos WHERE codigo_barras = @c AND activo = 1", ("@c", codigo)).FirstOrDefault();

    public static List<Producto> Buscar(string filtro) =>
        Lista($"SELECT {Campos} FROM productos WHERE activo = 1 AND " +
              "(nombre LIKE @f OR IFNULL(codigo_barras,'') LIKE @f OR categoria LIKE @f) ORDER BY nombre LIMIT 200",
              ("@f", "%" + filtro + "%"));

    /// <summary>Listado para grillas.</summary>
    public static DataTable Tabla(string filtro, bool soloStockBajo = false, string categoria = null) => Db.Tabla(
        "SELECT id, codigo_barras AS `Código`, nombre AS `Producto`, categoria AS `Categoría`, " +
        "precio_costo AS `Costo`, precio_venta AS `Precio`, " +
        "ROUND(CASE WHEN precio_costo > 0 THEN (precio_venta - precio_costo) / precio_costo * 100 ELSE 0 END, 1) AS `Margen %`, " +
        "IF(unidad = 'KG', 'Por kilo', 'Unidad') AS `Se vende`, " +
        "stock AS `Stock`, stock_minimo AS `Mínimo`, IF(controla_stock = 1, 'Sí', 'No') AS `Controla stock`, " +
        "IF(controla_stock = 1 AND stock_minimo > 0 AND stock <= stock_minimo, 1, 0) AS bajo " +
        "FROM productos WHERE activo = 1 AND (nombre LIKE @f OR IFNULL(codigo_barras,'') LIKE @f OR categoria LIKE @f)" +
        (soloStockBajo ? " AND controla_stock = 1 AND stock_minimo > 0 AND stock <= stock_minimo" : "") +
        (string.IsNullOrEmpty(categoria) ? "" : " AND categoria = @cat") +
        " ORDER BY categoria, nombre",
        ("@f", "%" + (filtro ?? "") + "%"), ("@cat", categoria));

    /// <summary>Productos marcados para venderse con botón (helados, pan, golosinas sueltas...).</summary>
    public static List<Producto> ParaVentaRapida() =>
        Lista($"SELECT {Campos} FROM productos WHERE activo = 1 AND venta_rapida = 1 ORDER BY categoria, precio_venta DESC, nombre");

    public static List<Producto> StockBajo() =>
        Lista($"SELECT {Campos} FROM productos WHERE activo = 1 AND controla_stock = 1 AND stock_minimo > 0 AND stock <= stock_minimo ORDER BY (stock - stock_minimo), nombre");

    public static List<string> Categorias()
    {
        var lista = new List<string>(KioscoLaFamilia.Modelos.Categorias.Predeterminadas);
        foreach (DataRow r in Db.Tabla("SELECT DISTINCT categoria FROM productos WHERE activo = 1").Rows)
            if (!lista.Contains(r.Str("categoria"))) lista.Add(r.Str("categoria"));
        lista.Sort(StringComparer.CurrentCultureIgnoreCase);
        return lista;
    }

    /// <summary>Alta o modificación. En modificación el stock NO se toca (se usa MoverStock).</summary>
    public static void Guardar(Producto p)
    {
        using var c = Db.Abrir();
        using var tx = c.BeginTransaction();
        try
        {
            object codigo = string.IsNullOrWhiteSpace(p.CodigoBarras) ? null : p.CodigoBarras.Trim();
            if (p.Id == 0)
            {
                p.Id = (int)Db.Insertar(c, tx,
                    "INSERT INTO productos (codigo_barras, nombre, categoria, precio_costo, precio_venta, unidad, stock, stock_minimo, controla_stock, venta_rapida) " +
                    "VALUES (@cod, @n, @cat, @pc, @pv, @un, @s, @sm, @cs, @vr)",
                    ("@cod", codigo), ("@n", p.Nombre), ("@cat", p.Categoria), ("@pc", p.PrecioCosto),
                    ("@pv", p.PrecioVenta), ("@un", p.Unidad), ("@s", p.Stock), ("@sm", p.StockMinimo),
                    ("@cs", p.ControlaStock), ("@vr", p.VentaRapida));
                if (p.Stock != 0 && p.ControlaStock)
                    RegistrarMovimiento(c, tx, p.Id, "INGRESO", p.Stock, "Stock inicial");
            }
            else
            {
                ActualizarPrecios(c, tx, p.Id, p.PrecioCosto, p.PrecioVenta);
                Db.Ejecutar(c, tx,
                    "UPDATE productos SET codigo_barras = @cod, nombre = @n, categoria = @cat, unidad = @un, stock_minimo = @sm, " +
                    "controla_stock = @cs, venta_rapida = @vr WHERE id = @id",
                    ("@cod", codigo), ("@n", p.Nombre), ("@cat", p.Categoria), ("@un", p.Unidad), ("@sm", p.StockMinimo),
                    ("@cs", p.ControlaStock), ("@vr", p.VentaRapida), ("@id", p.Id));
            }
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>Baja lógica: el producto deja de aparecer pero se conserva para el historial de ventas.</summary>
    public static void Eliminar(int id) =>
        Db.Ejecutar("UPDATE productos SET activo = 0, codigo_barras = NULL WHERE id = @id", ("@id", id));

    /// <summary>
    /// tipo: INGRESO (suma), BAJA (resta) o AJUSTE (cantidad = stock real contado).
    /// Opcionalmente actualiza costo y precio de venta.
    /// </summary>
    public static void MoverStock(int productoId, string tipo, decimal cantidad, string observacion,
                                  decimal? nuevoCosto = null, decimal? nuevoPrecio = null)
    {
        using var c = Db.Abrir();
        using var tx = c.BeginTransaction();
        try
        {
            decimal actual = Db.ADecimal(Db.Escalar(c, tx, "SELECT stock FROM productos WHERE id = @id FOR UPDATE", ("@id", productoId)));
            decimal delta = tipo switch
            {
                "AJUSTE" => cantidad - actual,
                "BAJA" => -Math.Abs(cantidad),
                _ => Math.Abs(cantidad)
            };
            Db.Ejecutar(c, tx, "UPDATE productos SET stock = stock + @d WHERE id = @id", ("@d", delta), ("@id", productoId));

            if (nuevoCosto.HasValue || nuevoPrecio.HasValue)
            {
                var t = Db.Tabla(c, tx, "SELECT precio_costo, precio_venta FROM productos WHERE id = @id", ("@id", productoId));
                ActualizarPrecios(c, tx, productoId,
                    nuevoCosto ?? t.Rows[0].Dec("precio_costo"),
                    nuevoPrecio ?? t.Rows[0].Dec("precio_venta"));
            }

            RegistrarMovimiento(c, tx, productoId, tipo, delta, observacion);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>Aumenta (o baja, con % negativo) los precios de venta; opcionalmente también el costo.</summary>
    public static int AumentarPrecios(decimal porcentaje, string categoria, bool tambienCosto)
    {
        bool todas = string.IsNullOrEmpty(categoria);
        string filtro = "activo = 1" + (todas ? "" : " AND categoria = @cat");
        decimal factor = 1 + porcentaje / 100m;

        using var c = Db.Abrir();
        using var tx = c.BeginTransaction();
        try
        {
            Db.Ejecutar(c, tx,
                "INSERT INTO historial_precios (producto_id, costo_anterior, precio_anterior, costo_nuevo, precio_nuevo, usuario_id) " +
                "SELECT id, precio_costo, precio_venta, " +
                (tambienCosto ? "ROUND(precio_costo * @f, 2)" : "precio_costo") +
                ", ROUND(precio_venta * @f, 2), @u FROM productos WHERE " + filtro,
                ("@f", factor), ("@u", Sesion.Usuario.Id), ("@cat", categoria));

            int n = Db.Ejecutar(c, tx,
                "UPDATE productos SET precio_venta = ROUND(precio_venta * @f, 2)" +
                (tambienCosto ? ", precio_costo = ROUND(precio_costo * @f, 2)" : "") +
                " WHERE " + filtro,
                ("@f", factor), ("@cat", categoria));
            tx.Commit();
            return n;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public static DataTable UltimosMovimientos(int limite = 100) => Db.Tabla(
        "SELECT m.fecha AS `Fecha`, p.nombre AS `Producto`, m.tipo AS `Tipo`, m.cantidad AS `Cantidad`, " +
        "m.stock_resultante AS `Stock final`, u.usuario AS `Usuario`, m.observacion AS `Observación` " +
        "FROM movimientos_stock m JOIN productos p ON p.id = m.producto_id JOIN usuarios u ON u.id = m.usuario_id " +
        "ORDER BY m.id DESC LIMIT @l", ("@l", limite));

    public static DataTable HistorialPrecios(int productoId) => Db.Tabla(
        "SELECT h.fecha AS `Fecha`, h.costo_anterior AS `Costo anterior`, h.costo_nuevo AS `Costo nuevo`, " +
        "h.precio_anterior AS `Precio anterior`, h.precio_nuevo AS `Precio nuevo`, u.usuario AS `Usuario` " +
        "FROM historial_precios h JOIN usuarios u ON u.id = h.usuario_id WHERE h.producto_id = @id ORDER BY h.id DESC",
        ("@id", productoId));

    // ----- Internos (dentro de transacción) -----

    private static void ActualizarPrecios(MySqlConnection c, MySqlTransaction tx, int id, decimal costo, decimal precio)
    {
        var t = Db.Tabla(c, tx, "SELECT precio_costo, precio_venta FROM productos WHERE id = @id", ("@id", id));
        if (t.Rows.Count == 0) return;
        decimal costoAnt = t.Rows[0].Dec("precio_costo");
        decimal precioAnt = t.Rows[0].Dec("precio_venta");
        if (costoAnt == costo && precioAnt == precio) return;

        Db.Ejecutar(c, tx, "UPDATE productos SET precio_costo = @pc, precio_venta = @pv WHERE id = @id",
            ("@pc", costo), ("@pv", precio), ("@id", id));
        Db.Ejecutar(c, tx,
            "INSERT INTO historial_precios (producto_id, costo_anterior, precio_anterior, costo_nuevo, precio_nuevo, usuario_id) " +
            "VALUES (@id, @ca, @pa, @cn, @pn, @u)",
            ("@id", id), ("@ca", costoAnt), ("@pa", precioAnt), ("@cn", costo), ("@pn", precio), ("@u", Sesion.Usuario.Id));
    }

    /// <summary>Registra el movimiento guardando el stock que quedó después del cambio.</summary>
    internal static void RegistrarMovimiento(MySqlConnection c, MySqlTransaction tx, int productoId, string tipo,
                                             decimal cantidad, string observacion)
    {
        Db.Ejecutar(c, tx,
            "INSERT INTO movimientos_stock (producto_id, tipo, cantidad, stock_resultante, usuario_id, observacion) " +
            "SELECT id, @t, @c, stock, @u, @o FROM productos WHERE id = @p",
            ("@p", productoId), ("@t", tipo), ("@c", cantidad), ("@u", Sesion.Usuario.Id), ("@o", observacion));
    }
}
