namespace KioscoLaFamilia;

/// <summary>
/// ==================== DISEÑO DEL PROGRAMA ====================
/// Todo lo visual está acá: colores, letras, logo, recuadro y marca de agua.
/// Para cambiar el aspecto de TODAS las pantallas, modificá solo este archivo
/// (y las imágenes de la carpeta Recursos). No hace falta tocar los formularios.
/// =============================================================
/// </summary>
public static class Tema
{
    // ---------- Colores (tomados del logo) ----------
    public static readonly Color Principal   = Color.FromArgb(10, 28, 180);   // azul del logo
    public static readonly Color Acento      = Color.FromArgb(240, 180, 20);  // amarillo de la cinta del logo
    public static readonly Color Amarillo    = Color.FromArgb(245, 166, 35);  // amarillo/naranja del sol
    public static readonly Color Exito       = Color.FromArgb(46, 125, 50);   // botones de cobrar / guardar
    public static readonly Color Peligro     = Color.FromArgb(200, 32, 45);   // eliminar / cancelar / avisos
    public static readonly Color Advertencia = Color.FromArgb(230, 120, 0);
    public static readonly Color Neutro      = Color.FromArgb(84, 110, 122);
    public static readonly Color Violeta     = Color.FromArgb(106, 27, 154);
    public static readonly Color Turquesa    = Color.FromArgb(0, 121, 107);
    public static readonly Color WhatsApp    = Color.FromArgb(37, 211, 102);
    public static readonly Color Fondo       = Color.FromArgb(243, 246, 250); // fondo de las ventanas
    public static readonly Color FondoPanel  = Color.White;                   // barras y paneles
    public static readonly Color FilaAlterna = Color.FromArgb(238, 243, 251); // grillas: una fila sí, una no
    public static readonly Color Seleccion   = Color.FromArgb(187, 214, 245); // grillas: fila seleccionada

    // ---------- Colores de los botones del menú principal ----------
    public static readonly Color MenuVender     = Exito;
    public static readonly Color MenuProductos  = Principal;
    public static readonly Color MenuStock      = Turquesa;
    public static readonly Color MenuClientes   = Violeta;
    public static readonly Color MenuStockBajo  = Neutro;
    public static readonly Color MenuHistorial  = Advertencia;
    public static readonly Color MenuReportes   = Color.FromArgb(55, 71, 79);
    public static readonly Color MenuUsuarios   = Color.FromArgb(93, 64, 55);
    public static readonly Color MenuConfig     = Color.FromArgb(96, 125, 139);
    public static readonly Color MenuMiCuenta   = Color.FromArgb(0, 105, 92);
    public static readonly Color MenuSalir      = Peligro;
    public static readonly Size TamanioBotonMenu = new Size(300, 150);

    // ---------- Estilo de los botones ----------
    // Suaves: fondo claro del color, letra del mismo color más oscura y borde fino.
    // false = botones de color fuerte con letra blanca (como antes).
    public static readonly bool BotonesSuaves = true;
    public const float SuavidadFondo = 0.84f;   // 0 = color fuerte … 1 = blanco
    // La acción principal de cada pantalla (Cobrar, Ingresar, Guardar…) se ve fuerte igual.

    // ---------- Letras ----------
    public const string Fuente = "Segoe UI";
    public const float TamanioFuente = 10f;
    public const string FuenteTicket = "Consolas";

    // ---------- Recuadro de cada formulario ----------
    public static readonly bool MostrarRecuadro = true;
    public static readonly Color ColorRecuadro = Principal;
    public const int GrosorRecuadro = 4;

    // ---------- Encabezado con logo de cada formulario ----------
    public static readonly bool MostrarEncabezado = true;
    public const int AltoEncabezado = 72;
    public static readonly Color FondoEncabezado = Color.White;
    public static readonly Color TextoEncabezado = Principal;
    public static readonly Color LineaEncabezado = Acento;   // línea amarilla debajo del encabezado

    // ---------- Pantalla completa ----------
    public static readonly bool PantallaCompleta = true;          // todas las pantallas ocupan toda la pantalla
    public static readonly bool DialogosPantallaCompleta = true;  // también las de cargar/editar (producto, cliente, etc.)
    public const int AnchoCamposCentrados = 720;                  // ancho del bloque de campos en pantallas de edición

    // ---------- Marca de agua ----------
    public static readonly bool MostrarMarcaDeAgua = true;
    public const float TamanioMarcaDeAgua = 0.75f;   // proporción de la pantalla que ocupa el logo de fondo

    // ---------- Logo en el ticket impreso ----------
    public static readonly bool LogoEnTicket = true;
    public const int AnchoLogoTicket = 150; // en centésimas de pulgada (150 = 3,8 cm)

    // ---------- Archivos de imagen (carpeta Recursos, junto al .exe) ----------
    public const string ArchivoLogo = "logo.png";
    public const string ArchivoMarcaDeAgua = "logo_marca_agua.png";
    public const string ArchivoIcono = "logo.ico";

    // ===== Carga de imágenes (no hace falta modificar) =====
    private static Image _logo, _marca;
    private static Icon _icono;
    private static bool _cargado;

    private static string Ruta(string archivo) => Path.Combine(AppContext.BaseDirectory, "Recursos", archivo);

    private static void Cargar()
    {
        if (_cargado) return;
        _cargado = true;
        try { if (File.Exists(Ruta(ArchivoLogo))) _logo = Image.FromFile(Ruta(ArchivoLogo)); } catch { }
        try { if (File.Exists(Ruta(ArchivoMarcaDeAgua))) _marca = Image.FromFile(Ruta(ArchivoMarcaDeAgua)); } catch { }
        try { if (File.Exists(Ruta(ArchivoIcono))) _icono = new Icon(Ruta(ArchivoIcono)); } catch { }
    }

    /// <summary>Logo a color (null si falta el archivo: el programa sigue funcionando).</summary>
    public static Image Logo { get { Cargar(); return _logo; } }
    public static Image MarcaDeAgua { get { Cargar(); return MostrarMarcaDeAgua ? _marca : null; } }
    public static Icon Icono { get { Cargar(); return _icono; } }
}
