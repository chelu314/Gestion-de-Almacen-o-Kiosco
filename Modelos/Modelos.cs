namespace KioscoLaFamilia.Modelos;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; }
    public string Nombre { get; set; }
    public string Rol { get; set; }
    public bool EsAdmin => Rol == "admin";
}

public class Producto
{
    public int Id { get; set; }
    public string CodigoBarras { get; set; }
    public string Nombre { get; set; }
    public string Categoria { get; set; } = "General";
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public string Unidad { get; set; } = "UNIDAD";   // UNIDAD o KG
    public decimal Stock { get; set; }
    public decimal StockMinimo { get; set; } = 5;
    public bool ControlaStock { get; set; } = true;
    public bool VentaRapida { get; set; }
    public bool Activo { get; set; } = true;

    public bool PorPeso => Unidad == "KG";
    public bool StockBajo => ControlaStock && StockMinimo > 0 && Stock <= StockMinimo;   // mínimo 0 = no avisar
    public string StockTexto => Cantidades.Formato(Stock, Unidad);
    public override string ToString() => Nombre;
}

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public decimal LimiteCredito { get; set; }
    public string Observaciones { get; set; }
    public override string ToString() => Nombre;
}

/// <summary>Renglón del carrito en el punto de venta.</summary>
public class ItemCarrito
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; }
    public string Descripcion { get; set; }
    public decimal Cantidad { get; set; }
    public string Unidad { get; set; } = "UNIDAD";
    public string CantidadTexto => Cantidades.Formato(Cantidad, Unidad);
    public decimal PrecioUnitario { get; set; }
    public decimal CostoUnitario { get; set; }
    public bool ControlaStock { get; set; } = true;
    public decimal Subtotal => Math.Round(Cantidad * PrecioUnitario, 2);
}

/// <summary>Producto llevado por un cliente en cuenta corriente.</summary>
public class LineaCuenta
{
    public int VentaId { get; set; }
    public DateTime FechaHora { get; set; }
    public string Fecha => FechaHora.ToString("dd/MM/yyyy");
    public string Hora => FechaHora.ToString("HH:mm");
    public string Producto { get; set; }
    public decimal Cantidad { get; set; }
    public string Unidad { get; set; } = "UNIDAD";
    public string CantidadTexto => Cantidades.Formato(Cantidad, Unidad);
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class PagoCuenta
{
    public DateTime FechaHora { get; set; }
    public string Fecha => FechaHora.ToString("dd/MM/yyyy HH:mm");
    public decimal Monto { get; set; }
    public string FormaPago { get; set; }
    public string Observacion { get; set; }
}

public static class Sesion
{
    public static Usuario Usuario { get; set; }
    public static bool EsAdmin => Usuario != null && Usuario.EsAdmin;
}

public static class FormasPago
{
    public static readonly (string Codigo, string Nombre)[] Todas =
    {
        ("EFECTIVO", "Efectivo"),
        ("TRANSFERENCIA", "Transferencia / QR"),
        ("DEBITO", "Tarjeta de débito"),
        ("CREDITO", "Tarjeta de crédito"),
        ("CUENTA_CORRIENTE", "Cuenta corriente (fiado)")
    };

    public static string Nombre(string codigo)
    {
        foreach (var f in Todas)
            if (f.Codigo == codigo) return f.Nombre;
        return codigo;
    }
}

/// <summary>Formato de cantidades: unidades enteras o kilos con gramos.</summary>
public static class Cantidades
{
    public static string Formato(decimal cantidad, string unidad)
    {
        if (unidad == "KG")
            return cantidad < 1 && cantidad > -1
                ? (cantidad * 1000).ToString("0") + " g"
                : cantidad.ToString("0.###") + " kg";
        return cantidad.ToString("0.###");
    }
}

public static class Categorias
{
    /// <summary>Tipos de producto del negocio (se pueden agregar más desde el alta de producto).</summary>
    public static readonly string[] Predeterminadas =
        { "Almacén", "Bebidas", "Golosinas", "Helados", "Lácteos", "Limpieza", "Panes", "Perfumería", "Snacks" };
}
