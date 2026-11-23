namespace KioscoLaFamilia.Datos;

public class ResumenMes
{
    public int Mes { get; set; }
    public string NombreMes { get; set; }
    public int CantidadVentas { get; set; }
    public decimal Total { get; set; }
    public decimal Costo { get; set; }
    public decimal Ganancia => Total - Costo;
    public decimal MargenPorc => Total == 0 ? 0 : Math.Round(Ganancia / Total * 100, 1);
}

public static class ReporteRepo
{
    public static List<int> AniosConVentas()
    {
        var lista = new List<int>();
        foreach (DataRow r in Db.Tabla("SELECT DISTINCT YEAR(fecha) AS anio FROM ventas ORDER BY anio DESC").Rows)
            lista.Add(r.Int("anio"));
        if (!lista.Contains(DateTime.Today.Year)) lista.Insert(0, DateTime.Today.Year);
        return lista;
    }

    /// <summary>Los 12 meses del año (incluso los que no tuvieron ventas).</summary>
    public static List<ResumenMes> VentasPorMes(int anio)
    {
        var meses = new List<ResumenMes>();
        var formato = CultureInfo.CurrentCulture.DateTimeFormat;
        for (int m = 1; m <= 12; m++)
        {
            var nombre = formato.GetMonthName(m);
            meses.Add(new ResumenMes { Mes = m, NombreMes = char.ToUpper(nombre[0]) + nombre.Substring(1) });
        }

        var t = Db.Tabla(
            "SELECT MONTH(fecha) AS mes, COUNT(*) AS cantidad, SUM(total) AS total, SUM(costo_total) AS costo " +
            "FROM ventas WHERE estado = 'ACTIVA' AND fecha >= @d AND fecha < @h GROUP BY MONTH(fecha)",
            ("@d", new DateTime(anio, 1, 1)), ("@h", new DateTime(anio + 1, 1, 1)));
        foreach (DataRow r in t.Rows)
        {
            var mes = meses[r.Int("mes") - 1];
            mes.CantidadVentas = r.Int("cantidad");
            mes.Total = r.Dec("total");
            mes.Costo = r.Dec("costo");
        }
        return meses;
    }

    public static DataTable ProductosMasVendidos(DateTime desde, DateTime hasta, int limite = 30) => Db.Tabla(
        "SELECT d.descripcion AS `Producto`, SUM(d.cantidad) AS `Unidades`, SUM(d.subtotal) AS `Vendido`, " +
        "SUM(d.subtotal - d.costo_unitario * d.cantidad) AS `Ganancia` " +
        "FROM venta_detalle d JOIN ventas v ON v.id = d.venta_id " +
        "WHERE v.estado = 'ACTIVA' AND v.fecha >= @d AND v.fecha < @h " +
        "GROUP BY d.producto_id, d.descripcion ORDER BY `Unidades` DESC LIMIT @l",
        ("@d", desde.Date), ("@h", hasta.Date.AddDays(1)), ("@l", limite));

    /// <summary>Ventas y ganancia agrupadas por tipo de producto (categoría).</summary>
    public static DataTable VentasPorCategoria(DateTime desde, DateTime hasta) => Db.Tabla(
        "SELECT p.categoria AS `Categoría`, COUNT(DISTINCT v.id) AS `Ventas`, SUM(d.subtotal) AS `Vendido`, " +
        "SUM(d.subtotal - d.costo_unitario * d.cantidad) AS `Ganancia`, " +
        "ROUND(SUM(d.subtotal) * 100 / NULLIF((SELECT SUM(v2.total) FROM ventas v2 WHERE v2.estado = 'ACTIVA' " +
        "  AND v2.fecha >= @d AND v2.fecha < @h), 0), 1) AS `% del total` " +
        "FROM venta_detalle d JOIN ventas v ON v.id = d.venta_id JOIN productos p ON p.id = d.producto_id " +
        "WHERE v.estado = 'ACTIVA' AND v.fecha >= @d AND v.fecha < @h " +
        "GROUP BY p.categoria ORDER BY `Vendido` DESC",
        ("@d", desde.Date), ("@h", hasta.Date.AddDays(1)));

    public static DataTable ProductosSinVentas(int dias) => Db.Tabla(
        "SELECT p.codigo_barras AS `Código`, p.nombre AS `Producto`, p.categoria AS `Categoría`, p.stock AS `Stock`, " +
        "(SELECT MAX(v.fecha) FROM venta_detalle d JOIN ventas v ON v.id = d.venta_id " +
        "  WHERE d.producto_id = p.id AND v.estado = 'ACTIVA') AS `Última venta` " +
        "FROM productos p WHERE p.activo = 1 AND p.controla_stock = 1 AND NOT EXISTS (" +
        "  SELECT 1 FROM venta_detalle d JOIN ventas v ON v.id = d.venta_id " +
        "  WHERE d.producto_id = p.id AND v.estado = 'ACTIVA' AND v.fecha >= @d) " +
        "ORDER BY p.nombre",
        ("@d", DateTime.Today.AddDays(-dias)));
}
