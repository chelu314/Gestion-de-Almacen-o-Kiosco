using System.Drawing.Printing;

namespace KioscoLaFamilia.Servicios;

/// <summary>Imprime texto monoespaciado (tickets) en cualquier impresora, incluidas las térmicas.</summary>
public static class Impresion
{
    public static void Imprimir(string texto, IWin32Window owner = null)
    {
        var lineas = texto.Replace("\r", "").Split('\n');
        int indice = 0;

        using var fuente = new Font(Tema.FuenteTicket, 9f);
        using var doc = new PrintDocument { DocumentName = "Ticket" };
        doc.DefaultPageSettings.Margins = new Margins(8, 8, 8, 8);

        bool primeraPagina = true;
        doc.BeginPrint += (s, e) => { indice = 0; primeraPagina = true; };
        doc.PrintPage += (s, e) =>
        {
            float y = e.MarginBounds.Top;

            // Logo arriba del ticket (se configura en Diseno/Tema.cs)
            if (primeraPagina && Tema.LogoEnTicket && Tema.Logo != null)
            {
                float ancho = Math.Min(Tema.AnchoLogoTicket, e.MarginBounds.Width);
                float altoLogo = ancho * Tema.Logo.Height / Tema.Logo.Width;
                float x = e.MarginBounds.Left + (e.MarginBounds.Width - ancho) / 2;
                e.Graphics.DrawImage(Tema.Logo, x, y, ancho, altoLogo);
                y += altoLogo + 6;
            }
            primeraPagina = false;

            float alto = fuente.GetHeight(e.Graphics);
            while (indice < lineas.Length)
            {
                if (y + alto > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }
                e.Graphics.DrawString(lineas[indice], fuente, Brushes.Black, e.MarginBounds.Left, y);
                y += alto;
                indice++;
            }
            e.HasMorePages = false;
        };

        using var dlg = new PrintDialog { Document = doc, UseEXDialog = true };
        var resultado = owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
        if (resultado == DialogResult.OK)
            doc.Print();
    }
}
