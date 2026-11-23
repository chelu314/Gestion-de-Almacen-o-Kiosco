namespace KioscoLaFamilia.Formularios;

/// <summary>Ventas por fecha, detalle, reimpresión de tickets, anulación y cierre de caja.</summary>
public class FrmHistorialVentas : Form
{
    private readonly SelectorPeriodo periodo;
    private readonly CheckBox chkAnuladas;
    private readonly DataGridView gridVentas;
    private readonly DataGridView gridDetalle;
    private readonly Label lblResumen;

    public FrmHistorialVentas()
    {
        UI.PrepararForm(this, "Historial de ventas", 1150, 700);
        WindowState = FormWindowState.Maximized;

        var filtros = UI.BarraFiltros();
        // Se actualiza solo al cambiar el período: no hace falta botón "Buscar"
        periodo = new SelectorPeriodo("Hoy");
        periodo.Cambio += (s, e) => Cargar();
        filtros.Controls.Add(periodo);
        chkAnuladas = new CheckBox { Text = "Mostrar anuladas", AutoSize = true, Margin = new Padding(16, 9, 4, 4) };
        chkAnuladas.CheckedChanged += (s, e) => Cargar();
        filtros.Controls.Add(chkAnuladas);

        gridVentas = UI.Grilla();
        UI.Configurar(gridVentas, moneda: new[] { "Total" }, fechas: new[] { "Fecha" },
            extra: g =>
            {
                foreach (DataGridViewRow r in g.Rows)
                    if (Convert.ToString(r.Cells["Estado"].Value) == "ANULADA")
                        r.DefaultCellStyle.ForeColor = Color.Gray;
            });
        gridVentas.SelectionChanged += (s, e) => CargarDetalle();
        gridVentas.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) VerTicket(); };

        gridDetalle = UI.Grilla();
        UI.Configurar(gridDetalle, moneda: new[] { "P. unitario", "Subtotal" });

        var split = new SplitContainer { Orientation = Orientation.Horizontal, SplitterWidth = 6 };
        split.Panel1.Controls.Add(gridVentas);
        split.Panel2.Controls.Add(gridDetalle);

        var pie = new Panel { Dock = DockStyle.Bottom, Height = 96, BackColor = Color.White };
        lblResumen = new Label { AutoSize = true, Location = new Point(12, 6), Font = new Font("Segoe UI", 10.5f, FontStyle.Bold) };
        var barra = new FlowLayoutPanel { Location = new Point(6, 36), Width = 1100, Height = 56, WrapContents = false };
        var opciones = new[]
        {
            new UI.Opcion("Exportar a Excel", () => UI.ExportarCsv(gridVentas, "ventas")),
            UI.Separador,
            new UI.Opcion("Anular venta", Anular, Peligrosa: true, Visible: Sesion.EsAdmin)
        };
        barra.Controls.Add(UI.BotonPrincipal("Cierre de caja", CierreCaja, 190));
        barra.Controls.Add(UI.BotonSecundario("Ver ticket", VerTicket, 150));
        barra.Controls.Add(UI.BotonMas(opciones));
        UI.MenuEnGrilla(gridVentas, opciones);
        pie.Controls.Add(lblResumen);
        pie.Controls.Add(barra);

        UI.Ensamblar(this, split, filtros, pie);
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        Load += (s, e) => Cargar();
        Shown += (s, e) => { try { split.SplitterDistance = (int)(split.Height * 0.62); } catch { } };
    }

    private int? VentaSeleccionada => UI.IdSeleccionado(gridVentas, "N°");

    private void Cargar()
    {
        UI.Intentar(() =>
        {
            var t = VentaRepo.Listar(periodo.Desde, periodo.Hasta, chkAnuladas.Checked);
            gridVentas.DataSource = t;
            var activas = t.AsEnumerable().Where(r => r["Estado"].ToString() == "ACTIVA").ToList();
            decimal total = activas.Sum(r => Db.ADecimal(r["Total"]));
            lblResumen.Text = $"{activas.Count} venta(s)   |   Total vendido: {UI.Moneda(total)}   |   " +
                              $"Ticket promedio: {UI.Moneda(activas.Count == 0 ? 0 : total / activas.Count)}";
            CargarDetalle();
        });
    }

    private void CargarDetalle()
    {
        var id = VentaSeleccionada;
        if (id == null) { gridDetalle.DataSource = null; return; }
        UI.Intentar(() => gridDetalle.DataSource = VentaRepo.Detalle(id.Value));
    }

    private void VerTicket()
    {
        var id = VentaSeleccionada;
        if (id == null) return;
        var cab = VentaRepo.Cabecera(id.Value);
        using var f = new FrmTicket("Ticket N° " + id.Value, Ticket.DeVenta(id.Value), cab?.Str("cliente_telefono"));
        f.ShowDialog(this);
    }

    private void CierreCaja()
    {
        using var f = new FrmTicket("Cierre de caja", Ticket.CierreCaja(periodo.Desde, periodo.Hasta));
        f.ShowDialog(this);
    }

    private void Anular()
    {
        var id = VentaSeleccionada;
        if (id == null) return;
        var motivo = Dialogos.PedirTexto(this, "Anular venta",
            $"¿Por qué se anula la venta N° {id.Value}?\n(El stock de los productos se devuelve automáticamente.)");
        if (string.IsNullOrWhiteSpace(motivo)) return;
        if (UI.Intentar(() => VentaRepo.Anular(id.Value, motivo), "No se pudo anular la venta."))
        {
            UI.Info($"Venta N° {id.Value} anulada.");
            Cargar();
        }
    }
}
