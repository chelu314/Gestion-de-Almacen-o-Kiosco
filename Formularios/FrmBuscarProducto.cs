namespace KioscoLaFamilia.Formularios;

/// <summary>Buscador de productos por nombre, código o categoría. Enter o doble clic elige.</summary>
public class FrmBuscarProducto : Form
{
    private readonly TextBox txtFiltro;
    private readonly DataGridView grid;
    public Producto Elegido { get; private set; }

    public static Producto Elegir(IWin32Window owner, string filtroInicial)
    {
        using var f = new FrmBuscarProducto(filtroInicial);
        return f.ShowDialog(owner) == DialogResult.OK ? f.Elegido : null;
    }

    public FrmBuscarProducto(string filtroInicial)
    {
        UI.PrepararForm(this, "Buscar producto", 820, 520);

        var filtros = UI.BarraFiltros();
        filtros.Controls.Add(UI.Etiqueta("Buscar:", 11f, true));
        txtFiltro = new TextBox { Width = 400, Font = new Font("Segoe UI", 12f), Text = filtroInicial ?? "" };
        txtFiltro.TextChanged += (s, e) => Cargar();
        txtFiltro.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Down) { grid.Focus(); e.Handled = true; }
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Aceptar(); }
        };
        filtros.Controls.Add(txtFiltro);

        grid = UI.Grilla();
        UI.Configurar(grid, ocultas: new[] { "id", "Costo", "Margen %", "bajo", "Controla stock" }, moneda: new[] { "Precio" },
            cantidades: new[] { "Stock", "Mínimo" });
        grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Aceptar(); };
        grid.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Aceptar(); }
        };

        var barra = UI.BarraBotones(DockStyle.Bottom, true);
        var btnOk = UI.Boton("Elegir (Enter)", UI.Exito, 150);
        UI.ColorearBoton(btnOk, UI.Exito, destacado: true);
        btnOk.Click += (s, e) => Aceptar();
        var btnCancelar = UI.Boton("Cancelar (Esc)", UI.Neutro, 150);
        btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        barra.Controls.Add(btnOk);
        barra.Controls.Add(btnCancelar);

        UI.Ensamblar(this, grid, filtros, barra);
        CancelButton = btnCancelar;

        Load += (s, e) => Cargar();
        Shown += (s, e) => { txtFiltro.Focus(); txtFiltro.SelectAll(); };
    }

    private void Cargar() => UI.Intentar(() => grid.DataSource = ProductoRepo.Tabla(txtFiltro.Text.Trim()));

    private void Aceptar()
    {
        var id = UI.IdSeleccionado(grid);
        if (id == null) return;
        Elegido = ProductoRepo.ObtenerPorId(id.Value);
        DialogResult = DialogResult.OK;
        Close();
    }
}
