using System.Text.RegularExpressions;

namespace KioscoLaFamilia.Formularios
{
    /// <summary>
    /// Datos del servidor MySQL de ESTA computadora (servidor, puerto, usuario, contraseña y base).
    /// Se guardan en conexion.txt junto al .exe, así cada uno usa el suyo.
    /// Aparece sola si al abrir el programa no hay conexión, y desde el login.
    /// El diseño está en FrmConexion.Designer.cs.
    /// </summary>
    public partial class FrmConexion : Form
    {
        public FrmConexion(string motivo)
        {
            InitializeComponent();
            AplicarTema();
            CargarDatosActuales();
            if (!string.IsNullOrEmpty(motivo))
            {
                lblMensaje.ForeColor = Tema.Peligro;
                lblMensaje.Text = motivo;
                // Al abrirse, prueba sola y explica el motivo abajo (base inexistente, contraseña, etc.)
                Shown += (s, e) => ProbarConexion();
            }
        }

        private void AplicarTema()
        {
            Text = "Conexión a la base de datos — " + UI.Negocio;
            if (Tema.Icono != null) Icon = Tema.Icono;
            lblTitulo.ForeColor = Tema.Principal;
            lblMensaje.ForeColor = Tema.Neutro;
            UI.ColorearBoton(btnProbar, Tema.Principal);
            UI.ColorearBoton(btnCrearBase, Tema.Turquesa);
            UI.ColorearBoton(btnGuardar, Tema.Exito, destacado: true);
            UI.ColorearBoton(btnCancelar, Tema.Neutro);
            if (Tema.MostrarRecuadro)
                Paint += (s, e) => Marco.DibujarRecuadro(this, e.Graphics);
        }

        /// <summary>Muestra lo que ya está guardado en conexion.txt.</summary>
        private void CargarDatosActuales()
        {
            try
            {
                var b = new MySqlConnectionStringBuilder(Db.Cadena);
                txtServidor.Text = b.Server;
                numPuerto.Value = Math.Clamp((int)b.Port, 1, 65535);
                txtUsuario.Text = b.UserID;
                txtClave.Text = b.Password;
                txtBase.Text = b.Database;
            }
            catch { /* si el archivo está mal escrito, quedan los valores por defecto */ }
        }

        private string NombreBase => txtBase.Text.Trim();

        private string ArmarCadena()
        {
            var b = new MySqlConnectionStringBuilder
            {
                Server = txtServidor.Text.Trim(),
                Port = (uint)numPuerto.Value,
                UserID = txtUsuario.Text.Trim(),
                Password = txtClave.Text,
                Database = NombreBase,
                CharacterSet = "utf8mb4",
                AllowPublicKeyRetrieval = true,
                ConnectionTimeout = 8
            };
            return b.ConnectionString;
        }

        private bool DatosValidos()
        {
            if (txtServidor.Text.Trim() == "") { Estado("Escribí el servidor (en la misma PC: localhost).", false); txtServidor.Focus(); return false; }
            if (txtUsuario.Text.Trim() == "") { Estado("Escribí el usuario de MySQL (con XAMPP suele ser root).", false); txtUsuario.Focus(); return false; }
            if (!Regex.IsMatch(NombreBase, "^[A-Za-z0-9_]{1,60}$"))
            {
                Estado("El nombre de la base solo puede tener letras, números y _ (sin espacios).", false);
                txtBase.Focus();
                return false;
            }
            return true;
        }

        private void Estado(string texto, bool ok)
        {
            lblEstado.ForeColor = ok ? Tema.Exito : Tema.Peligro;
            lblEstado.Text = texto;
        }

        /// <summary>Traduce los errores más comunes de MySQL a algo entendible.</summary>
        private static string Explicar(Exception ex)
        {
            var m = ex as MySqlException ?? ex.InnerException as MySqlException;
            return m?.Number switch
            {
                1045 => "Usuario o contraseña de MySQL incorrectos.",
                1049 => "El servidor responde, pero la base no existe. Tocá \"Crear la base de datos\".",
                1146 => "La base existe pero está vacía. Tocá \"Crear la base de datos\".",
                1042 => "No se encontró el servidor. ¿Está iniciado MySQL/XAMPP? ¿Servidor y puerto correctos?",
                _ => "No se pudo conectar: " + ex.Message
            };
        }

        private bool ProbarConexion()
        {
            if (!DatosValidos()) return false;
            Cursor = Cursors.WaitCursor;
            try
            {
                Db.Probar(ArmarCadena());
                Estado("✔ Conexión correcta.", true);
                return true;
            }
            catch (Exception ex)
            {
                Estado(Explicar(ex), false);
                return false;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // ================= Eventos =================

        private void chkVerClave_CheckedChanged(object sender, EventArgs e) =>
            txtClave.UseSystemPasswordChar = !chkVerClave.Checked;

        private void btnProbar_Click(object sender, EventArgs e) => ProbarConexion();

        private void btnCrearBase_Click(object sender, EventArgs e)
        {
            if (!DatosValidos()) return;
            if (!UI.Confirmar($"Se va a crear la base \"{NombreBase}\" con todas sus tablas en el servidor {txtServidor.Text.Trim()}.\n\n" +
                              "Si ya existe, no se borra nada (solo se agregan las tablas que falten).\n\n¿Continuar?"))
                return;

            string cadena = ArmarCadena();
            Cursor = Cursors.WaitCursor;
            try
            {
                Db.EjecutarScript(cadena, "01_crear_base.sql", NombreBase);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                Estado(Explicar(ex), false);
                return;
            }
            Cursor = Cursors.Default;

            if (UI.Confirmar("Base creada.\n\n¿Cargar también los productos del almacén (lista revisada)?"))
            {
                try { Db.EjecutarScript(cadena, "04_productos_almacen.sql", NombreBase); }
                catch (Exception ex) { UI.Advertir("La base se creó, pero no se pudieron cargar los productos:\n" + ex.Message); }
            }
            ProbarConexion();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!DatosValidos()) return;
            if (!ProbarConexion() &&
                !UI.Confirmar("La conexión no funciona con estos datos.\n¿Guardarlos igual?"))
                return;

            try
            {
                Db.GuardarCadena(ArmarCadena());
            }
            catch (Exception ex)
            {
                UI.Error("No se pudo guardar conexion.txt:\n" + ex.Message +
                         "\n\nSi el programa está en \"Archivos de programa\", ejecutalo como administrador o movelo a otra carpeta.");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
