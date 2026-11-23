namespace KioscoLaFamilia.Formularios;

/// <summary>Listado de clientes con su saldo de cuenta corriente.</summary>
public class FrmClientes : Form
{
    private readonly TextBox txtFiltro;
    private readonly CheckBox chkDeudores;
    private readonly DataGridView grid;
    private readonly Label lblTotal;

    public FrmClientes()
    {
        UI.PrepararForm(this, "Clientes y cuentas corrientes", 1050, 620);

        var filtros = UI.BarraFiltros();
        filtros.Controls.Add(UI.Etiqueta("Buscar cliente:", 10f, true));
        txtFiltro = UI.Texto(280);
        txtFiltro.TextChanged += (s, e) => Cargar();
        filtros.Controls.Add(txtFiltro);
        chkDeudores = new CheckBox { Text = "Solo los que deben", AutoSize = true, Margin = new Padding(14, 9, 4, 4) };
        chkDeudores.CheckedChanged += (s, e) => Cargar();
        filtros.Controls.Add(chkDeudores);
        lblTotal = UI.Etiqueta("", 10f, true);
        lblTotal.ForeColor = UI.Peligro;
        filtros.Controls.Add(lblTotal);

        grid = UI.Grilla();
        UI.Configurar(grid, ocultas: new[] { "id" }, moneda: new[] { "Límite", "Saldo" },
            extra: g =>
            {
                foreach (DataGridViewRow r in g.Rows)
                {
                    decimal saldo = Db.ADecimal(r.Cells["Saldo"].Value);
                    r.Cells["Saldo"].Style.ForeColor = saldo > 0 ? UI.Peligro : UI.Exito;
                }
            });
        grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) VerCuenta(); };

        var barra = UI.BarraBotones();
        barra.Controls.Add(UI.BotonPrincipal("Ver cuenta y cobrar", VerCuenta, 210));
        barra.Controls.Add(UI.BotonSecundario("Nuevo cliente", Nuevo, 160));
        var opciones = new[]
        {
            new UI.Opcion("Editar datos del cliente", Editar),
            new UI.Opcion("Exportar a Excel", () => UI.ExportarCsv(grid, "clientes")),
            UI.Separador,
            new UI.Opcion("Eliminar cliente", Eliminar, Peligrosa: true)
        };
        barra.Controls.Add(UI.BotonMas(opciones));
        UI.MenuEnGrilla(grid, opciones);

        UI.Ensamblar(this, grid, filtros, barra);
        KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Escape) Close();
            else if (e.KeyCode == Keys.Enter && grid.ContainsFocus) { e.SuppressKeyPress = true; VerCuenta(); }
        };
        Load += (s, e) => Cargar();
    }


    private void Cargar()
    {
        UI.Intentar(() =>
        {
            var t = ClienteRepo.Tabla(txtFiltro.Text.Trim(), chkDeudores.Checked);
            grid.DataSource = t;
            decimal deuda = t.AsEnumerable().Sum(r => Math.Max(0, Db.ADecimal(r["Saldo"])));
            lblTotal.Text = $"Total a cobrar: {UI.Moneda(deuda)}";
        });
    }

    private void Nuevo()
    {
        using var f = new FrmClienteEditar(null);
        if (f.ShowDialog(this) == DialogResult.OK) Cargar();
    }

    private void Editar()
    {
        var id = UI.IdSeleccionado(grid);
        if (id == null) return;
        using var f = new FrmClienteEditar(ClienteRepo.ObtenerPorId(id.Value));
        if (f.ShowDialog(this) == DialogResult.OK) Cargar();
    }

    private void Eliminar()
    {
        var id = UI.IdSeleccionado(grid);
        if (id == null) return;
        var c = ClienteRepo.ObtenerPorId(id.Value);
        decimal saldo = ClienteRepo.Saldo(c.Id);
        string extra = saldo > 0 ? $"\n\n¡OJO! Todavía debe {UI.Moneda(saldo)}." : "";
        if (!UI.Confirmar($"¿Eliminar al cliente \"{c.Nombre}\"?{extra}")) return;
        if (UI.Intentar(() => ClienteRepo.Eliminar(c.Id))) Cargar();
    }

    private void VerCuenta()
    {
        var id = UI.IdSeleccionado(grid);
        if (id == null) return;
        using var f = new FrmCuentaCliente(id.Value);
        f.ShowDialog(this);
        Cargar();
    }
}
