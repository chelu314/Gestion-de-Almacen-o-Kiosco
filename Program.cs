namespace KioscoLaFamilia;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Formato argentino: $ 1.234,56 y fechas dd/MM/yyyy
        var cultura = new CultureInfo("es-AR");
        CultureInfo.DefaultThreadCurrentCulture = cultura;
        CultureInfo.DefaultThreadCurrentUICulture = cultura;
        Thread.CurrentThread.CurrentCulture = cultura;
        Thread.CurrentThread.CurrentUICulture = cultura;

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.ThreadException += (s, e) => UI.Error("Ocurrió un error inesperado:\n\n" + e.Exception.Message);

        // Si no hay conexión, se abre la pantalla para cargar el servidor de esta PC
        while (true)
        {
            try
            {
                Db.ProbarConexion();
                ConfigRepo.Reiniciar();
                UsuarioRepo.CrearAdminSiNoHay();
                break;
            }
            catch (Exception ex)
            {
                // El motivo exacto lo muestra la pantalla al probar la conexión
                _ = ex;
                using var conexion = new Formularios.FrmConexion(
                    "No se pudo conectar. Revisá los datos de tu servidor o creá la base.");
                if (conexion.ShowDialog() != DialogResult.OK) return;
            }
        }

        // Ciclo de sesión: permite "Cerrar sesión" y volver al login
        while (true)
        {
            using (var login = new FrmLogin())
            {
                if (login.ShowDialog() != DialogResult.OK) break;
            }

            var principal = new FrmPrincipal();
            Application.Run(principal);
            if (!principal.CerrarSesion) break;
            Sesion.Usuario = null;
        }
    }
}
