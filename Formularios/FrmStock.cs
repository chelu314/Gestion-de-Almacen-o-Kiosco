namespace KioscoLaFamilia.Formularios;

/// <summary>Ingreso de mercadería, ajuste por conteo y bajas (roturas/vencidos), escaneando el código.</summary>
public class FrmStock : Form
{
    private static readonly (string Codigo, string Nombre)[] Tipos =
    {
        ("INGRESO", "Ingreso de mercadería (suma)"),
        ("AJUSTE", "Ajuste por conteo (fija el stock real)"),
        ("BAJA", "Baja: roto / vencido / pérdida (resta)")
    };

    private Producto producto;
    private readonly TextBox txtCodigo;
    private readonly Label lblProducto;
    private readonly ComboBox cmbTipo;
    private readonly NumericUpDown numCantidad;
    private readonly CheckBox chkPrecios;
    private readonly NumericUpDown numCosto;
    private readonly NumericUpDown numPrecio;
    private readonly TextBox txtObs;
    private readonly DataGridView grid;

    public FrmStock()
    {
        UI.PrepararForm(this, "Ingreso y ajuste de stock", 1100, 680);

        var campos = UI.TablaCampos();
        campos.Dock = DockStyle.Top;
        campos.Height = 390;
        campos.BackColor = Color.White;

        var filaCodigo = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        txtCodigo = UI.Texto(260);
        txtCodigo.Font = new Font("Segoe UI", 12f);
        txtCodigo.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BuscarCodigo(); }
        };
        var btnBuscar = UI.Boton("Buscar (F3)", UI.Neutro, 120, 32);
        btnBuscar.Click += (s, e) => { var p = FrmBuscarProducto.Elegir(this, txtCodigo.Text.Trim()); if (p != null) MostrarProducto(p); };
        filaCodigo.Controls.Add(txtCodigo);
        filaCodigo.Controls.Add(btnBuscar);
        UI.Campo(campos, "Código (escanear)", filaCodigo);

        lblProducto = UI.Etiqueta("— escaneá o buscá un producto —", 11f, true);
        lblProducto.ForeColor = UI.Primario;
        UI.Campo(campos, "Producto", lblProducto);

        cmbTipo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 340 };
        foreach (var t in Tipos) cmbTipo.Items.Add(t.Nombre);
        cmbTipo.SelectedIndex = 0;
        UI.Campo(campos, "Tipo de movimiento", cmbTipo);

        numCantidad = UI.Numero(1_000_000, 3, 120);
        numCantidad.Value = 1;
        numCantidad.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Registrar(); }
        };
        UI.Campo(campos, "Cantidad (kg con coma si es por kilo)", numCantidad);

        chkPrecios = new CheckBox { Text = "Actualizar costo y precio de venta", AutoSize = true };
        UI.Campo(campos, "", chkPrecios);

        numCosto = UI.Numero();
        numCosto.Enabled = false;
        UI.Campo(campos, "Nuevo costo", numCosto);
        numPrecio = UI.Numero();
        numPrecio.Enabled = false;
        UI.Campo(campos, "Nuevo precio venta", numPrecio);
        chkPrecios.CheckedChanged += (s, e) => { numCosto.Enabled = numPrecio.Enabled = chkPrecios.Checked; };

        txtObs = UI.Texto(340);
        UI.Campo(campos, "Observación", txtObs);

        var acciones = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        acciones.Controls.Add(UI.BotonPrincipal("Registrar movimiento", Registrar, 220, Tema.Exito));
        // "Ver faltantes" solo muestra lo que falta
        acciones.Controls.Add(UI.BotonSecundario("Ver faltantes", () =>
        {
            using var f = new FrmStockBajo();
            f.ShowDialog(this);
        }, 160));
        // Los pedidos se hacen a mano, eligiendo el proveedor
        acciones.Controls.Add(UI.BotonSecundario("Proveedores y pedidos", () =>
        {
            using var f = new FrmProveedores();
            f.ShowDialog(this);
        }, 220));
        UI.Campo(campos, "", acciones);

        var titulo = UI.Etiqueta("Últimos movimientos de stock", 11f, true);
        titulo.Dock = DockStyle.Top;
        titulo.AutoSize = false;
        titulo.Height = 30;
        titulo.Padding = new Padding(10, 4, 0, 0);

        grid = UI.Grilla();
        UI.Configurar(grid, fechas: new[] { "Fecha" }, cantidades: new[] { "Cantidad", "Stock final" });

        var abajo = new Panel();
        UI.Ensamblar(abajo, grid, titulo);
        UI.Ensamblar(this, abajo, campos);

        KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Escape) Close();
            else if (e.KeyCode == Keys.F3) { var p = FrmBuscarProducto.Elegir(this, txtCodigo.Text.Trim()); if (p != null) MostrarProducto(p); }
        };
        Load += (s, e) => CargarMovimientos();
        Shown += (s, e) => txtCodigo.Focus();
    }

    private void CargarMovimientos() => UI.Intentar(() => grid.DataSource = ProductoRepo.UltimosMovimientos(100));

    private void BuscarCodigo()
    {
        var codigo = txtCodigo.Text.Trim();
        if (codigo == "") return;
        var p = ProductoRepo.ObtenerPorCodigo(codigo);
        if (p == null)
        {
            var lista = ProductoRepo.Buscar(codigo);
            p = lista.Count == 1 ? lista[0] : (lista.Count > 1 ? FrmBuscarProducto.Elegir(this, codigo) : null);
        }
        if (p == null)
        {
            if (UI.Confirmar($"No existe un producto con el código {codigo}.\n¿Darlo de alta ahora?"))
            {
                using var f = new FrmProductoEditar(null, codigo);
                if (f.ShowDialog(this) == DialogResult.OK) { CargarMovimientos(); p = ProductoRepo.ObtenerPorId(f.ProductoId); }
            }
            if (p == null) { txtCodigo.SelectAll(); return; }
        }
        MostrarProducto(p);
    }

    private void MostrarProducto(Producto p)
    {
        producto = p;
        txtCodigo.Text = p.CodigoBarras;
        lblProducto.Text = $"{p.Nombre}   |   Stock actual: {p.StockTexto}{(p.PorPeso ? "" : " u.")}   |   Precio: {UI.Moneda(p.PrecioVenta)}{(p.PorPeso ? " /kg" : "")}";
        if (!p.ControlaStock) lblProducto.Text += "   (no controla stock)";
        numCosto.Value = p.PrecioCosto;
        numPrecio.Value = p.PrecioVenta;
        numCantidad.Value = cmbTipo.SelectedIndex == 1 ? Math.Max(0, p.Stock) : 1;
        numCantidad.Focus();
        numCantidad.Select(0, numCantidad.Text.Length);
    }

    private void Registrar()
    {
        if (producto == null) { UI.Advertir("Primero escaneá o buscá un producto."); txtCodigo.Focus(); return; }
        var tipo = Tipos[cmbTipo.SelectedIndex].Codigo;
        decimal cantidad = numCantidad.Value;
        if (tipo != "AJUSTE" && cantidad == 0) { UI.Advertir("Ingresá una cantidad."); return; }

        var obs = txtObs.Text.Trim();
        if (obs == "") obs = Tipos[cmbTipo.SelectedIndex].Nombre;

        bool ok = UI.Intentar(() => ProductoRepo.MoverStock(producto.Id, tipo, cantidad, obs,
            chkPrecios.Checked ? numCosto.Value : (decimal?)null,
            chkPrecios.Checked ? numPrecio.Value : (decimal?)null));
        if (!ok) return;

        var actualizado = ProductoRepo.ObtenerPorId(producto.Id);
        System.Media.SystemSounds.Asterisk.Play();
        lblProducto.Text = $"✔ {actualizado.Nombre}: stock ahora {actualizado.StockTexto}";

        producto = null;
        txtCodigo.Clear();
        txtObs.Clear();
        chkPrecios.Checked = false;
        numCantidad.Value = 1;
        CargarMovimientos();
        txtCodigo.Focus();
    }
}
