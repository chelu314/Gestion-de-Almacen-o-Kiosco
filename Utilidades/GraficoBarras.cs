namespace KioscoLaFamilia;

/// <summary>Gráfico de barras dobles (ventas vs. ganancia) dibujado con GDI+, sin librerías externas.</summary>
public class GraficoBarras : Control
{
    private string[] _etiquetas = Array.Empty<string>();
    private decimal[] _serie1 = Array.Empty<decimal>();
    private decimal[] _serie2 = Array.Empty<decimal>();

    public string Nombre1 { get; set; } = "Ventas";
    public string Nombre2 { get; set; } = "Ganancia";

    public GraficoBarras()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
    }

    public void Cargar(string[] etiquetas, decimal[] serie1, decimal[] serie2)
    {
        _etiquetas = etiquetas ?? Array.Empty<string>();
        _serie1 = serie1 ?? Array.Empty<decimal>();
        _serie2 = serie2 ?? Array.Empty<decimal>();
        Invalidate();
    }

    private static string Abreviar(decimal v)
    {
        if (v >= 1_000_000) return "$" + (v / 1_000_000m).ToString("0.#") + "M";
        if (v >= 1_000) return "$" + (v / 1_000m).ToString("0.#") + "k";
        return "$" + v.ToString("0");
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int n = Math.Min(_etiquetas.Length, Math.Min(_serie1.Length, _serie2.Length));
        var area = new Rectangle(64, 34, Width - 80, Height - 70);
        if (n == 0 || area.Width < 60 || area.Height < 60) return;

        decimal max = Math.Max(_serie1.Take(n).DefaultIfEmpty(0).Max(), _serie2.Take(n).DefaultIfEmpty(0).Max());
        if (max <= 0) max = 1;

        using var fuente = new Font("Segoe UI", 8.5f);
        using var lapizGuia = new Pen(Color.FromArgb(225, 225, 225));
        using var pincel1 = new SolidBrush(UI.Primario);
        using var pincel2 = new SolidBrush(UI.Exito);

        // Líneas guía
        for (int i = 0; i <= 4; i++)
        {
            float y = area.Bottom - area.Height * i / 4f;
            g.DrawLine(lapizGuia, area.Left, y, area.Right, y);
            g.DrawString(Abreviar(max * i / 4), fuente, Brushes.Gray, 4, y - 8);
        }

        // Barras
        float anchoGrupo = area.Width / (float)n;
        float anchoBarra = Math.Max(2, anchoGrupo * 0.34f);
        for (int i = 0; i < n; i++)
        {
            float x = area.Left + i * anchoGrupo + anchoGrupo * 0.16f;
            float h1 = (float)(Math.Max(0, _serie1[i]) / max) * area.Height;
            float h2 = (float)(Math.Max(0, _serie2[i]) / max) * area.Height;
            g.FillRectangle(pincel1, x, area.Bottom - h1, anchoBarra, h1);
            g.FillRectangle(pincel2, x + anchoBarra, area.Bottom - h2, anchoBarra, h2);

            var tam = g.MeasureString(_etiquetas[i], fuente);
            g.DrawString(_etiquetas[i], fuente, Brushes.Black, area.Left + i * anchoGrupo + (anchoGrupo - tam.Width) / 2, area.Bottom + 4);
        }

        // Leyenda
        g.FillRectangle(pincel1, area.Left, 10, 14, 14);
        g.DrawString(Nombre1, fuente, Brushes.Black, area.Left + 18, 9);
        g.FillRectangle(pincel2, area.Left + 110, 10, 14, 14);
        g.DrawString(Nombre2, fuente, Brushes.Black, area.Left + 128, 9);
    }
}
