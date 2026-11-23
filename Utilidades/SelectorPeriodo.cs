namespace KioscoLaFamilia;

/// <summary>
/// Lista desplegable "Período: Hoy / Ayer / Este mes / … / Elegir fechas".
/// Al cambiar la opción (o las fechas) avisa con el evento Cambio: no hace falta botón "Ver".
/// Las fechas solo aparecen con "Elegir fechas…".
/// </summary>
public class SelectorPeriodo : FlowLayoutPanel
{
    private readonly ComboBox cmb;
    private readonly DateTimePicker dtpDesde;
    private readonly DateTimePicker dtpHasta;
    private readonly Label lblHasta;
    private readonly List<(string Nombre, Func<(DateTime Desde, DateTime Hasta)> Rango)> opciones = new();
    private bool silencio;

    public event EventHandler Cambio;

    public DateTime Desde => dtpDesde.Value.Date;
    public DateTime Hasta => dtpHasta.Value.Date;

    /// <param name="extras">Opciones propias que van primero (por ejemplo "Lo que debe").</param>
    /// <param name="seleccion">Nombre de la opción elegida al abrir.</param>
    public SelectorPeriodo(string seleccion = "Hoy", params (string Nombre, Func<(DateTime, DateTime)> Rango)[] extras)
    {
        AutoSize = true;
        WrapContents = false;
        Margin = new Padding(0);
        BackColor = Color.Transparent;

        var hoy = DateTime.Today;
        var inicioSemana = hoy.AddDays(-(((int)hoy.DayOfWeek + 6) % 7));   // lunes
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        foreach (var e in extras) opciones.Add(e);
        opciones.Add(("Hoy", () => (hoy, hoy)));
        opciones.Add(("Ayer", () => (hoy.AddDays(-1), hoy.AddDays(-1))));
        opciones.Add(("Esta semana", () => (inicioSemana, hoy)));
        opciones.Add(("Este mes", () => (inicioMes, hoy)));
        opciones.Add(("Mes pasado", () => (inicioMes.AddMonths(-1), inicioMes.AddDays(-1))));
        opciones.Add(("Este año", () => (new DateTime(hoy.Year, 1, 1), hoy)));
        opciones.Add(("Todo", () => (new DateTime(2000, 1, 1), hoy)));
        opciones.Add(("Elegir fechas…", null));

        Controls.Add(UI.Etiqueta("Período:", 10f, true));
        cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150, Margin = new Padding(4, 6, 4, 4) };
        foreach (var o in opciones) cmb.Items.Add(o.Nombre);
        Controls.Add(cmb);

        dtpDesde = UI.Fecha(hoy);
        lblHasta = UI.Etiqueta("al");
        dtpHasta = UI.Fecha(hoy);
        Controls.Add(dtpDesde);
        Controls.Add(lblHasta);
        Controls.Add(dtpHasta);

        cmb.SelectedIndexChanged += (s, e) => AplicarOpcion();
        dtpDesde.ValueChanged += (s, e) => FechasCambiadas();
        dtpHasta.ValueChanged += (s, e) => FechasCambiadas();

        int i = opciones.FindIndex(o => o.Nombre == seleccion);
        silencio = true;
        cmb.SelectedIndex = i >= 0 ? i : 0;
        silencio = false;
    }

    private bool EligeFechas => opciones[cmb.SelectedIndex].Rango == null;

    private void AplicarOpcion()
    {
        bool manual = EligeFechas;
        dtpDesde.Visible = lblHasta.Visible = dtpHasta.Visible = manual;
        if (!manual)
        {
            var (d, h) = opciones[cmb.SelectedIndex].Rango();
            bool antes = silencio;
            silencio = true;
            dtpDesde.Value = d;
            dtpHasta.Value = h;
            silencio = antes;
        }
        if (!silencio) Cambio?.Invoke(this, EventArgs.Empty);
    }

    private void FechasCambiadas()
    {
        if (silencio || !EligeFechas) return;
        if (dtpDesde.Value.Date > dtpHasta.Value.Date) return;   // espera a que termine de elegir
        Cambio?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Vuelve a calcular el rango de la opción elegida (útil para "Lo que debe" después de un pago).</summary>
    public void Recalcular()
    {
        silencio = true;
        AplicarOpcion();
        silencio = false;
    }
}
