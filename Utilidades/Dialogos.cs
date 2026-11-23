namespace KioscoLaFamilia;

/// <summary>Ventanitas para pedir un dato rápido (texto, número u opción).</summary>
public static class Dialogos
{
    private static T Mostrar<T>(IWin32Window owner, string titulo, string mensaje, Control control, Func<T> leer, T siCancela)
    {
        using var f = new Form();
        UI.PrepararDialogo(f, titulo, 420, 190, false);

        var cuerpo = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(16, 12, 16, 0)
        };
        var lbl = UI.Etiqueta(mensaje);
        lbl.MaximumSize = new Size(380, 0);
        cuerpo.Controls.Add(lbl);
        control.Width = 370;
        cuerpo.Controls.Add(control);

        var barra = UI.BarraBotones(DockStyle.Bottom, true);
        var ok = UI.Boton("Aceptar", UI.Primario, 120);
        UI.ColorearBoton(ok, UI.Primario, destacado: true);
        ok.DialogResult = DialogResult.OK;
        var cancelar = UI.Boton("Cancelar", UI.Neutro, 120);
        cancelar.DialogResult = DialogResult.Cancel;
        barra.Controls.Add(ok);
        barra.Controls.Add(cancelar);

        UI.Ensamblar(f, cuerpo, barra);
        f.AcceptButton = ok;
        f.CancelButton = cancelar;
        f.Shown += (s, e) => control.Focus();

        var r = owner == null ? f.ShowDialog() : f.ShowDialog(owner);
        return r == DialogResult.OK ? leer() : siCancela;
    }

    public static string PedirTexto(IWin32Window owner, string titulo, string mensaje, string valor = "", bool oculto = false)
    {
        var txt = new TextBox { Text = valor, UseSystemPasswordChar = oculto };
        return Mostrar(owner, titulo, mensaje, txt, () => txt.Text.Trim(), null);
    }

    /// <summary>
    /// Pide un número de WhatsApp: no acepta letras y no deja seguir hasta que el número sea válido.
    /// Devuelve el número local (ej. 1181077056) o null si cancela.
    /// </summary>
    public static string PedirTelefono(IWin32Window owner, string titulo, string mensaje, string valor = "")
    {
        while (true)
        {
            var txt = UI.TextoTelefono();
            txt.Text = valor ?? "";
            var numero = Mostrar(owner, titulo, mensaje, txt, () => txt.Text.Trim(), null);
            if (numero == null) return null;
            var error = WhatsApp.Validar(numero);
            if (error == null) return WhatsApp.Local(numero);
            UI.Advertir(error);
            valor = numero;
        }
    }

    public static decimal? PedirNumero(IWin32Window owner, string titulo, string mensaje, decimal valor,
                                       decimal minimo = 0, decimal maximo = 100_000_000m, int decimales = 2)
    {
        var num = UI.Numero(maximo, decimales);
        num.Minimum = minimo;
        num.Value = Math.Clamp(valor, minimo, maximo);
        return Mostrar<decimal?>(owner, titulo, mensaje, num, () => num.Value, null);
    }

    public static string Elegir(IWin32Window owner, string titulo, string mensaje, IEnumerable<string> opciones, string seleccionada = null)
    {
        var cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var o in opciones) cmb.Items.Add(o);
        if (cmb.Items.Count > 0)
            cmb.SelectedIndex = seleccionada != null && cmb.Items.Contains(seleccionada) ? cmb.Items.IndexOf(seleccionada) : 0;
        return Mostrar(owner, titulo, mensaje, cmb, () => cmb.SelectedItem?.ToString(), null);
    }
}

/// <summary>Ventana para cargar el peso de un producto que se vende por kilo.</summary>
public static class DialogoPeso
{
    /// <summary>Devuelve el peso en KILOS (ej. 0,250) o null si se cancela.</summary>
    public static decimal? Pedir(IWin32Window owner, Producto p, decimal kilosIniciales = 0)
    {
        using var f = new Form();
        UI.PrepararDialogo(f, "Peso — " + p.Nombre, 460, 300, false);

        var cuerpo = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(16, 10, 16, 0) };
        cuerpo.Controls.Add(UI.Etiqueta($"{p.Nombre}  —  {UI.Moneda(p.PrecioVenta)} el kilo", 11f, true));
        cuerpo.Controls.Add(UI.Etiqueta("Peso en GRAMOS (lo que marca la balanza):"));

        var num = UI.Numero(100_000, 0, 200);
        num.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
        num.Value = Math.Clamp(Math.Round(kilosIniciales * 1000), 0, 100_000);
        cuerpo.Controls.Add(num);

        var rapidos = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 6, 0, 0) };
        foreach (var g in new[] { 100, 250, 500, 1000 })
        {
            var b = UI.Boton(g < 1000 ? g + " g" : "1 kg", UI.Neutro, 90, 34);
            b.Click += (s, e) => { num.Value = g; num.Focus(); };
            rapidos.Controls.Add(b);
        }
        cuerpo.Controls.Add(rapidos);

        var lblImporte = UI.Etiqueta("Importe: " + UI.Moneda(0), 14f, true);
        lblImporte.ForeColor = UI.Exito;
        cuerpo.Controls.Add(lblImporte);
        void Recalcular()
        {
            decimal gramos = num.Value; // leer Value valida lo escrito
            lblImporte.Text = "Importe: " + UI.Moneda(Math.Round(gramos / 1000m * p.PrecioVenta, 2));
        }
        num.ValueChanged += (s, e) => Recalcular();
        num.KeyUp += (s, e) => Recalcular();
        Recalcular();

        var barra = UI.BarraBotones(DockStyle.Bottom, true);
        var ok = UI.Boton("Agregar", UI.Exito, 120);
        UI.ColorearBoton(ok, UI.Exito, destacado: true);
        ok.DialogResult = DialogResult.OK;
        var cancelar = UI.Boton("Cancelar", UI.Neutro, 120);
        cancelar.DialogResult = DialogResult.Cancel;
        barra.Controls.Add(ok);
        barra.Controls.Add(cancelar);

        UI.Ensamblar(f, cuerpo, barra);
        f.AcceptButton = ok;
        f.CancelButton = cancelar;
        f.Shown += (s, e) => num.Focus();

        if (f.ShowDialog(owner) != DialogResult.OK) return null;
        decimal resultado = num.Value;
        return resultado <= 0 ? null : resultado / 1000m;
    }
}
