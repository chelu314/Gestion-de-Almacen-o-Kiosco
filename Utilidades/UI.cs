namespace KioscoLaFamilia;

/// <summary>
/// Ayudas para armar los formularios por código. Los colores y letras
/// NO se cambian acá: se toman de Diseno/Tema.cs.
/// </summary>
public static class UI
{
    public static Color Primario => Tema.Principal;
    public static Color Exito => Tema.Exito;
    public static Color Peligro => Tema.Peligro;
    public static Color Advertencia => Tema.Advertencia;
    public static Color Neutro => Tema.Neutro;
    public static Color Violeta => Tema.Violeta;
    public static Color Turquesa => Tema.Turquesa;
    public static Color Fondo => Tema.Fondo;

    public static readonly Font Fuente = new Font(Tema.Fuente, Tema.TamanioFuente);
    public static readonly Font FuenteTitulo = new Font(Tema.Fuente, 18f, FontStyle.Bold);
    public static readonly Font FuenteMono = new Font(Tema.FuenteTicket, 10.5f);

    public static string Negocio => ConfigRepo.Obtener("nombre_negocio", "Almacén La Tutti");

    public static string Moneda(decimal d) => d.ToString("C2", CultureInfo.CurrentCulture);

    // ---------- Formularios ----------

    public static void PrepararForm(Form f, string titulo, int ancho, int alto, bool encabezado = true)
    {
        f.Text = titulo + " — " + Negocio;
        f.Font = Fuente;
        f.BackColor = Fondo;
        f.StartPosition = FormStartPosition.CenterParent;
        f.ClientSize = new Size(ancho, alto);
        f.MinimumSize = new Size(Math.Min(ancho, 640), Math.Min(alto, 420));
        f.KeyPreview = true;
        f.ShowInTaskbar = false;
        Marco.Aplicar(f, titulo, encabezado);
    }

    public static void PrepararDialogo(Form f, string titulo, int ancho, int alto, bool encabezado = true)
    {
        PrepararForm(f, titulo, ancho, alto, encabezado);
        f.FormBorderStyle = FormBorderStyle.FixedDialog;
        f.MaximizeBox = false;
        f.MinimizeBox = false;
    }

    /// <summary>Agrega un control "Fill" y los bordes (Top/Bottom/Left/Right) en el orden correcto de acoplado.</summary>
    public static void Ensamblar(Control contenedor, Control relleno, params Control[] bordes)
    {
        foreach (var b in bordes) contenedor.Controls.Add(b);
        relleno.Dock = DockStyle.Fill;
        contenedor.Controls.Add(relleno);
        relleno.BringToFront();
    }

    // ---------- Controles ----------

    public static Button Boton(string texto, Color color, int ancho = 140, int alto = 40)
    {
        var b = new Button
        {
            Text = texto,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Width = ancho,
            Height = alto,
            Cursor = Cursors.Hand,
            Font = new Font(Tema.Fuente, 10f, FontStyle.Bold),
            Margin = new Padding(4),
            UseVisualStyleBackColor = false
        };
        ColorearBoton(b, color);
        return b;
    }

    /// <summary>
    /// Pinta un botón según Tema.BotonesSuaves. "destacado" = acción principal o aviso:
    /// siempre con el color fuerte y letra blanca para que resalte.
    /// </summary>
    public static void ColorearBoton(Button b, Color color, bool destacado = false)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.UseVisualStyleBackColor = false;
        if (destacado || !Tema.BotonesSuaves)
        {
            b.BackColor = color;
            b.ForeColor = Color.White;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Mezclar(color, Color.Black, 0.12f);
            b.FlatAppearance.MouseDownBackColor = Mezclar(color, Color.Black, 0.22f);
            return;
        }
        b.BackColor = Mezclar(color, Color.White, Tema.SuavidadFondo);
        b.ForeColor = Mezclar(color, Color.Black, 0.28f);
        b.FlatAppearance.BorderSize = 1;
        b.FlatAppearance.BorderColor = Mezclar(color, Color.White, 0.45f);
        b.FlatAppearance.MouseOverBackColor = Mezclar(color, Color.White, Tema.SuavidadFondo - 0.12f);
        b.FlatAppearance.MouseDownBackColor = Mezclar(color, Color.White, Tema.SuavidadFondo - 0.25f);
    }

    /// <summary>Mezcla dos colores: t = 0 devuelve a, t = 1 devuelve b.</summary>
    public static Color Mezclar(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return Color.FromArgb(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }

    // ---------- Botones con criterio: 1 principal por pantalla, el resto secundarios ----------

    /// <summary>La acción principal de la pantalla: color fuerte. Debería haber uno solo.</summary>
    public static Button BotonPrincipal(string texto, Action accion, int ancho = 190, Color? color = null)
    {
        var b = Boton(texto, color ?? Tema.Principal, ancho, 42);
        ColorearBoton(b, color ?? Tema.Principal, destacado: true);
        b.Click += (s, e) => accion();
        return b;
    }

    /// <summary>Acciones de uso frecuente pero no principales: tono neutro suave.</summary>
    public static Button BotonSecundario(string texto, Action accion, int ancho = 170, Color? color = null)
    {
        var b = Boton(texto, color ?? Tema.Principal, ancho, 42);
        b.Font = new Font(Tema.Fuente, 10f, FontStyle.Regular);
        b.Click += (s, e) => accion();
        return b;
    }

    /// <summary>Una opción de un menú desplegable. Texto "-" = línea separadora.</summary>
    public record Opcion(string Texto, Action Accion = null, bool Peligrosa = false, bool Visible = true);

    public static readonly Opcion Separador = new Opcion("-");

    public static ContextMenuStrip Menu(params Opcion[] opciones)
    {
        var m = new ContextMenuStrip { Font = new Font(Tema.Fuente, 10f), ShowImageMargin = false };
        foreach (var o in opciones.Where(o => o.Visible))
        {
            if (o.Texto == "-") { m.Items.Add(new ToolStripSeparator()); continue; }
            var item = new ToolStripMenuItem(o.Texto) { Padding = new Padding(6, 4, 6, 4) };
            if (o.Peligrosa) item.ForeColor = Tema.Peligro;
            var accion = o.Accion;
            item.Click += (s, e) => accion?.Invoke();
            m.Items.Add(item);
        }
        return m;
    }

    /// <summary>Botón "Más opciones ▾" que despliega las acciones menos usadas.</summary>
    public static Button BotonMas(params Opcion[] opciones)
    {
        var menu = Menu(opciones);
        var b = Boton("Más opciones ▾", Tema.Neutro, 160, 42);
        b.Font = new Font(Tema.Fuente, 10f, FontStyle.Regular);
        b.Click += (s, e) => menu.Show(b, new Point(0, b.Height));
        return b;
    }

    /// <summary>El mismo menú aparece con clic derecho sobre una fila de la grilla.</summary>
    public static void MenuEnGrilla(DataGridView g, params Opcion[] opciones)
    {
        g.ContextMenuStrip = Menu(opciones);
        g.CellMouseDown += (s, e) =>
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;
            g.ClearSelection();
            g.Rows[e.RowIndex].Selected = true;
            var col = e.ColumnIndex >= 0 && g.Columns[e.ColumnIndex].Visible
                ? e.ColumnIndex
                : g.Columns.Cast<DataGridViewColumn>().First(c => c.Visible).Index;
            g.CurrentCell = g.Rows[e.RowIndex].Cells[col];
        };
    }

    public static Label Etiqueta(string texto, float tamanio = 10f, bool negrita = false) => new Label
    {
        Text = texto,
        AutoSize = true,
        Font = new Font(Tema.Fuente, tamanio, negrita ? FontStyle.Bold : FontStyle.Regular),
        Margin = new Padding(4, 9, 4, 4)
    };

    public static TextBox Texto(int ancho = 260) => new TextBox { Width = ancho, Margin = new Padding(4, 5, 4, 4) };

    /// <summary>
    /// Caja para teléfonos: solo deja escribir números, espacios, guiones y "+".
    /// Si pegan algo con letras, las letras se borran solas.
    /// </summary>
    public static TextBox TextoTelefono(int ancho = 200)
    {
        var t = Texto(ancho);
        t.MaxLength = 20;
        HacerCampoTelefono(t);
        return t;
    }

    public static void HacerCampoTelefono(TextBox t)
    {
        static bool Permitido(char c) => char.IsDigit(c) || c == ' ' || c == '-' || c == '+';
        t.KeyPress += (s, e) =>
        {
            if (char.IsControl(e.KeyChar) || Permitido(e.KeyChar)) return;
            e.Handled = true;
            System.Media.SystemSounds.Beep.Play();
        };
        t.TextChanged += (s, e) =>
        {
            if (t.Text.All(Permitido)) return;
            int pos = t.SelectionStart;
            t.Text = new string(t.Text.Where(Permitido).ToArray());
            t.SelectionStart = Math.Min(pos, t.Text.Length);
        };
    }

    public static NumericUpDown Numero(decimal maximo = 100_000_000m, int decimales = 2, int ancho = 150)
    {
        var n = new NumericUpDown
        {
            Minimum = 0,
            Maximum = maximo,
            DecimalPlaces = decimales,
            ThousandsSeparator = true,
            Width = ancho,
            TextAlign = HorizontalAlignment.Right,
            Margin = new Padding(4, 5, 4, 4)
        };
        // Al entrar se selecciona todo para poder escribir directamente
        n.Enter += (s, e) => n.Select(0, n.Text.Length);
        return n;
    }

    public static DateTimePicker Fecha(DateTime valor) => new DateTimePicker
    {
        Format = DateTimePickerFormat.Short,
        Value = valor,
        Width = 130,
        Margin = new Padding(4, 5, 4, 4)
    };

    public static FlowLayoutPanel BarraBotones(DockStyle dock = DockStyle.Bottom, bool derechaAIzquierda = false) => new FlowLayoutPanel
    {
        Dock = dock,
        Height = 58,
        Padding = new Padding(8, 8, 8, 4),
        FlowDirection = derechaAIzquierda ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
        BackColor = Tema.FondoPanel,
        WrapContents = false
    };

    public static FlowLayoutPanel BarraFiltros() => new FlowLayoutPanel
    {
        Dock = DockStyle.Top,
        Height = 54,
        Padding = new Padding(8, 8, 8, 0),
        BackColor = Tema.FondoPanel,
        WrapContents = false
    };

    /// <summary>Tabla de 2 columnas (etiqueta | control) para formularios de edición.</summary>
    public static TableLayoutPanel TablaCampos()
    {
        var t = new TableLayoutPanel { ColumnCount = 2, Dock = DockStyle.Fill, Padding = new Padding(18, 14, 18, 8), AutoScroll = true };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return t;
    }

    public static void Campo(TableLayoutPanel t, string etiqueta, Control control)
    {
        int fila = t.RowCount;
        t.RowCount = fila + 1;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var l = Etiqueta(etiqueta);
        l.Anchor = AnchorStyles.Left;
        control.Anchor = AnchorStyles.Left;
        t.Controls.Add(l, 0, fila);
        t.Controls.Add(control, 1, fila);
    }

    // ---------- Grillas ----------

    public static DataGridView Grilla()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = 36
        };
        g.ColumnHeadersDefaultCellStyle.BackColor = Primario;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.Font = new Font(Tema.Fuente, 10f, FontStyle.Bold);
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Primario;
        g.RowTemplate.Height = 30;
        g.AlternatingRowsDefaultCellStyle.BackColor = Tema.FilaAlterna;
        g.DefaultCellStyle.SelectionBackColor = Tema.Seleccion;
        g.DefaultCellStyle.SelectionForeColor = Color.Black;
        if (Tema.MostrarMarcaDeAgua) Marco.MarcaDeAguaEnGrilla(g);
        return g;
    }

    /// <summary>
    /// Aplica formato a las columnas cada vez que se cargan datos:
    /// oculta, da formato moneda/fecha y cambia títulos.
    /// </summary>
    public static void Configurar(DataGridView g, string[] ocultas = null, string[] moneda = null,
                                  Dictionary<string, string> titulos = null, string[] fechas = null,
                                  Action<DataGridView> extra = null, string[] cantidades = null)
    {
        g.DataBindingComplete += (s, e) =>
        {
            foreach (DataGridViewColumn c in g.Columns)
            {
                if (ocultas != null && ocultas.Contains(c.Name)) c.Visible = false;
                if (moneda != null && moneda.Contains(c.Name))
                {
                    c.DefaultCellStyle.Format = "C2";
                    c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                if (fechas != null && fechas.Contains(c.Name)) c.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                if (cantidades != null && cantidades.Contains(c.Name))
                {
                    c.DefaultCellStyle.Format = "0.###";
                    c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                if (titulos != null && titulos.TryGetValue(c.Name, out var titulo)) c.HeaderText = titulo;
            }
            extra?.Invoke(g);
        };
    }

    public static int? IdSeleccionado(DataGridView g, string columna = "id")
    {
        if (g.CurrentRow == null || g.CurrentRow.Index < 0) return null;
        var v = g.CurrentRow.Cells[columna].Value;
        return v == null || v == DBNull.Value ? null : Convert.ToInt32(v);
    }

    /// <summary>Exporta las columnas visibles a CSV (separador ; para Excel en español).</summary>
    public static void ExportarCsv(DataGridView g, string nombreSugerido)
    {
        using var dlg = new SaveFileDialog
        {
            Filter = "Archivo CSV (Excel)|*.csv",
            FileName = nombreSugerido + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        var columnas = g.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).OrderBy(c => c.DisplayIndex).ToList();
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(";", columnas.Select(c => Csv(c.HeaderText))));
        foreach (DataGridViewRow fila in g.Rows)
        {
            if (fila.IsNewRow) continue;
            sb.AppendLine(string.Join(";", columnas.Select(c => Csv(fila.Cells[c.Index].FormattedValue?.ToString()))));
        }
        File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
        Info("Archivo exportado:\n" + dlg.FileName);
    }

    private static string Csv(string s)
    {
        s ??= "";
        return s.Contains(';') || s.Contains('"') || s.Contains('\n') ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
    }

    // ---------- Mensajes ----------

    public static void Info(string m) => MessageBox.Show(m, Negocio, MessageBoxButtons.OK, MessageBoxIcon.Information);
    public static void Advertir(string m) => MessageBox.Show(m, Negocio, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public static void Error(string m) => MessageBox.Show(m, Negocio, MessageBoxButtons.OK, MessageBoxIcon.Error);
    public static bool Confirmar(string m) =>
        MessageBox.Show(m, Negocio, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    /// <summary>Ejecuta una acción mostrando un mensaje amigable si falla.</summary>
    public static bool Intentar(Action accion, string contexto = "")
    {
        try
        {
            accion();
            return true;
        }
        catch (Exception ex)
        {
            Error((contexto == "" ? "" : contexto + "\n\n") + ex.Message);
            return false;
        }
    }
}
