namespace KioscoLaFamilia.Formularios;

/// <summary>
/// Estado de cuenta: productos llevados con fecha y hora, pagos y saldo.
/// Permite registrar pagos y enviar el resumen por WhatsApp.
/// </summary>
public class FrmCuentaCliente : Form
{
    private readonly int clienteId;
    private Cliente cliente;
    private decimal saldo;
    private List<LineaCuenta> lineas = new List<LineaCuenta>();
    private List<PagoCuenta> pagos = new List<PagoCuenta>();

    private readonly Label lblCliente;
    private readonly Label lblSaldo;
    private readonly Label lblTotales;
    private readonly SelectorPeriodo periodo;
    private readonly DataGridView gridProductos;
    private readonly DataGridView gridPagos;

    public FrmCuentaCliente(int clienteId)
    {
        this.clienteId = clienteId;
        UI.PrepararForm(this, "Cuenta del cliente", 1000, 680);

        // ----- Encabezado con cliente y saldo -----
        var cabecera = new Panel { Dock = DockStyle.Top, Height = 132, BackColor = Color.White };
        lblCliente = new Label { AutoSize = true, Font = new Font("Segoe UI", 15f, FontStyle.Bold), Location = new Point(14, 8) };
        lblSaldo = new Label { AutoSize = true, Font = new Font("Segoe UI", 18f, FontStyle.Bold), Location = new Point(14, 40) };

        // "Lo que debe" muestra solo las compras que forman la deuda actual; se aplica solo al elegir
        periodo = new SelectorPeriodo("Lo que debe", ("Lo que debe", () =>
        {
            var inicio = ClienteRepo.FechaInicioDeuda(clienteId) ?? DateTime.Today;
            return (inicio.Date, DateTime.Today);
        }))
        { Location = new Point(10, 88) };
        periodo.Cambio += (s, e) => CargarMovimientos();
        cabecera.Controls.AddRange(new Control[] { lblCliente, lblSaldo, periodo });

        // ----- Pestañas: productos llevados / pagos -----
        var tabs = new TabControl { Font = new Font("Segoe UI", 10f) };
        var tabProductos = new TabPage("Productos llevados");
        var tabPagos = new TabPage("Pagos realizados");
        gridProductos = UI.Grilla();
        UI.Configurar(gridProductos,
            ocultas: new[] { "VentaId", "FechaHora", "Cantidad", "Unidad" },
            moneda: new[] { "PrecioUnitario", "Subtotal" },
            titulos: new Dictionary<string, string> { ["PrecioUnitario"] = "P. unitario", ["CantidadTexto"] = "Cant." },
            extra: g =>
            {
                if (!g.Columns.Contains("Producto")) return;
                g.Columns["Fecha"].DisplayIndex = 0;
                g.Columns["Hora"].DisplayIndex = 1;
                g.Columns["Producto"].FillWeight = 250;
            });
        gridPagos = UI.Grilla();
        UI.Configurar(gridPagos, ocultas: new[] { "FechaHora" }, moneda: new[] { "Monto" },
            titulos: new Dictionary<string, string> { ["FormaPago"] = "Forma de pago", ["Observacion"] = "Observación" },
            extra: g => { if (g.Columns.Contains("Fecha")) g.Columns["Fecha"].DisplayIndex = 0; });
        tabProductos.Controls.Add(gridProductos);
        tabPagos.Controls.Add(gridPagos);
        tabs.TabPages.Add(tabProductos);
        tabs.TabPages.Add(tabPagos);

        // ----- Totales y botones -----
        var pie = new Panel { Dock = DockStyle.Bottom, Height = 96, BackColor = Color.White };
        lblTotales = new Label { AutoSize = true, Location = new Point(12, 6), Font = new Font("Segoe UI", 10.5f, FontStyle.Bold) };
        var barra = new FlowLayoutPanel { Location = new Point(6, 36), Width = 1200, Height = 56, WrapContents = false, Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right };
        // 1) anota un pago  2) recibo del último pago  3) todo lo que llevó y pagó   (se cierra con Esc o la X)
        // Los documentos se abren primero en pantalla, con botones Imprimir y WhatsApp.
        var btnPago = UI.BotonPrincipal("Registrar pago", RegistrarPago, 200, Tema.Exito);
        var btnComprobante = UI.BotonSecundario("Comprobante de pago", () => VerComprobante(this, clienteId), 210);
        var btnResumen = UI.BotonSecundario("Resumen de cuenta", MostrarResumen, 200);
        var ayuda = new ToolTip();
        ayuda.SetToolTip(btnPago, "Anota lo que paga el cliente y ofrece mandarle el comprobante.");
        ayuda.SetToolTip(btnComprobante, "Recibo del último pago (dice DEUDA CANCELADA si quedó en cero). Se puede imprimir o mandar por WhatsApp.");
        ayuda.SetToolTip(btnResumen, "Todo lo que llevó y pagó en el período, con el saldo. Se puede imprimir o mandar por WhatsApp.");
        barra.Controls.AddRange(new Control[] { btnPago, btnComprobante, btnResumen });
        pie.Controls.Add(lblTotales);
        pie.Controls.Add(barra);

        UI.Ensamblar(this, tabs, cabecera, pie);
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

        Load += (s, e) =>
        {
            CargarCliente();
            periodo.Recalcular();
            CargarMovimientos();
        };
    }

    private void CargarCliente()
    {
        cliente = ClienteRepo.ObtenerPorId(clienteId);
        saldo = ClienteRepo.Saldo(clienteId);
        lblCliente.Text = cliente.Nombre + (string.IsNullOrEmpty(cliente.Telefono) ? "" : "   —   Tel: " + cliente.Telefono);
        if (saldo > 0)
        {
            lblSaldo.Text = "DEBE: " + UI.Moneda(saldo);
            lblSaldo.ForeColor = UI.Peligro;
        }
        else
        {
            lblSaldo.Text = saldo < 0 ? "Saldo a favor: " + UI.Moneda(-saldo) : "Sin deuda ✔";
            lblSaldo.ForeColor = UI.Exito;
        }
        if (cliente.LimiteCredito > 0) lblSaldo.Text += $"     (límite {UI.Moneda(cliente.LimiteCredito)})";
    }

    private void CargarMovimientos()
    {
        UI.Intentar(() =>
        {
            lineas = ClienteRepo.Lineas(clienteId, periodo.Desde, periodo.Hasta);
            pagos = ClienteRepo.Pagos(clienteId, periodo.Desde, periodo.Hasta);
            gridProductos.DataSource = lineas;
            gridPagos.DataSource = pagos;
            lblTotales.Text =
                $"Compras del período: {UI.Moneda(lineas.Sum(l => l.Subtotal))} ({lineas.Select(l => l.VentaId).Distinct().Count()} compras)" +
                $"     |     Pagos del período: {UI.Moneda(pagos.Sum(p => p.Monto))}" +
                $"     |     SALDO TOTAL: {UI.Moneda(saldo)}";
        });
    }

    private string TextoResumen() =>
        Ticket.EstadoCuenta(cliente, periodo.Desde, periodo.Hasta, lineas, pagos, saldo);

    private void MostrarResumen()
    {
        using var f = new FrmTicket("Estado de cuenta — " + cliente.Nombre, TextoResumen(), cliente.Telefono);
        f.ShowDialog(this);
    }

    /// <summary>Teléfono del cliente; si no tiene, lo pide (y ofrece guardarlo). Null si se cancela.</summary>
    private static string ObtenerTelefono(IWin32Window owner, Cliente c)
    {
        if (!string.IsNullOrWhiteSpace(c.Telefono) && WhatsApp.Validar(c.Telefono) == null) return c.Telefono;
        var aviso = string.IsNullOrWhiteSpace(c.Telefono)
            ? $"{c.Nombre} no tiene teléfono cargado."
            : $"El teléfono de {c.Nombre} no es válido ({c.Telefono}).";
        var telefono = Dialogos.PedirTelefono(owner, "WhatsApp",
            aviso + "\nIngresalo: código de área sin 0 + número sin 15. Ej: 1181077056", c.Telefono);
        if (telefono == null) return null;
        if (UI.Confirmar("¿Guardar este número en la ficha del cliente?"))
        {
            c.Telefono = telefono;
            UI.Intentar(() => ClienteRepo.Guardar(c));
        }
        return telefono;
    }

    /// <summary>Arma el comprobante del último pago del cliente.</summary>
    private static string TextoComprobante(Cliente c)
    {
        var pago = ClienteRepo.UltimoPago(c.Id);
        if (pago == null) return null;
        decimal saldoActual = ClienteRepo.Saldo(c.Id);
        return Ticket.ComprobantePago(c, pago, saldoActual + pago.Monto, saldoActual);
    }

    /// <summary>Muestra el comprobante del último pago con botones Imprimir y WhatsApp (también desde la lista de clientes).</summary>
    public static void VerComprobante(IWin32Window owner, int clienteId)
    {
        var c = ClienteRepo.ObtenerPorId(clienteId);
        if (c == null) return;
        var texto = TextoComprobante(c);
        if (texto == null) { UI.Info($"{c.Nombre} todavía no registró pagos."); return; }
        using var f = new FrmTicket("Comprobante de pago — " + c.Nombre, texto, c.Telefono);
        f.ShowDialog(owner);
    }

    /// <summary>Manda directo por WhatsApp el comprobante del último pago (se usa justo después de registrar un pago).</summary>
    public static void EnviarComprobante(IWin32Window owner, int clienteId)
    {
        var c = ClienteRepo.ObtenerPorId(clienteId);
        if (c == null) return;
        var texto = TextoComprobante(c);
        if (texto == null) { UI.Info($"{c.Nombre} todavía no registró pagos."); return; }
        var telefono = ObtenerTelefono(owner, c);
        if (telefono == null) return;
        if (!WhatsApp.Enviar(telefono, texto))
            UI.Advertir("El número de teléfono no parece válido.");
    }

    private void RegistrarPago()
    {
        var monto = Dialogos.PedirNumero(this, "Registrar pago",
            $"{cliente.Nombre} debe {UI.Moneda(Math.Max(0, saldo))}.\n¿Cuánto paga?", Math.Max(0, saldo), 0.01m);
        if (monto == null) return;

        var formas = FormasPago.Todas.Where(f => f.Codigo != "CUENTA_CORRIENTE").ToList();
        var nombre = Dialogos.Elegir(this, "Registrar pago", "Forma de pago:", formas.Select(f => f.Nombre));
        if (nombre == null) return;
        var codigo = formas.First(f => f.Nombre == nombre).Codigo;

        if (!UI.Intentar(() => ClienteRepo.RegistrarPago(clienteId, monto.Value, codigo, "Pago de cuenta"), "No se pudo registrar el pago."))
            return;

        CargarCliente();
        CargarMovimientos();

        string estado = saldo <= 0 ? "¡La deuda quedó CANCELADA!" : $"Saldo pendiente: {UI.Moneda(saldo)}";
        var r = MessageBox.Show(
            $"Pago de {UI.Moneda(monto.Value)} registrado.\n{estado}\n\n" +
            "¿Enviarle el comprobante por WhatsApp?\n\n" +
            "Sí = enviar por WhatsApp     No = ver / imprimir     Cancelar = nada",
            UI.Negocio, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
        if (r == DialogResult.Yes)
        {
            EnviarComprobante(this, clienteId);
        }
        else if (r == DialogResult.No)
        {
            using var f = new FrmTicket("Comprobante de pago", TextoComprobante(cliente), cliente.Telefono);
            f.ShowDialog(this);
        }
    }
}
