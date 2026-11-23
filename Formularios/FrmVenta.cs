namespace KioscoLaFamilia.Formularios;

/// <summary>
/// Punto de venta. El lector de código de barras funciona como un teclado:
/// escribe el código y manda Enter, así que solo hay que tener el foco en "Código".
/// Atajos: F3 buscar · F12 cobrar · Supr quitar · Esc cancelar.
/// </summary>
public class FrmVenta : Form
{
    private readonly BindingList<ItemCarrito> carrito = new BindingList<ItemCarrito>();

    private readonly TextBox txtCodigo;
    private readonly NumericUpDown numCant;
    private readonly NumericUpDown numPaga;
    private readonly DataGridView grid;
    private readonly Label lblTotal;
    private readonly Label lblItems;
    private readonly Label lblVuelto;
    private readonly Label lblUltima;
    private readonly ComboBox cmbForma;
    private readonly ComboBox cmbCliente;
    private int ultimaVentaId;
    private readonly Button btnTicket;
    private readonly Panel panelRapido;
    private readonly FlowLayoutPanel filaCategorias;
    private readonly FlowLayoutPanel filaProductos;
    private List<Producto> productosRapidos = new List<Producto>();

    public FrmVenta()
    {
        UI.PrepararForm(this, "Punto de venta", 1150, 700);
        WindowState = FormWindowState.Maximized;

        // ===== Barra superior: código y cantidad =====
        var arriba = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 66, Padding = new Padding(10, 12, 10, 0), BackColor = Color.White, WrapContents = false };
        arriba.Controls.Add(UI.Etiqueta("Código / escanear:", 11f, true));
        txtCodigo = new TextBox { Width = 330, Font = new Font("Segoe UI", 14f) };
        txtCodigo.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; AgregarDesdeCodigo(); }
        };
        arriba.Controls.Add(txtCodigo);
        arriba.Controls.Add(UI.Etiqueta("Cant.:", 11f, true));
        numCant = UI.Numero(9999, 0, 80);
        numCant.Minimum = 1;
        numCant.Value = 1;
        numCant.Font = new Font("Segoe UI", 14f);
        arriba.Controls.Add(numCant);
        // El lector manda Enter solo, así que no hace falta botón "Agregar"
        var btnBuscar = UI.BotonSecundario("Buscar por nombre (F3)", BuscarProducto, 200);
        btnBuscar.Height = 38;
        arriba.Controls.Add(btnBuscar);
        var ayuda = UI.Etiqueta("Escaneá o escribí el código y Enter  ·  3*código = 3 unidades", 9f);
        ayuda.ForeColor = UI.Neutro;
        arriba.Controls.Add(ayuda);

        // ===== Panel derecho: total y cobro =====
        var derecha = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 350,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(16, 10, 10, 10),
            BackColor = Color.White,
            AutoScroll = true
        };
        derecha.Controls.Add(UI.Etiqueta("TOTAL A COBRAR", 11f, true));
        lblTotal = new Label { Text = UI.Moneda(0), Font = new Font("Segoe UI", 30f, FontStyle.Bold), ForeColor = UI.Exito, AutoSize = true };
        derecha.Controls.Add(lblTotal);
        lblItems = UI.Etiqueta("0 artículos");
        derecha.Controls.Add(lblItems);

        derecha.Controls.Add(UI.Etiqueta("Forma de pago", 10f, true));
        cmbForma = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 310, Font = new Font("Segoe UI", 11f) };
        foreach (var f in FormasPago.Todas) cmbForma.Items.Add(f.Nombre);
        cmbForma.SelectedIndex = 0;
        cmbForma.SelectedIndexChanged += (s, e) => ActualizarCobro();
        derecha.Controls.Add(cmbForma);

        derecha.Controls.Add(UI.Etiqueta("Cliente", 10f, true));
        var filaCliente = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        cmbCliente = new ComboBox
        {
            Width = 262,
            Font = new Font("Segoe UI", 11f),
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems
        };
        var btnNuevoCliente = UI.Boton("+", UI.Primario, 40, 30);
        btnNuevoCliente.Click += (s, e) => NuevoCliente();
        new ToolTip().SetToolTip(btnNuevoCliente, "Cargar un cliente nuevo");
        filaCliente.Controls.Add(cmbCliente);
        filaCliente.Controls.Add(btnNuevoCliente);
        derecha.Controls.Add(filaCliente);

        derecha.Controls.Add(UI.Etiqueta("Paga con (efectivo)", 10f, true));
        numPaga = UI.Numero(100_000_000m, 2, 310);
        numPaga.Font = new Font("Segoe UI", 14f);
        numPaga.KeyUp += (s, e) => ActualizarVuelto();
        numPaga.ValueChanged += (s, e) => ActualizarVuelto();
        numPaga.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Cobrar(); }
        };
        derecha.Controls.Add(numPaga);

        lblVuelto = new Label { Text = "Vuelto: " + UI.Moneda(0), Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = UI.Primario, AutoSize = true, Margin = new Padding(4, 10, 4, 10) };
        derecha.Controls.Add(lblVuelto);

        var btnCobrar = UI.Boton("COBRAR (F12)", UI.Exito, 310, 62);
        btnCobrar.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
        UI.ColorearBoton(btnCobrar, UI.Exito, destacado: true);
        btnCobrar.Click += (s, e) => Cobrar();
        derecha.Controls.Add(btnCobrar);

        var btnCancelar = UI.BotonSecundario("Cancelar venta (Esc)", CancelarVenta, 310, Tema.Neutro);
        derecha.Controls.Add(btnCancelar);

        lblUltima = new Label { AutoSize = true, MaximumSize = new Size(310, 0), ForeColor = UI.Neutro, Margin = new Padding(4, 14, 4, 4) };
        derecha.Controls.Add(lblUltima);
        // Aparece recién después de la primera venta
        btnTicket = UI.BotonSecundario("Ver ticket de esa venta", VerTicketUltima, 310, Tema.Neutro);
        btnTicket.Visible = false;
        derecha.Controls.Add(btnTicket);

        // ===== Centro: carrito =====
        var centro = new Panel();
        grid = UI.Grilla();
        grid.DataSource = carrito;
        grid.Font = new Font("Segoe UI", 11f);
        UI.Configurar(grid,
            ocultas: new[] { "ProductoId", "CostoUnitario", "Cantidad", "Unidad", "ControlaStock" },
            moneda: new[] { "PrecioUnitario", "Subtotal" },
            titulos: new Dictionary<string, string>
            {
                ["Codigo"] = "Código",
                ["Descripcion"] = "Producto",
                ["CantidadTexto"] = "Cant.",
                ["PrecioUnitario"] = "Precio (u. o kg)"
            },
            extra: g =>
            {
                if (g.Columns.Contains("Descripcion")) g.Columns["Descripcion"].FillWeight = 260;
            });

        // Doble clic en un renglón = cambiar cantidad;  + y - del teclado numérico suman o restan uno
        grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) FijarCantidad(); };
        var barraCarrito = UI.BarraBotones();
        barraCarrito.Controls.Add(UI.BotonSecundario("Cambiar cantidad", FijarCantidad, 180));
        barraCarrito.Controls.Add(UI.BotonSecundario("Quitar producto (Supr)", QuitarSeleccionado, 200));
        // Botones de venta rápida: helados, pan por kilo, golosinas sueltas, etc.
        panelRapido = new Panel { Dock = DockStyle.Top, Height = 190, BackColor = Color.White, Padding = new Padding(6) };
        filaCategorias = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, WrapContents = false, AutoScroll = true };
        filaProductos = new FlowLayoutPanel { AutoScroll = true };
        UI.Ensamblar(panelRapido, filaProductos, filaCategorias);


        var contenedorGrid = new Panel();
        UI.Ensamblar(contenedorGrid, grid, panelRapido);
        UI.Ensamblar(centro, contenedorGrid, barraCarrito);

        UI.Ensamblar(this, centro, arriba, derecha);

        // ===== Atajos =====
        KeyDown += (s, e) =>
        {
            switch (e.KeyCode)
            {
                case Keys.F12: e.Handled = true; Cobrar(); break;
                case Keys.F3: e.Handled = true; BuscarProducto(); break;
                case Keys.F4: numPaga.Focus(); break;
                case Keys.Escape: e.Handled = true; CancelarVenta(); break;
                case Keys.Delete:
                    if (grid.ContainsFocus) { e.Handled = true; QuitarSeleccionado(); }
                    break;
                case Keys.Add:
                    if (grid.ContainsFocus) { e.Handled = true; CambiarCantidad(+1); }
                    break;
                case Keys.Subtract:
                    if (grid.ContainsFocus) { e.Handled = true; CambiarCantidad(-1); }
                    break;
            }
        };

        FormClosing += (s, e) =>
        {
            if (carrito.Count > 0 && !UI.Confirmar("Hay una venta sin cobrar. ¿Salir igual?")) e.Cancel = true;
        };

        Load += (s, e) => { CargarClientes(); CargarVentaRapida(); ActualizarCobro(); };
        Shown += (s, e) => txtCodigo.Focus();
    }

    // ---------------------------------------------------------------

    private string FormaSeleccionada => FormasPago.Todas[cmbForma.SelectedIndex].Codigo;
    private decimal Total => carrito.Sum(i => i.Subtotal);

    // ----- Venta rápida con botones -----

    private void CargarVentaRapida()
    {
        try { productosRapidos = ProductoRepo.ParaVentaRapida(); }
        catch { productosRapidos = new List<Producto>(); }

        filaCategorias.Controls.Clear();
        var categorias = productosRapidos.Select(p => p.Categoria).Distinct().ToList();
        panelRapido.Visible = categorias.Count > 0;
        foreach (var cat in categorias)
        {
            var b = UI.Boton(cat, UI.Primario, 120, 36);
            b.Click += (s, e) => MostrarCategoria(cat);
            filaCategorias.Controls.Add(b);
        }
        if (categorias.Count > 0) MostrarCategoria(categorias[0]);
    }

    private void MostrarCategoria(string categoria)
    {
        foreach (Button b in filaCategorias.Controls)
            UI.ColorearBoton(b, UI.Primario, destacado: b.Text == categoria);

        filaProductos.SuspendLayout();
        filaProductos.Controls.Clear();
        foreach (var p in productosRapidos.Where(x => x.Categoria == categoria))
        {
            var texto = p.Nombre + "\n" + UI.Moneda(p.PrecioVenta) + (p.PorPeso ? " /kg" : "");
            var b = UI.Boton(texto, UI.Turquesa, 150, 64);
            b.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            var producto = p;
            b.Click += (s, e) =>
            {
                AgregarProducto(producto, (int)numCant.Value);
                numCant.Value = 1;
                txtCodigo.Focus();
            };
            filaProductos.Controls.Add(b);
        }
        filaProductos.ResumeLayout();
        txtCodigo.Focus();
    }

    private void CargarClientes()
    {
        cmbCliente.Items.Clear();
        cmbCliente.Items.Add(new Cliente { Id = 0, Nombre = "Consumidor final" });
        foreach (var c in ClienteRepo.ListaActivos()) cmbCliente.Items.Add(c);
        cmbCliente.SelectedIndex = 0;
    }

    private Cliente ClienteSeleccionado()
    {
        if (cmbCliente.SelectedItem is Cliente c) return c.Id == 0 ? null : c;
        var texto = cmbCliente.Text.Trim();
        var encontrado = cmbCliente.Items.Cast<Cliente>()
            .FirstOrDefault(x => x.Nombre.Equals(texto, StringComparison.OrdinalIgnoreCase));
        return encontrado == null || encontrado.Id == 0 ? null : encontrado;
    }

    private void AgregarDesdeCodigo()
    {
        var texto = txtCodigo.Text.Trim();
        if (texto == "") return;

        int cantidad = (int)numCant.Value;
        var partes = texto.Split('*');
        if (partes.Length == 2 && int.TryParse(partes[0].Trim(), out int c) && c > 0)
        {
            cantidad = c;
            texto = partes[1].Trim();
        }

        Producto p = null;
        if (!UI.Intentar(() => p = ProductoRepo.ObtenerPorCodigo(texto), "No se pudo buscar el producto.")) return;

        if (p == null)
        {
            // No es un código: buscar por nombre
            var lista = ProductoRepo.Buscar(texto);
            if (lista.Count == 1) p = lista[0];
            else if (lista.Count > 1) p = FrmBuscarProducto.Elegir(this, texto);
        }

        if (p == null)
        {
            System.Media.SystemSounds.Exclamation.Play();
            if (Sesion.EsAdmin && texto.All(char.IsDigit))
            {
                if (UI.Confirmar($"El código {texto} no existe.\n¿Querés darlo de alta ahora?"))
                {
                    using var f = new FrmProductoEditar(null, texto);
                    if (f.ShowDialog(this) == DialogResult.OK) p = ProductoRepo.ObtenerPorId(f.ProductoId);
                }
            }
            else
            {
                UI.Advertir("Producto no encontrado: " + texto);
            }
        }

        if (p != null) AgregarProducto(p, cantidad);

        txtCodigo.Clear();
        numCant.Value = 1;
        txtCodigo.Focus();
    }

    /// <summary>Agrega un producto: si se vende por kilo pide el peso; si no, usa la cantidad.</summary>
    private void AgregarProducto(Producto p, int unidades)
    {
        if (p.PorPeso)
        {
            var kilos = DialogoPeso.Pedir(this, p);
            if (kilos == null) return;
            Agregar(p, kilos.Value);
        }
        else
        {
            Agregar(p, unidades);
        }
    }

    private void Agregar(Producto p, decimal cantidad)
    {
        var item = carrito.FirstOrDefault(i => i.ProductoId == p.Id);
        decimal nuevaCantidad = (item?.Cantidad ?? 0) + cantidad;

        if (p.ControlaStock && nuevaCantidad > p.Stock &&
            !UI.Confirmar($"Stock insuficiente de \"{p.Nombre}\" (hay {p.StockTexto}).\n¿Vender igual?"))
            return;

        if (item == null)
        {
            carrito.Add(new ItemCarrito
            {
                ProductoId = p.Id,
                Codigo = p.CodigoBarras,
                Descripcion = p.Nombre,
                Cantidad = cantidad,
                Unidad = p.Unidad,
                ControlaStock = p.ControlaStock,
                PrecioUnitario = p.PrecioVenta,
                CostoUnitario = p.PrecioCosto
            });
            item = carrito[carrito.Count - 1];
        }
        else
        {
            item.Cantidad = nuevaCantidad;
            carrito.ResetBindings();
        }

        SeleccionarItem(item);
        ActualizarCobro();
    }

    private void SeleccionarItem(ItemCarrito item)
    {
        int idx = carrito.IndexOf(item);
        if (idx >= 0 && idx < grid.Rows.Count)
        {
            grid.ClearSelection();
            grid.Rows[idx].Selected = true;
            var celdaVisible = grid.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.Visible);
            if (celdaVisible != null) grid.CurrentCell = grid.Rows[idx].Cells[celdaVisible.Index];
        }
    }

    private ItemCarrito ItemSeleccionado =>
        grid.CurrentRow != null && grid.CurrentRow.Index >= 0 && grid.CurrentRow.Index < carrito.Count
            ? carrito[grid.CurrentRow.Index]
            : null;

    private void CambiarCantidad(int delta)
    {
        var item = ItemSeleccionado;
        if (item == null) return;
        if (item.Unidad == "KG") { FijarCantidad(); return; }
        item.Cantidad += delta;
        if (item.Cantidad <= 0) carrito.Remove(item);
        else { carrito.ResetBindings(); SeleccionarItem(item); }
        ActualizarCobro();
        txtCodigo.Focus();
    }

    private void FijarCantidad()
    {
        var item = ItemSeleccionado;
        if (item == null) return;
        decimal? n;
        if (item.Unidad == "KG")
        {
            var p = ProductoRepo.ObtenerPorId(item.ProductoId);
            n = p == null ? null : DialogoPeso.Pedir(this, p, item.Cantidad);
        }
        else
        {
            n = Dialogos.PedirNumero(this, "Cantidad", "Nueva cantidad para " + item.Descripcion, item.Cantidad, 1, 9999, 0);
        }
        if (n == null) return;
        item.Cantidad = n.Value;
        carrito.ResetBindings();
        SeleccionarItem(item);
        ActualizarCobro();
        txtCodigo.Focus();
    }

    private void QuitarSeleccionado()
    {
        var item = ItemSeleccionado;
        if (item == null) return;
        carrito.Remove(item);
        ActualizarCobro();
        txtCodigo.Focus();
    }

    private void BuscarProducto()
    {
        var p = FrmBuscarProducto.Elegir(this, txtCodigo.Text.Trim());
        if (p != null) AgregarProducto(p, (int)numCant.Value);
        txtCodigo.Clear();
        numCant.Value = 1;
        txtCodigo.Focus();
    }

    private void NuevoCliente()
    {
        using var f = new FrmClienteEditar(null);
        if (f.ShowDialog(this) != DialogResult.OK) return;
        CargarClientes();
        var nuevo = cmbCliente.Items.Cast<Cliente>().FirstOrDefault(c => c.Id == f.ClienteId);
        if (nuevo != null) cmbCliente.SelectedItem = nuevo;
    }

    private void ActualizarCobro()
    {
        lblTotal.Text = UI.Moneda(Total);
        lblItems.Text = $"{carrito.Count} renglón(es)";
        bool efectivo = FormaSeleccionada == "EFECTIVO";
        numPaga.Enabled = efectivo;
        if (!efectivo) numPaga.Value = 0;
        ActualizarVuelto();
    }

    private void ActualizarVuelto()
    {
        decimal paga = numPaga.Value; // leer Value valida el texto escrito
        if (FormaSeleccionada != "EFECTIVO" || paga == 0)
        {
            lblVuelto.Text = "Vuelto: " + UI.Moneda(0);
            lblVuelto.ForeColor = UI.Primario;
            return;
        }
        decimal vuelto = paga - Total;
        lblVuelto.Text = vuelto >= 0 ? "Vuelto: " + UI.Moneda(vuelto) : "Falta: " + UI.Moneda(-vuelto);
        lblVuelto.ForeColor = vuelto >= 0 ? UI.Primario : UI.Peligro;
    }

    private void Cobrar()
    {
        if (carrito.Count == 0)
        {
            UI.Advertir("No hay productos en la venta.");
            txtCodigo.Focus();
            return;
        }

        string forma = FormaSeleccionada;
        var cliente = ClienteSeleccionado();
        decimal total = Total;
        decimal? pagaCon = null;

        if (forma == "CUENTA_CORRIENTE")
        {
            if (cliente == null)
            {
                UI.Advertir("Para vender en cuenta corriente elegí un cliente.");
                cmbCliente.Focus();
                return;
            }
            decimal saldo = ClienteRepo.Saldo(cliente.Id);
            if (cliente.LimiteCredito > 0 && saldo + total > cliente.LimiteCredito &&
                !UI.Confirmar($"{cliente.Nombre} debe {UI.Moneda(saldo)} y con esta compra supera su límite de {UI.Moneda(cliente.LimiteCredito)}.\n¿Fiar igual?"))
                return;
        }
        else if (forma == "EFECTIVO")
        {
            decimal paga = numPaga.Value;
            if (paga == 0) paga = total; // pago justo
            if (paga < total)
            {
                UI.Advertir($"El monto recibido ({UI.Moneda(paga)}) es menor al total ({UI.Moneda(total)}).");
                numPaga.Focus();
                return;
            }
            pagaCon = paga;
        }

        int ventaId;
        try
        {
            ventaId = VentaRepo.Registrar(cliente?.Id, forma, pagaCon, carrito.ToList());
        }
        catch (Exception ex)
        {
            UI.Error("No se pudo registrar la venta:\n" + ex.Message);
            return;
        }

        ultimaVentaId = ventaId;
        var vendidos = carrito.Select(i => i.ProductoId).ToHashSet();
        btnTicket.Visible = true;
        lblUltima.Text = $"Última venta N° {ventaId}: {UI.Moneda(total)}" +
                         (pagaCon.HasValue ? $" — Vuelto {UI.Moneda(pagaCon.Value - total)}" : "");

        carrito.Clear();
        cmbForma.SelectedIndex = 0;
        cmbCliente.SelectedIndex = 0;
        numPaga.Value = 0;
        ActualizarCobro();

        // Aviso de poco stock de lo que se acaba de vender
        var bajos = ProductoRepo.StockBajo().Where(p => vendidos.Contains(p.Id)).ToList();
        if (bajos.Count > 0)
            UI.Advertir("Queda poco stock de:\n\n" +
                        string.Join("\n", bajos.Select(p => $"• {p.Nombre}: quedan {p.StockTexto} (mínimo {Cantidades.Formato(p.StockMinimo, p.Unidad)})")));

        if (ConfigRepo.ObtenerBool("mostrar_ticket", false) || forma == "CUENTA_CORRIENTE")
            MostrarTicket(ventaId, cliente?.Telefono);

        txtCodigo.Focus();
    }

    private void MostrarTicket(int ventaId, string telefono)
    {
        using var f = new FrmTicket("Ticket N° " + ventaId, Ticket.DeVenta(ventaId), telefono);
        f.ShowDialog(this);
    }

    private void VerTicketUltima()
    {
        if (ultimaVentaId == 0) { UI.Info("Todavía no se registraron ventas en esta ventana."); return; }
        var cab = VentaRepo.Cabecera(ultimaVentaId);
        MostrarTicket(ultimaVentaId, cab?.Str("cliente_telefono"));
    }

    private void CancelarVenta()
    {
        if (carrito.Count == 0) { Close(); return; }
        if (!UI.Confirmar("¿Cancelar la venta actual?")) return;
        carrito.Clear();
        ActualizarCobro();
        txtCodigo.Focus();
    }
}
