namespace KioscoLaFamilia;

/// <summary>
/// Aplica el recuadro, el encabezado con logo y la marca de agua a los formularios.
/// Se llama automáticamente desde UI.PrepararForm, así que cada pantalla lo tiene sin
/// repetir código. Los colores y medidas salen de Tema.cs.
/// </summary>
public static class Marco
{
    public static void Aplicar(Form f, string titulo, bool conEncabezado = true)
    {
        if (Tema.Icono != null) f.Icon = Tema.Icono;

        if (Tema.MostrarRecuadro)
        {
            f.Padding = new Padding(Tema.GrosorRecuadro);
            f.Paint += (s, e) => DibujarRecuadro(f, e.Graphics);
            f.Resize += (s, e) => f.Invalidate();
        }

        // Pantalla completa (las ventanitas de "pedir un dato" quedan chicas)
        if (conEncabezado && Tema.PantallaCompleta)
        {
            f.Load += (s, e) =>
            {
                bool esDialogo = f.FormBorderStyle == FormBorderStyle.FixedDialog;
                if (!esDialogo || Tema.DialogosPantallaCompleta)
                {
                    f.WindowState = FormWindowState.Maximized;
                    if (esDialogo) CentrarCampos(f);
                }
            };
        }

        // Marca de agua en los paneles de fondo
        f.Load += (s, e) => AplicarMarcaDeAgua(f);

        if (conEncabezado && Tema.MostrarEncabezado)
        {
            // Se agrega al cargar, cuando el formulario ya tiene todos sus controles,
            // y se manda "al fondo" para que quede arriba de todo al acoplarse.
            f.Load += (s, e) =>
            {
                var encabezado = Encabezado(titulo);
                f.Controls.Add(encabezado);
                encabezado.SendToBack();
                if (f.WindowState == FormWindowState.Normal)
                {
                    f.ClientSize = new Size(f.ClientSize.Width, f.ClientSize.Height + encabezado.Height);
                    f.Top = Math.Max(0, f.Top - encabezado.Height / 2);
                }
            };
        }
    }

    public static void DibujarRecuadro(Control c, Graphics g)
    {
        if (!Tema.MostrarRecuadro) return;
        using var lapiz = new Pen(Tema.ColorRecuadro, Tema.GrosorRecuadro);
        float m = Tema.GrosorRecuadro / 2f;
        g.DrawRectangle(lapiz, m, m, c.ClientSize.Width - Tema.GrosorRecuadro, c.ClientSize.Height - Tema.GrosorRecuadro);
    }

    /// <summary>Barra superior con el logo, el título de la pantalla y el nombre del negocio.</summary>
    public static Panel Encabezado(string titulo)
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = Tema.AltoEncabezado, BackColor = Tema.FondoEncabezado };
        panel.Paint += (s, e) =>
        {
            using var lapiz = new Pen(Tema.LineaEncabezado, 3);
            e.Graphics.DrawLine(lapiz, 0, panel.Height - 2, panel.Width, panel.Height - 2);
        };

        int x = 12;
        if (Tema.Logo != null)
        {
            var logo = new PictureBox
            {
                Image = Tema.Logo,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(8, 5),
                Size = new Size((int)((Tema.AltoEncabezado - 12) * 1.5), Tema.AltoEncabezado - 12),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(logo);
            x = logo.Right + 12;
        }

        panel.Controls.Add(new Label
        {
            Text = titulo,
            Font = new Font(Tema.Fuente, 16f, FontStyle.Bold),
            ForeColor = Tema.TextoEncabezado,
            AutoSize = true,
            Location = new Point(x, 8)
        });
        panel.Controls.Add(new Label
        {
            Text = UI.Negocio,
            Font = new Font(Tema.Fuente, 9.5f),
            ForeColor = Tema.Neutro,
            AutoSize = true,
            Location = new Point(x + 2, 42)
        });
        return panel;
    }

    /// <summary>Poné este valor en el Tag de un formulario para que quede en blanco, sin logo de fondo.</summary>
    public const string SinMarcaDeAgua = "sin-marca-de-agua";

    /// <summary>En pantallas de edición maximizadas, centra el bloque de campos.</summary>
    private static void CentrarCampos(Form f)
    {
        var campos = f.Controls.OfType<TableLayoutPanel>().FirstOrDefault(t => t.Dock == DockStyle.Fill);
        if (campos == null) return;
        var original = campos.Padding;
        void Ajustar()
        {
            int lado = Math.Max(original.Left, (f.ClientSize.Width - Tema.AnchoCamposCentrados) / 2);
            campos.Padding = new Padding(lado, Math.Max(original.Top, 30), lado, original.Bottom);
        }
        Ajustar();
        f.Resize += (s, e) => Ajustar();
    }

    /// <summary>
    /// Pone el logo de fondo en los paneles grandes del formulario (los que rellenan la pantalla)
    /// y hace transparentes sus etiquetas para que se vea el logo detrás.
    /// </summary>
    public static void AplicarMarcaDeAgua(Control raiz)
    {
        if (Tema.MarcaDeAgua == null) return;
        if (raiz.Tag as string == SinMarcaDeAgua) return;   // pantallas que van en blanco (comprobantes)
        foreach (Control c in raiz.Controls)
        {
            if (c is DataGridView || c is TextBoxBase) continue;
            // Panel incluye FlowLayoutPanel, TableLayoutPanel y TabPage
            bool panelDeFondo = c is Panel && c.Dock == DockStyle.Fill && c.BackgroundImage == null;
            if (panelDeFondo)
            {
                MarcaDeAgua(c);
                ActivarDobleBuffer(c);
                foreach (Control hijo in c.Controls)
                    if (hijo is Label || hijo is CheckBox || hijo is RadioButton)
                        hijo.BackColor = Color.Transparent;
            }
            AplicarMarcaDeAgua(c);
        }
    }

    /// <summary>Dibuja el logo transparente en el centro de una grilla (encima de las filas, muy suave).</summary>
    public static void MarcaDeAguaEnGrilla(DataGridView g)
    {
        g.Paint += (s, e) =>
        {
            var img = Tema.MarcaDeAgua;
            if (img == null) return;
            float max = Math.Min(g.ClientSize.Width, g.ClientSize.Height) * Tema.TamanioMarcaDeAgua;
            if (max < 80) return;
            float escala = Math.Min(max / img.Width, max / img.Height);
            float w = img.Width * escala, h = img.Height * escala;
            e.Graphics.DrawImage(img, (g.ClientSize.Width - w) / 2, (g.ClientSize.Height - h) / 2, w, h);
        };
        g.Scroll += (s, e) => g.Invalidate();
        g.Resize += (s, e) => g.Invalidate();
    }

    private static void ActivarDobleBuffer(Control c)
    {
        typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(c, true);
    }

    /// <summary>Pone el logo transparente de fondo en un panel.</summary>
    public static void MarcaDeAgua(Control c, ImageLayout distribucion = ImageLayout.Zoom)
    {
        if (Tema.MarcaDeAgua == null) return;
        c.BackgroundImage = Tema.MarcaDeAgua;
        c.BackgroundImageLayout = distribucion;
    }
}
