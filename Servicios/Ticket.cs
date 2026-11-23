namespace KioscoLaFamilia.Servicios;

/// <summary>
/// Arma los textos de tickets con ancho fijo (32 caracteres para impresora de 58 mm,
/// 42-48 para 80 mm). Se usan para imprimir y para enviar por WhatsApp.
/// </summary>
public static class Ticket
{
    private static int Ancho => Math.Clamp(ConfigRepo.ObtenerInt("ancho_ticket", 32), 24, 64);

    private static string Separador(char c = '-') => new string(c, Ancho);

    private static string Cortar(string t, int max) => t == null ? "" : (t.Length > max ? t.Substring(0, max) : t);

    private static string Centro(string t)
    {
        t = Cortar(t, Ancho);
        return new string(' ', Math.Max(0, (Ancho - t.Length) / 2)) + t;
    }

    /// <summary>Texto a la izquierda y valor alineado a la derecha.</summary>
    private static string Columnas(string izq, string der)
    {
        der ??= "";
        int espacio = Math.Max(1, Ancho - der.Length);
        izq = Cortar(izq ?? "", espacio - 1);
        return izq.PadRight(espacio) + der;
    }

    public static string M(decimal d) => d.ToString("C2", CultureInfo.CurrentCulture);

    private static void Encabezado(StringBuilder sb, string titulo)
    {
        sb.AppendLine(Centro(ConfigRepo.Obtener("nombre_negocio", "Almacén La Tutti").ToUpper()));
        var dir = ConfigRepo.Obtener("direccion");
        var tel = ConfigRepo.Obtener("telefono");

        // Si la dirección es más larga que el ticket, se parte en varios renglones
        
        if (dir != "")
            foreach (var linea in Partir(dir, Ancho)) sb.AppendLine(Centro(linea));
        if (tel != "") sb.AppendLine(Centro("Tel: " + tel));
        sb.AppendLine(Separador('='));
        sb.AppendLine(Centro(titulo));
        sb.AppendLine(Separador());
    }

    private static void Pie(StringBuilder sb)
    {
        sb.AppendLine(Separador());
        var pie = ConfigRepo.Obtener("pie_ticket", "¡Gracias por su compra!");

        // Lo mismo con el mensaje al pie: si es largo, va en varios renglones.
        if (pie != "")
            foreach (var linea in Partir(pie, Ancho)) sb.AppendLine(Centro(linea));
    }

    private static void Renglon(StringBuilder sb, string producto, decimal cantidad, string unidad, decimal precio, decimal subtotal)
    {
        foreach (var parte in Partir(producto, Ancho)) sb.AppendLine(parte);
        string detalle = unidad == "KG"
            ? $"  {Cantidades.Formato(cantidad, unidad)} x {M(precio)}/kg"
            : $"  {cantidad:0.###} x {M(precio)}";
        sb.AppendLine(Columnas(detalle, M(subtotal)));
    }

    /// <summary>
    /// Parte un texto largo en renglones de "ancho" letras SIN cortar palabras:
    /// va sumando palabra por palabra y cuando la próxima no entra, empieza otro renglón.
    /// </summary>
    private static IEnumerable<string> Partir(string texto, int ancho)
    {
        texto ??= "";
        string renglon = "";
        // Split(' ') separa el texto en palabras usando los espacios
        foreach (var palabra in texto.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            // ¿Entra la palabra en el renglón actual? (+1 por el espacio)
            if (renglon.Length + 1 + palabra.Length <= ancho || renglon == "")
            {
                renglon = renglon == "" ? palabra : renglon + " " + palabra;
            }
            else
            {
                yield return renglon;   // el renglón está lleno: lo devolvemos
                renglon = palabra;      // y la palabra empieza el siguiente
            }
            // Una palabra sola más larga que el ticket (rarísimo): se corta a la fuerza
            while (renglon.Length > ancho)
            {
                yield return renglon.Substring(0, ancho);
                renglon = renglon.Substring(ancho);
            }
        }
        if (renglon != "") yield return renglon;   // el último renglón que quedó
    }

    // ===================================================================

    public static string DeVenta(int ventaId)
    {
        var v = VentaRepo.Cabecera(ventaId);
        if (v == null) return "Venta no encontrada.";
        var detalle = Db.Tabla("SELECT * FROM venta_detalle WHERE venta_id = @id ORDER BY id", ("@id", ventaId));

        var sb = new StringBuilder();
        Encabezado(sb, "TICKET N° " + ventaId.ToString("D6"));
        sb.AppendLine("Fecha:  " + v.Fecha("fecha").ToString("dd/MM/yyyy HH:mm"));
        sb.AppendLine("Cajero: " + v.Str("usuario"));
        if (v.Str("cliente") != "") sb.AppendLine("Cliente: " + v.Str("cliente"));
        if (v.Str("estado") == "ANULADA") sb.AppendLine(Centro("*** VENTA ANULADA ***"));
        sb.AppendLine(Separador());

        int unidades = 0;
        foreach (DataRow r in detalle.Rows)
        {
            Renglon(sb, r.Str("descripcion"), r.Dec("cantidad"), r.Str("unidad"), r.Dec("precio_unitario"), r.Dec("subtotal"));
            unidades += r.Str("unidad") == "KG" ? 1 : (int)Math.Ceiling(r.Dec("cantidad"));
        }

        sb.AppendLine(Separador());
        sb.AppendLine(Columnas($"Artículos: {unidades}", ""));
        sb.AppendLine(Columnas("TOTAL:", M(v.Dec("total"))));
        sb.AppendLine(Columnas("Pago:", FormasPago.Nombre(v.Str("forma_pago"))));
        if (v.Str("forma_pago") == "EFECTIVO" && v.Dec("pagado_con") > 0)
        {
            sb.AppendLine(Columnas("Pagó con:", M(v.Dec("pagado_con"))));
            sb.AppendLine(Columnas("Vuelto:", M(v.Dec("vuelto"))));
        }
        if (v.Str("forma_pago") == "CUENTA_CORRIENTE" && v.Int("cliente_id") > 0)
            sb.AppendLine(Columnas("Saldo en cuenta:", M(ClienteRepo.Saldo(v.Int("cliente_id")))));
        Pie(sb);
        return sb.ToString();
    }

    /// <summary>Estado de cuenta del cliente: productos llevados, pagos y saldo.</summary>
    public static string EstadoCuenta(Cliente c, DateTime desde, DateTime hasta,
                                      List<LineaCuenta> lineas, List<PagoCuenta> pagos, decimal saldo)
    {
        var sb = new StringBuilder();
        Encabezado(sb, "ESTADO DE CUENTA");
        sb.AppendLine("Cliente: " + c.Nombre);
        sb.AppendLine($"Período: {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}");
        sb.AppendLine("Emitido: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        sb.AppendLine(Separador());

        if (lineas.Count == 0)
        {
            sb.AppendLine("Sin compras en el período.");
        }
        else
        {
            sb.AppendLine("PRODUCTOS LLEVADOS");
            foreach (var compra in lineas.GroupBy(l => l.VentaId))
            {
                var primera = compra.First();
                sb.AppendLine();
                sb.AppendLine($">> {primera.FechaHora:dd/MM/yyyy} {primera.FechaHora:HH:mm} hs");
                foreach (var l in compra) Renglon(sb, l.Producto, l.Cantidad, l.Unidad, l.PrecioUnitario, l.Subtotal);
                sb.AppendLine(Columnas("   Total compra:", M(compra.Sum(x => x.Subtotal))));
            }
        }

        decimal totalCompras = lineas.Sum(l => l.Subtotal);
        decimal totalPagos = pagos.Sum(p => p.Monto);

        if (pagos.Count > 0)
        {
            sb.AppendLine(Separador());
            sb.AppendLine("PAGOS RECIBIDOS");
            foreach (var p in pagos)
                sb.AppendLine(Columnas(p.FechaHora.ToString("dd/MM/yy HH:mm"), M(p.Monto)));
        }

        sb.AppendLine(Separador('='));
        sb.AppendLine(Columnas("Compras del período:", M(totalCompras)));
        sb.AppendLine(Columnas("Pagos del período:", M(totalPagos)));
        sb.AppendLine(Separador());
        sb.AppendLine(Columnas("SALDO A PAGAR:", M(Math.Max(0, saldo))));
        if (saldo < 0) sb.AppendLine(Columnas("Saldo a favor:", M(-saldo)));
        Pie(sb);
        return sb.ToString();
    }

    public static string ReciboPago(Cliente c, decimal monto, string formaPago, decimal saldoRestante)
    {
        var sb = new StringBuilder();
        Encabezado(sb, "RECIBO DE PAGO");
        sb.AppendLine("Fecha:   " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        sb.AppendLine("Cliente: " + c.Nombre);
        sb.AppendLine(Separador());
        sb.AppendLine(Columnas("Monto recibido:", M(monto)));
        sb.AppendLine(Columnas("Forma de pago:", FormasPago.Nombre(formaPago)));
        sb.AppendLine(Separador());
        sb.AppendLine(Columnas("Saldo pendiente:", M(Math.Max(0, saldoRestante))));
        if (saldoRestante < 0) sb.AppendLine(Columnas("Saldo a favor:", M(-saldoRestante)));
        Pie(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Comprobante para mandarle al cliente cuando paga: monto, saldo anterior y saldo actual.
    /// Si la deuda quedó en cero dice "DEUDA CANCELADA".
    /// </summary>
    public static string ComprobantePago(Cliente c, PagoCuenta pago, decimal saldoAnterior, decimal saldoActual)
    {
        var sb = new StringBuilder();
        Encabezado(sb, "COMPROBANTE DE PAGO");
        sb.AppendLine("Fecha:   " + pago.FechaHora.ToString("dd/MM/yyyy HH:mm"));
        sb.AppendLine("Cliente: " + c.Nombre);
        sb.AppendLine(Separador());
        sb.AppendLine(Columnas("Deuda anterior:", M(Math.Max(0, saldoAnterior))));
        sb.AppendLine(Columnas("Pagó:", M(pago.Monto)));
        sb.AppendLine(Columnas("Forma de pago:", FormasPago.Nombre(pago.FormaPago)));
        sb.AppendLine(Separador());
        if (saldoActual <= 0)
        {
            sb.AppendLine(Centro("*** DEUDA CANCELADA ***"));
            sb.AppendLine(Centro("Su cuenta quedó al día."));
            if (saldoActual < 0) sb.AppendLine(Columnas("Saldo a favor:", M(-saldoActual)));
        }
        else
        {
            sb.AppendLine(Columnas("SALDO PENDIENTE:", M(saldoActual)));
        }
        sb.AppendLine();
        sb.AppendLine(Centro("¡Gracias por su pago!"));
        Pie(sb);
        return sb.ToString();
    }

    public static string CierreCaja(DateTime desde, DateTime hasta)
    {
        var ventas = VentaRepo.ResumenPorFormaPago(desde, hasta);
        var cobros = VentaRepo.CobrosCuentas(desde, hasta);

        var sb = new StringBuilder();
        Encabezado(sb, "CIERRE DE CAJA");
        sb.AppendLine(desde.Date == hasta.Date
            ? $"Día: {desde:dd/MM/yyyy}"
            : $"Del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}");
        sb.AppendLine("Emitido: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + " por " + Sesion.Usuario.NombreUsuario);
        sb.AppendLine(Separador());
        sb.AppendLine("VENTAS");

        decimal totalVentas = 0, efectivo = 0;
        int cantidad = 0;
        foreach (DataRow r in ventas.Rows)
        {
            sb.AppendLine(Columnas($"{FormasPago.Nombre(r.Str("forma_pago"))} ({r.Int("cantidad")})", M(r.Dec("total"))));
            totalVentas += r.Dec("total");
            cantidad += r.Int("cantidad");
            if (r.Str("forma_pago") == "EFECTIVO") efectivo += r.Dec("total");
        }
        sb.AppendLine(Columnas($"Total vendido ({cantidad})", M(totalVentas)));

        decimal totalCobros = 0;
        if (cobros.Rows.Count > 0)
        {
            sb.AppendLine(Separador());
            sb.AppendLine("COBROS DE CUENTAS CORRIENTES");
            foreach (DataRow r in cobros.Rows)
            {
                sb.AppendLine(Columnas($"{FormasPago.Nombre(r.Str("forma_pago"))} ({r.Int("cantidad")})", M(r.Dec("total"))));
                totalCobros += r.Dec("total");
                if (r.Str("forma_pago") == "EFECTIVO") efectivo += r.Dec("total");
            }
        }

        sb.AppendLine(Separador('='));
        sb.AppendLine(Columnas("EFECTIVO EN CAJA:", M(efectivo)));
        sb.AppendLine(Columnas("Ingresos totales:", M(totalVentas - SumaFiado(ventas) + totalCobros)));
        sb.AppendLine(Columnas("Vendido en fiado:", M(SumaFiado(ventas))));
        sb.AppendLine(Separador());
        return sb.ToString();
    }

    private static decimal SumaFiado(DataTable ventas)
    {
        decimal s = 0;
        foreach (DataRow r in ventas.Rows)
            if (r.Str("forma_pago") == "CUENTA_CORRIENTE") s += r.Dec("total");
        return s;
    }

    /// <summary>
    /// Pedido manual para mandar por WhatsApp. No usa columnas de ancho fijo
    /// porque se lee en el celular: un renglón por producto con su cantidad.
    /// </summary>
    public static string PedidoManual(string proveedor, IEnumerable<(string Producto, string Cantidad)> items)
    {
        var sb = new StringBuilder();
        // Nombre y dirección salen de Configuración: si un día los cambiás, el mensaje se actualiza solo
        string negocio = ConfigRepo.Obtener("nombre_negocio", "Almacén La Tutti");
        string direccion = ConfigRepo.Obtener("direccion");

        // Si hay dirección cargada la agregamos; si no, el mensaje queda sin ella
        string donde = direccion == "" ? negocio : negocio + ", dirección " + direccion;
        sb.AppendLine("Hola " + proveedor + ", ¿me podés enviar a " + donde + " lo siguiente?");
        sb.AppendLine("Fecha: " + DateTime.Now.ToString("dd/MM/yyyy"));
        sb.AppendLine();
        foreach (var (producto, cantidad) in items)
            sb.AppendLine("• " + cantidad + " - " + producto);   // ej: "• 2 cajas - Leche entera"
        sb.AppendLine();
        sb.AppendLine("¡Gracias!");
        return sb.ToString();
    }
    /// <summary>Lista de pedido para el proveedor con los productos en falta.</summary>
    public static string PedidoProveedor(IEnumerable<(Producto Producto, decimal Cantidad)> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("PEDIDO - " + ConfigRepo.Obtener("nombre_negocio", "Almacén La Tutti"));
        sb.AppendLine("Fecha: " + DateTime.Now.ToString("dd/MM/yyyy"));
        sb.AppendLine(Separador());
        foreach (var (p, cant) in items)
            sb.AppendLine(Columnas(p.Nombre, p.PorPeso ? Cantidades.Formato(cant, "KG") : cant.ToString("0") + " u."));
        sb.AppendLine(Separador());
        return sb.ToString();
    }
}
