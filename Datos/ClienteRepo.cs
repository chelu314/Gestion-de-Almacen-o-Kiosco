namespace KioscoLaFamilia.Datos;

public static class ClienteRepo
{
    private const string SqlSaldo =
        "(COALESCE((SELECT SUM(v.total) FROM ventas v WHERE v.cliente_id = c.id AND v.forma_pago = 'CUENTA_CORRIENTE' AND v.estado = 'ACTIVA'), 0) " +
        " - COALESCE((SELECT SUM(p.monto) FROM pagos_clientes p WHERE p.cliente_id = c.id), 0))";

    private static Cliente Mapear(DataRow r) => new Cliente
    {
        Id = r.Int("id"),
        Nombre = r.Str("nombre"),
        Telefono = r.Str("telefono"),
        Direccion = r.Str("direccion"),
        LimiteCredito = r.Dec("limite_credito"),
        Observaciones = r.Str("observaciones")
    };

    public static DataTable Tabla(string filtro, bool soloDeudores) => Db.Tabla(
        "SELECT * FROM (SELECT c.id, c.nombre AS `Cliente`, c.telefono AS `Teléfono`, c.direccion AS `Dirección`, " +
        "c.limite_credito AS `Límite`, " + SqlSaldo + " AS `Saldo` " +
        "FROM clientes c WHERE c.activo = 1 AND (c.nombre LIKE @f OR IFNULL(c.telefono,'') LIKE @f)) x " +
        (soloDeudores ? "WHERE x.`Saldo` > 0 " : "") +
        "ORDER BY x.`Cliente`",
        ("@f", "%" + (filtro ?? "") + "%"));

    public static List<Cliente> ListaActivos()
    {
        var lista = new List<Cliente>();
        foreach (DataRow r in Db.Tabla("SELECT * FROM clientes WHERE activo = 1 ORDER BY nombre").Rows)
            lista.Add(Mapear(r));
        return lista;
    }

    public static Cliente ObtenerPorId(int id)
    {
        var t = Db.Tabla("SELECT * FROM clientes WHERE id = @id", ("@id", id));
        return t.Rows.Count == 0 ? null : Mapear(t.Rows[0]);
    }

    public static void Guardar(Cliente c)
    {
        if (c.Id == 0)
        {
            c.Id = Convert.ToInt32(Db.Escalar(
                "INSERT INTO clientes (nombre, telefono, direccion, limite_credito, observaciones) " +
                "VALUES (@n, @t, @d, @l, @o); SELECT LAST_INSERT_ID();",
                ("@n", c.Nombre), ("@t", c.Telefono), ("@d", c.Direccion), ("@l", c.LimiteCredito), ("@o", c.Observaciones)));
        }
        else
        {
            Db.Ejecutar("UPDATE clientes SET nombre = @n, telefono = @t, direccion = @d, limite_credito = @l, observaciones = @o WHERE id = @id",
                ("@n", c.Nombre), ("@t", c.Telefono), ("@d", c.Direccion), ("@l", c.LimiteCredito), ("@o", c.Observaciones), ("@id", c.Id));
        }
    }

    public static void Eliminar(int id) => Db.Ejecutar("UPDATE clientes SET activo = 0 WHERE id = @id", ("@id", id));

    public static decimal Saldo(int clienteId) => Db.ADecimal(Db.Escalar(
        "SELECT " + SqlSaldo + " FROM clientes c WHERE c.id = @id", ("@id", clienteId)));

    public static void RegistrarPago(int clienteId, decimal monto, string formaPago, string observacion) =>
        Db.Ejecutar("INSERT INTO pagos_clientes (cliente_id, monto, forma_pago, usuario_id, observacion) VALUES (@c, @m, @f, @u, @o)",
            ("@c", clienteId), ("@m", monto), ("@f", formaPago), ("@u", Sesion.Usuario.Id), ("@o", observacion));

    /// <summary>Productos llevados en cuenta corriente entre dos fechas (inclusive).</summary>
    public static List<LineaCuenta> Lineas(int clienteId, DateTime desde, DateTime hasta)
    {
        var lista = new List<LineaCuenta>();
        var t = Db.Tabla(
            "SELECT v.id, v.fecha, d.descripcion, d.cantidad, d.unidad, d.precio_unitario, d.subtotal " +
            "FROM ventas v JOIN venta_detalle d ON d.venta_id = v.id " +
            "WHERE v.cliente_id = @c AND v.forma_pago = 'CUENTA_CORRIENTE' AND v.estado = 'ACTIVA' " +
            "AND v.fecha >= @d AND v.fecha < @h ORDER BY v.fecha, d.id",
            ("@c", clienteId), ("@d", desde.Date), ("@h", hasta.Date.AddDays(1)));
        foreach (DataRow r in t.Rows)
            lista.Add(new LineaCuenta
            {
                VentaId = r.Int("id"),
                FechaHora = r.Fecha("fecha"),
                Producto = r.Str("descripcion"),
                Cantidad = r.Dec("cantidad"),
                Unidad = r.Str("unidad"),
                PrecioUnitario = r.Dec("precio_unitario"),
                Subtotal = r.Dec("subtotal")
            });
        return lista;
    }

    /// <summary>Último pago registrado del cliente (null si nunca pagó).</summary>
    public static PagoCuenta UltimoPago(int clienteId)
    {
        var t = Db.Tabla("SELECT fecha, monto, forma_pago, observacion FROM pagos_clientes " +
                         "WHERE cliente_id = @c ORDER BY id DESC LIMIT 1", ("@c", clienteId));
        if (t.Rows.Count == 0) return null;
        var r = t.Rows[0];
        return new PagoCuenta
        {
            FechaHora = r.Fecha("fecha"),
            Monto = r.Dec("monto"),
            FormaPago = r.Str("forma_pago"),
            Observacion = r.Str("observacion")
        };
    }

    public static List<PagoCuenta> Pagos(int clienteId, DateTime desde, DateTime hasta)
    {
        var lista = new List<PagoCuenta>();
        var t = Db.Tabla(
            "SELECT fecha, monto, forma_pago, observacion FROM pagos_clientes " +
            "WHERE cliente_id = @c AND fecha >= @d AND fecha < @h ORDER BY fecha",
            ("@c", clienteId), ("@d", desde.Date), ("@h", hasta.Date.AddDays(1)));
        foreach (DataRow r in t.Rows)
            lista.Add(new PagoCuenta
            {
                FechaHora = r.Fecha("fecha"),
                Monto = r.Dec("monto"),
                FormaPago = r.Str("forma_pago"),
                Observacion = r.Str("observacion")
            });
        return lista;
    }

    /// <summary>
    /// Fecha de la compra más antigua que todavía forma parte de la deuda
    /// (se asume que los pagos cancelan primero las compras más viejas).
    /// Devuelve null si el cliente no debe nada.
    /// </summary>
    public static DateTime? FechaInicioDeuda(int clienteId)
    {
        decimal saldo = Saldo(clienteId);
        if (saldo <= 0) return null;

        var t = Db.Tabla(
            "SELECT fecha, total FROM ventas WHERE cliente_id = @c AND forma_pago = 'CUENTA_CORRIENTE' " +
            "AND estado = 'ACTIVA' ORDER BY fecha DESC", ("@c", clienteId));
        decimal acumulado = 0;
        DateTime? desde = null;
        foreach (DataRow r in t.Rows)
        {
            acumulado += r.Dec("total");
            desde = r.Fecha("fecha");
            if (acumulado >= saldo) break;
        }
        return desde;
    }
}
