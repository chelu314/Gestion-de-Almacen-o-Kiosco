namespace KioscoLaFamilia.Formularios;

/// <summary>
/// Menú principal. Arriba a la izquierda el logo y el usuario; arriba a la derecha las opciones
/// de la cuenta (Mi cuenta, Usuarios, Configuración, Cerrar sesión) y el aviso de stock bajo.
/// En el centro, solo los accesos de trabajo diario.
/// </summary>
public class FrmPrincipal : Form
{
    public bool CerrarSesion { get; private set; }

    private readonly Button btnAvisoStock;
    private readonly Label lblEstado;

    public FrmPrincipal()
    {
        Text = UI.Negocio + " — Sistema de gestión";
        Font = UI.Fuente;
        BackColor = UI.Fondo;
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(960, 640);
        KeyPreview = true;
        if (Tema.Icono != null) Icon = Tema.Icono;
        if (Tema.MostrarRecuadro)
        {
            Padding = new Padding(Tema.GrosorRecuadro);
            Paint += (s, e) => Marco.DibujarRecuadro(this, e.Graphics);
            Resize += (s, e) => Invalidate();
        }
        bool admin = Sesion.EsAdmin;

        // ===== Encabezado =====
        var encabezado = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Tema.FondoEncabezado };
        encabezado.Paint += (s, e) =>
        {
            using var lapiz = new Pen(Tema.LineaEncabezado, 4);
            e.Graphics.DrawLine(lapiz, 0, encabezado.Height - 2, encabezado.Width, encabezado.Height - 2);
        };
        int x = 24;
        if (Tema.Logo != null)
        {
            var logo = new PictureBox { Image = Tema.Logo, SizeMode = PictureBoxSizeMode.Zoom, Location = new Point(12, 6), Size = new Size(110, 96) };
            encabezado.Controls.Add(logo);
            x = logo.Right + 14;
        }
        encabezado.Controls.Add(new Label
        {
            Text = UI.Negocio,
            ForeColor = Tema.TextoEncabezado,
            Font = new Font(Tema.Fuente, 22f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(x, 16)
        });
        encabezado.Controls.Add(new Label
        {
            Text = $"{Sesion.Usuario.Nombre}  ·  {(admin ? "Administrador" : "Cajero")}  ·  {DateTime.Now:dddd d 'de' MMMM}",
            ForeColor = Tema.Neutro,
            Font = new Font(Tema.Fuente, 11f),
            AutoSize = true,
            Location = new Point(x + 3, 62)
        });

        // Opciones de la cuenta, chicas, a la derecha
        var cuenta = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 34, 16, 0),
            BackColor = Color.Transparent
        };
        cuenta.Controls.Add(Chico("Cerrar sesión", () => { CerrarSesion = true; Close(); }));
        if (admin) cuenta.Controls.Add(Chico("Configuración", () => Abrir(new FrmConfiguracion())));
        if (admin) cuenta.Controls.Add(Chico("Usuarios", () => Abrir(new FrmUsuarios())));
        cuenta.Controls.Add(Chico("Mi cuenta", () => Abrir(new FrmMiCuenta())));
        btnAvisoStock = UI.Boton("", Tema.Peligro, 230, 36);
        UI.ColorearBoton(btnAvisoStock, Tema.Peligro, destacado: true);
        btnAvisoStock.Margin = new Padding(4, 0, 18, 0);
        btnAvisoStock.Visible = false;
        btnAvisoStock.Click += (s, e) => Abrir(new FrmStockBajo());
        cuenta.Controls.Add(btnAvisoStock);
        encabezado.Controls.Add(cuenta);

        // ===== Accesos de trabajo diario =====
        var menu = new FlowLayoutPanel { Padding = new Padding(40, 36, 40, 20), AutoScroll = true, BackColor = Tema.Fondo };
        Marco.MarcaDeAgua(menu);

        menu.Controls.Add(Mosaico("Vender", "Cobrar y emitir tickets  ·  F1", Tema.MenuVender, () => Abrir(new FrmVenta()), principal: true));
        if (admin) menu.Controls.Add(Mosaico("Productos", "Precios, códigos y tipos  ·  F2", Tema.MenuProductos, () => Abrir(new FrmProductos())));
        if (admin) menu.Controls.Add(Mosaico("Stock", "Ingresos, ajustes y faltantes", Tema.MenuStock, () => Abrir(new FrmStock())));
        menu.Controls.Add(Mosaico("Clientes", "Cuentas, pagos y comprobantes  ·  F3", Tema.MenuClientes, () => Abrir(new FrmClientes())));
        menu.Controls.Add(Mosaico("Ventas y caja", "Historial, tickets y cierre de caja", Tema.MenuHistorial, () => Abrir(new FrmHistorialVentas())));
        if (admin) menu.Controls.Add(Mosaico("Reportes", "Ventas y ganancias por mes", Tema.MenuReportes, () => Abrir(new FrmReportes())));

        lblEstado = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 28,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 0, 0),
            ForeColor = Tema.Neutro,
            BackColor = Color.White,
            Text = "Atajos:  F1 Vender   ·   " + (admin ? "F2 Productos   ·   " : "") + "F3 Clientes"
        };

        UI.Ensamblar(this, menu, encabezado, lblEstado);

        KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.F1) Abrir(new FrmVenta());
            else if (e.KeyCode == Keys.F2 && admin) Abrir(new FrmProductos());
            else if (e.KeyCode == Keys.F3) Abrir(new FrmClientes());
        };

        Shown += (s, e) => ActualizarStock(ConfigRepo.ObtenerBool("alerta_stock_inicio", true));
        // Al abrir, se fija en GitHub si hay una versión nueva (en segundo plano, sin trabar nada)
        Shown += async (s, e) => await Actualizaciones.BuscarAsync(this);
    }

    /// <summary>Acceso grande del menú: título y una línea que explica para qué sirve.</summary>
    private static Button Mosaico(string titulo, string descripcion, Color color, Action accion, bool principal = false)
    {
        var b = UI.Boton(titulo + "\n" + descripcion, color, Tema.TamanioBotonMenu.Width, Tema.TamanioBotonMenu.Height);
        UI.ColorearBoton(b, color, destacado: principal);
        b.Font = new Font(Tema.Fuente, 11f, FontStyle.Regular);
        b.Margin = new Padding(12);
        b.Padding = new Padding(10);
        b.TextAlign = ContentAlignment.MiddleCenter;
        // El texto se dibuja aparte: título grande y la descripción chica debajo
        b.AccessibleName = titulo;
        b.Text = "";
        b.Paint += (s, e) =>
        {
            using var fTitulo = new Font(Tema.Fuente, 17f, FontStyle.Bold);
            using var fDesc = new Font(Tema.Fuente, 10f);
            var formato = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var pincel = new SolidBrush(b.ForeColor);
            e.Graphics.DrawString(titulo, fTitulo, pincel, new RectangleF(0, 0, b.Width, b.Height * 0.62f), formato);
            e.Graphics.DrawString(descripcion, fDesc, pincel, new RectangleF(8, b.Height * 0.55f, b.Width - 16, b.Height * 0.4f), formato);
        };
        b.Click += (s, e) => accion();
        return b;
    }

    /// <summary>Botoncito discreto del encabezado.</summary>
    private static Button Chico(string texto, Action accion)
    {
        var b = UI.Boton(texto, Tema.Neutro, 130, 36);
        b.Font = new Font(Tema.Fuente, 9.5f);
        b.Margin = new Padding(4, 0, 4, 0);
        b.Click += (s, e) => accion();
        return b;
    }

    private void Abrir(Form f)
    {
        using (f) f.ShowDialog(this);
        ActualizarStock(false);
    }

    private void ActualizarStock(bool mostrarAviso)
    {
        List<Producto> bajos;
        try { bajos = ProductoRepo.StockBajo(); }
        catch { btnAvisoStock.Visible = false; return; }

        btnAvisoStock.Visible = bajos.Count > 0;
        btnAvisoStock.Text = $"⚠ {bajos.Count} producto(s) con poco stock";

        if (mostrarAviso && bajos.Count > 0)
        {
            var lista = string.Join("\n", bajos.Take(15).Select(p => $"• {p.Nombre}: quedan {p.StockTexto} (mínimo {Cantidades.Formato(p.StockMinimo, p.Unidad)})"));
            if (bajos.Count > 15) lista += $"\n… y {bajos.Count - 15} más.";
            UI.Advertir("Estos productos tienen poco stock:\n\n" + lista + "\n\nTocá el aviso rojo de arriba para ver la lista y armar el pedido.");
        }
    }
}
