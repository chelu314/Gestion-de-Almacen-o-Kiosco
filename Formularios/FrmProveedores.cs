namespace KioscoLaFamilia.Formularios;

/// <summary>Agenda de proveedores. Desde acá se hace el pedido a cada uno.</summary>
public class FrmProveedores : Form
{
    private readonly TextBox txtFiltro;
    private readonly DataGridView grid;

    public FrmProveedores()
    {
        UI.PrepararForm(this, "Proveedores y pedidos", 1000, 600);

        // Arriba: buscador
        var filtros = UI.BarraFiltros();
        filtros.Controls.Add(UI.Etiqueta("Buscar proveedor:", 10f, true));
        txtFiltro = UI.Texto(280);
        txtFiltro.TextChanged += (s, e) => Cargar();   // busca mientras escribís
        filtros.Controls.Add(txtFiltro);

        // Centro: la lista
        grid = UI.Grilla();
        UI.Configurar(grid, ocultas: new[] { "id" });
        grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) HacerPedido(); };  // doble clic = pedir

        // Abajo: un botón fuerte (lo más usado) y los demás suaves
        var barra = UI.BarraBotones();
        barra.Controls.Add(UI.BotonPrincipal("Hacer pedido", HacerPedido, 190, Tema.WhatsApp));
        barra.Controls.Add(UI.BotonSecundario("Nuevo proveedor", Nuevo, 180));
        var opciones = new[]
        {
            new UI.Opcion("Editar datos del proveedor", Editar),
            UI.Separador,
            new UI.Opcion("Eliminar proveedor", Eliminar, Peligrosa: true)
        };
        barra.Controls.Add(UI.BotonMas(opciones));
        UI.MenuEnGrilla(grid, opciones);   // el mismo menú con clic derecho

        UI.Ensamblar(this, grid, filtros, barra);
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        Load += (s, e) => Cargar();
    }

    private void Cargar() => UI.Intentar(() => grid.DataSource = ProveedorRepo.Tabla(txtFiltro.Text.Trim()));

    private void Nuevo()
    {
        using var f = new FrmProveedorEditar(null);
        if (f.ShowDialog(this) == DialogResult.OK) Cargar();
    }

    private void Editar()
    {
        var id = UI.IdSeleccionado(grid);   // null si no hay ninguno elegido
        if (id == null) return;
        using var f = new FrmProveedorEditar(ProveedorRepo.ObtenerPorId(id.Value));
        if (f.ShowDialog(this) == DialogResult.OK) Cargar();
    }

    private void Eliminar()
    {
        var id = UI.IdSeleccionado(grid);
        if (id == null) return;
        var p = ProveedorRepo.ObtenerPorId(id.Value);
        if (!UI.Confirmar($"¿Eliminar al proveedor \"{p.Nombre}\"?")) return;
        if (UI.Intentar(() => ProveedorRepo.Eliminar(p.Id))) Cargar();
    }

    private void HacerPedido()
    {
        var id = UI.IdSeleccionado(grid);
        if (id == null)
        {
            UI.Info("Primero elegí un proveedor de la lista (o cargá uno con \"Nuevo proveedor\").");
            return;
        }
        using var f = new FrmPedido(ProveedorRepo.ObtenerPorId(id.Value));
        f.ShowDialog(this);
    }
}