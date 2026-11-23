namespace KioscoLaFamilia.Datos;

public static class VentaRepo
{
    /// <summary>
    /// Registra la venta completa en una transacción: cabecera, detalle,
    /// descuento de stock y movimientos. Devuelve el número de venta.
    /// </summary>
    public static int Registrar(int? clienteId, string formaPago, decimal? pagadoCon, IList<ItemCarrito> items)
    {
        if (items == null || items.Count == 0) throw new InvalidOperationException("La venta no tiene productos.");

        decimal total = items.Sum(i => i.Subtotal);
        decimal costo = Math.Round(items.Sum(i => i.CostoUnitario * i.Cantidad), 2);
        decimal? vuelto = pagadoCon.HasValue ? pagadoCon.Value - total : null;

        using var c = Db.Abrir();
        using var tx = c.BeginTransaction();
        try
        {
            int ventaId = (int)Db.Insertar(c, tx,
                "INSERT INTO ventas (fecha, usuario_id, cliente_id, total, costo_total, forma_pago, pagado_con, vuelto) " +
                "VALUES (NOW(), @u, @c, @t, @ct, @f, @pc, @v)",
                ("@u", Sesion.Usuario.Id), ("@c", clienteId), ("@t", total), ("@ct", costo),
                ("@f", formaPago), ("@pc", pagadoCon), ("@v", vuelto));

            foreach (var i in items)
            {
                Db.Ejecutar(c, tx,
                    "INSERT INTO venta_detalle (venta_id, producto_id, descripcion, cantidad, unidad, precio_unitario, costo_unitario, subtotal) " +
                    "VALUES (@v, @p, @d, @c, @un, @pu, @cu, @s)",
                    ("@v", ventaId), ("@p", i.ProductoId), ("@d", i.Descripcion), ("@c", i.Cantidad), ("@un", i.Unidad),
                    ("@pu", i.PrecioUnitario), ("@cu", i.CostoUnitario), ("@s", i.Subtotal));

                // Las presentaciones de helado (y lo que no controla stock) no descuentan
                if (!i.ControlaStock) continue;
                Db.Ejecutar(c, tx, "UPDATE productos SET stock = stock - @c WHERE id = @p",
                    ("@c", i.Cantidad), ("@p", i.ProductoId));

                ProductoRepo.RegistrarMovimiento(c, tx, i.ProductoId, "VENTA", -i.Cantidad, "Venta N° " + ventaId);
            }

            tx.Commit();
            return ventaId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>Anula una venta y devuelve el stock.</summary>
    public static void Anular(int ventaId, string motivo)
    {
        using var c = Db.Abrir();
        using var tx = c.BeginTransaction();
        try
        {
            var estado = Convert.ToString(Db.Escalar(c, tx, "SELECT estado FROM ventas WHERE id = @id FOR UPDATE", ("@id", ventaId)));
            if (estado != "ACTIVA") throw new InvalidOperationException("La venta ya estaba anulada.");

            var detalle = Db.Tabla(c, tx, "SELECT d.producto_id, d.cantidad FROM venta_detalle d JOIN productos p ON p.id = d.producto_id " +
                "WHERE d.venta_id = @id AND p.controla_stock = 1", ("@id", ventaId));
            foreach (DataRow r in detalle.Rows)
            {
                Db.Ejecutar(c, tx, "UPDATE productos SET stock = stock + @c WHERE id = @p",
                    ("@c", r.Dec("cantidad")), ("@p", r.Int("producto_id")));
                ProductoRepo.RegistrarMovimiento(c, tx, r.Int("producto_id"), "ANULACION", r.Dec("cantidad"),
                    $"Anulación venta N° {ventaId}: {motivo}");
            }
            Db.Ejecutar(c, tx, "UPDATE ventas SET estado = 'ANULADA' WHERE id = @id", ("@id", ventaId));
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public static DataTable Listar(DateTime desde, DateTime hasta, bool incluirAnuladas) => Db.Tabla(
        "SELECT v.id AS `N°`, v.fecha AS `Fecha`, u.usuario AS `Cajero`, IFNULL(c.nombre, 'Consumidor final') AS `Cliente`, " +
        "CASE v.forma_pago WHEN 'EFECTIVO' THEN 'Efectivo' WHEN 'TRANSFERENCIA' THEN 'Transferencia' " +
        "WHEN 'DEBITO' THEN 'Débito' WHEN 'CREDITO' THEN 'Crédito' ELSE 'Cuenta corriente' END AS `Forma de pago`, " +
        "v.total AS `Total`, v.estado AS `Estado` " +
        "FROM ventas v JOIN usuarios u ON u.id = v.usuario_id LEFT JOIN clientes c ON c.id = v.cliente_id " +
        "WHERE v.fecha >= @d AND v.fecha < @h" + (incluirAnuladas ? "" : " AND v.estado = 'ACTIVA'") +
        " ORDER BY v.id DESC",
        ("@d", desde.Date), ("@h", hasta.Date.AddDays(1)));

    public static DataTable Detalle(int ventaId) => Db.Tabla(
        "SELECT descripcion AS `Producto`, " +
        "IF(unidad = 'KG', CONCAT(TRIM(TRAILING '.' FROM TRIM(TRAILING '0' FROM cantidad)), ' kg'), " +
        "TRIM(TRAILING '.' FROM TRIM(TRAILING '0' FROM cantidad))) AS `Cant.`, precio_unitario AS `P. unitario`, subtotal AS `Subtotal` " +
        "FROM venta_detalle WHERE venta_id = @id ORDER BY id", ("@id", ventaId));

    public static DataRow Cabecera(int ventaId)
    {
        var t = Db.Tabla(
            "SELECT v.*, u.usuario, u.nombre AS cajero, c.nombre AS cliente, c.telefono AS cliente_telefono " +
            "FROM ventas v JOIN usuarios u ON u.id = v.usuario_id LEFT JOIN clientes c ON c.id = v.cliente_id WHERE v.id = @id",
            ("@id", ventaId));
        return t.Rows.Count == 0 ? null : t.Rows[0];
    }

    /// <summary>Totales por forma de pago para el cierre de caja.</summary>
    public static DataTable ResumenPorFormaPago(DateTime desde, DateTime hasta) => Db.Tabla(
        "SELECT forma_pago, COUNT(*) AS cantidad, SUM(total) AS total FROM ventas " +
        "WHERE estado = 'ACTIVA' AND fecha >= @d AND fecha < @h GROUP BY forma_pago ORDER BY forma_pago",
        ("@d", desde.Date), ("@h", hasta.Date.AddDays(1)));

    /// <summary>Cobros de cuentas corrientes en el período, agrupados por forma de pago.</summary>
    public static DataTable CobrosCuentas(DateTime desde, DateTime hasta) => Db.Tabla(
        "SELECT forma_pago, COUNT(*) AS cantidad, SUM(monto) AS total FROM pagos_clientes " +
        "WHERE fecha >= @d AND fecha < @h GROUP BY forma_pago",
        ("@d", desde.Date), ("@h", hasta.Date.AddDays(1)));
}
