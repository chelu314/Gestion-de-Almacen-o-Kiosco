namespace KioscoLaFamilia.Formularios;

/// <summary>Productos en o por debajo del mínimo, con cantidad sugerida a pedir.</summary>
public class FrmStockBajo : Form
{
    private readonly DataGridView grid;
    private List<Producto> productos = new List<Producto>();

    public FrmStockBajo()
    {
        UI.PrepararForm(this, "Productos con poco stock", 900, 560);

        var aviso = new Label
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.FromArgb(255, 235, 238),
            ForeColor = UI.Peligro,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0)
        };

        grid = UI.Grilla();
        UI.Configurar(grid, ocultas: new[] { "Id" }, moneda: new[] { "Costo" },
            titulos: new Dictionary<string, string> { ["Sugerido_pedir"] = "Sugerido a pedir" });

        var barra = UI.BarraBotones();
        // El pedido ya no se arma desde acá: cada proveedor es distinto, se hace desde "Proveedores y pedidos"
        barra.Controls.Add(UI.BotonSecundario("Exportar a Excel", () => UI.ExportarCsv(grid, "stock_bajo"), 170));

        UI.Ensamblar(this, grid, aviso, barra);
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        Load += (s, e) => Cargar(aviso);
    }

    private static decimal Sugerido(Producto p) => Math.Max(1, Math.Ceiling(p.StockMinimo * 2 - p.Stock));

    private void Cargar(Label aviso)
    {
        UI.Intentar(() =>
        {
            productos = ProductoRepo.StockBajo();
            grid.DataSource = productos.Select(p => new
            {
                p.Id,
                Código = p.CodigoBarras,
                Producto = p.Nombre,
                Categoría = p.Categoria,
                Stock = p.StockTexto,
                Mínimo = Cantidades.Formato(p.StockMinimo, p.Unidad),
                Sugerido_pedir = Sugerido(p),
                Costo = p.PrecioCosto
            }).ToList();
            aviso.Text = productos.Count == 0
                ? "Todo en orden: no hay productos por debajo del mínimo."
                : $"⚠ {productos.Count} producto(s) con poco stock. Conviene reponerlos.";
            aviso.ForeColor = productos.Count == 0 ? UI.Exito : UI.Peligro;
            // Fondo verde clarito si está todo bien, rosa si falta algo
            aviso.BackColor = productos.Count == 0 ? Color.FromArgb(232, 245, 233) : Color.FromArgb(255, 235, 238);
        });
    }

   
}
