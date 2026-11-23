namespace KioscoLaFamilia.Formularios;

/// <summary>Ventas y ganancias por mes, mejor y peor mes, productos más vendidos y sin movimiento.</summary>
public class FrmReportes : Form
{
    // Pestaña meses
    private readonly ComboBox cmbAnio;
    private readonly DataGridView gridMeses;
    private readonly GraficoBarras grafico;
    private readonly Label lblResumen;

    // Pestaña productos
    private readonly SelectorPeriodo periodoTop;
    private readonly DataGridView gridTop;

    // Pestaña por categoría
    private readonly SelectorPeriodo periodoCat;
    private readonly DataGridView gridCategorias;
    private readonly GraficoBarras graficoCat;

    // Pestaña sin ventas
    private readonly NumericUpDown numDias;
    private readonly DataGridView gridSinVentas;

    public FrmReportes()
    {
        UI.PrepararForm(this, "Reportes y ganancias", 1150, 720);
        WindowState = FormWindowState.Maximized;
        var tabs = new TabControl { Font = new Font("Segoe UI", 10.5f) };

        // ================= Ventas por mes =================
        var tabMeses = new TabPage("Ventas y ganancias por mes") { BackColor = UI.Fondo };
        var filtrosMes = UI.BarraFiltros();
        filtrosMes.Controls.Add(UI.Etiqueta("Año:", 11f, true));
        cmbAnio = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
        filtrosMes.Controls.Add(cmbAnio);

        lblResumen = new Label
        {
            Dock = DockStyle.Right,
            Width = 330,
            BackColor = Color.White,
            Padding = new Padding(14),
            Font = new Font("Segoe UI", 10.5f)
        };

        gridMeses = UI.Grilla();
        UI.Configurar(gridMeses,
            ocultas: new[] { "Mes" },
            moneda: new[] { "Total", "Costo", "Ganancia" },
            titulos: new Dictionary<string, string>
            {
                ["NombreMes"] = "Mes",
                ["CantidadVentas"] = "Cant. ventas",
                ["Total"] = "Total vendido",
                ["Costo"] = "Costo mercadería",
                ["MargenPorc"] = "Margen %"
            },
            extra: g =>
            {
                foreach (DataGridViewRow r in g.Rows)
                {
                    var c = r.Cells["Ganancia"];
                    c.Style.ForeColor = Db.ADecimal(c.Value) >= 0 ? UI.Exito : UI.Peligro;
                    c.Style.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                }
            });

        grafico = new GraficoBarras { Dock = DockStyle.Bottom, Height = 260 };

        var centroMes = new Panel();
        UI.Ensamblar(centroMes, gridMeses, grafico);
        UI.Ensamblar(tabMeses, centroMes, filtrosMes, lblResumen);
        cmbAnio.SelectedIndexChanged += (s, e) => CargarMeses();

        // ================= Productos más vendidos =================
        var tabTop = new TabPage("Productos más vendidos") { BackColor = UI.Fondo };
        var filtrosTop = UI.BarraFiltros();
        periodoTop = new SelectorPeriodo("Este mes");
        periodoTop.Cambio += (s, e) => CargarTop();
        filtrosTop.Controls.Add(periodoTop);
        gridTop = UI.Grilla();
        UI.Configurar(gridTop, moneda: new[] { "Vendido", "Ganancia" }, cantidades: new[] { "Unidades" });
        UI.Ensamblar(tabTop, gridTop, filtrosTop);

        // ================= Por tipo de producto =================
        var tabCat = new TabPage("Ventas por tipo de producto") { BackColor = UI.Fondo };
        var filtrosCat = UI.BarraFiltros();
        periodoCat = new SelectorPeriodo("Este mes");
        periodoCat.Cambio += (s, e) => CargarCategorias();
        filtrosCat.Controls.Add(periodoCat);
        gridCategorias = UI.Grilla();
        UI.Configurar(gridCategorias, moneda: new[] { "Vendido", "Ganancia" });
        graficoCat = new GraficoBarras { Dock = DockStyle.Bottom, Height = 260 };
        var centroCat = new Panel();
        UI.Ensamblar(centroCat, gridCategorias, graficoCat);
        UI.Ensamblar(tabCat, centroCat, filtrosCat);

        // ================= Sin ventas =================
        var tabSin = new TabPage("Productos sin ventas") { BackColor = UI.Fondo };
        var filtrosSin = UI.BarraFiltros();
        filtrosSin.Controls.Add(UI.Etiqueta("Sin ventas en los últimos"));
        numDias = UI.Numero(3650, 0, 80);
        numDias.Value = 30;
        filtrosSin.Controls.Add(numDias);
        filtrosSin.Controls.Add(UI.Etiqueta("días"));
        numDias.ValueChanged += (s, e) => CargarSinVentas();
        gridSinVentas = UI.Grilla();
        UI.Configurar(gridSinVentas, fechas: new[] { "Última venta" });
        UI.Ensamblar(tabSin, gridSinVentas, filtrosSin);

        tabs.TabPages.AddRange(new[] { tabMeses, tabCat, tabTop, tabSin });
        // Un solo "Exportar a Excel" abajo: exporta la pestaña que estás mirando
        var barra = UI.BarraBotones();
        barra.Controls.Add(UI.BotonSecundario("Exportar a Excel", () =>
        {
            var (grilla, nombre) = tabs.SelectedIndex switch
            {
                0 => (gridMeses, "ventas_por_mes_" + cmbAnio.Text),
                1 => (gridCategorias, "ventas_por_tipo"),
                2 => (gridTop, "mas_vendidos"),
                _ => (gridSinVentas, "sin_ventas")
            };
            UI.ExportarCsv(grilla, nombre);
        }, 180));
        UI.Ensamblar(this, tabs, barra);

        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        Load += (s, e) =>
        {
            UI.Intentar(() =>
            {
                foreach (var a in ReporteRepo.AniosConVentas()) cmbAnio.Items.Add(a);
                cmbAnio.SelectedItem = DateTime.Today.Year; // dispara CargarMeses
            });
            CargarCategorias();
            CargarTop();
            CargarSinVentas();
        };
    }

    private void CargarMeses()
    {
        if (cmbAnio.SelectedItem == null) return;
        int anio = (int)cmbAnio.SelectedItem;

        UI.Intentar(() =>
        {
            var meses = ReporteRepo.VentasPorMes(anio);
            gridMeses.DataSource = meses;
            grafico.Cargar(
                meses.Select(m => m.NombreMes.Substring(0, 3)).ToArray(),
                meses.Select(m => m.Total).ToArray(),
                meses.Select(m => m.Ganancia).ToArray());

            // Meses a comparar: los ya terminados (el mes en curso está incompleto)
            bool anioActual = anio == DateTime.Today.Year;
            var cerrados = meses.Where(m => anio < DateTime.Today.Year || (anioActual && m.Mes < DateTime.Today.Month)).ToList();
            var comparables = cerrados.Count > 0 ? cerrados : meses.Where(m => m.CantidadVentas > 0).ToList();

            decimal totalAnio = meses.Sum(m => m.Total);
            decimal gananciaAnio = meses.Sum(m => m.Ganancia);
            int mesesConVentas = meses.Count(m => m.CantidadVentas > 0);

            var sb = new StringBuilder();
            sb.AppendLine($"RESUMEN {anio}");
            sb.AppendLine();
            sb.AppendLine("Total vendido:");
            sb.AppendLine("   " + UI.Moneda(totalAnio));
            sb.AppendLine("Ganancia total:");
            sb.AppendLine("   " + UI.Moneda(gananciaAnio));
            sb.AppendLine("Margen promedio: " + (totalAnio == 0 ? "0" : (gananciaAnio / totalAnio * 100).ToString("0.0")) + " %");
            sb.AppendLine("Promedio mensual:");
            sb.AppendLine("   " + UI.Moneda(mesesConVentas == 0 ? 0 : totalAnio / mesesConVentas));
            sb.AppendLine();
            if (comparables.Count > 0)
            {
                var mejor = comparables.OrderByDescending(m => m.Total).First();
                var peor = comparables.OrderBy(m => m.Total).First();
                var masGanancia = comparables.OrderByDescending(m => m.Ganancia).First();
                sb.AppendLine("▲ MES CON MÁS VENTAS:");
                sb.AppendLine($"   {mejor.NombreMes}: {UI.Moneda(mejor.Total)}");
                sb.AppendLine("▼ MES CON MENOS VENTAS:");
                sb.AppendLine($"   {peor.NombreMes}: {UI.Moneda(peor.Total)}");
                sb.AppendLine("$ MES CON MÁS GANANCIA:");
                sb.AppendLine($"   {masGanancia.NombreMes}: {UI.Moneda(masGanancia.Ganancia)}");
                if (anioActual && cerrados.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("(Se comparan los meses ya cerrados;");
                    sb.AppendLine(" el mes en curso todavía no terminó.)");
                }
            }
            else
            {
                sb.AppendLine("Todavía no hay meses para comparar.");
            }
            lblResumen.Text = sb.ToString();
        });
    }

    private void CargarCategorias()
    {
        UI.Intentar(() =>
        {
            var t = ReporteRepo.VentasPorCategoria(periodoCat.Desde, periodoCat.Hasta);
            gridCategorias.DataSource = t;
            var filas = t.AsEnumerable().ToList();
            graficoCat.Cargar(
                filas.Select(r => r["Categoría"].ToString()).ToArray(),
                filas.Select(r => Db.ADecimal(r["Vendido"])).ToArray(),
                filas.Select(r => Db.ADecimal(r["Ganancia"])).ToArray());
        });
    }

    private void CargarTop() =>
        UI.Intentar(() => gridTop.DataSource = ReporteRepo.ProductosMasVendidos(periodoTop.Desde, periodoTop.Hasta));

    private void CargarSinVentas() =>
        UI.Intentar(() => gridSinVentas.DataSource = ReporteRepo.ProductosSinVentas((int)numDias.Value));
}
