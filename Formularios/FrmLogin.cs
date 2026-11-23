namespace KioscoLaFamilia.Formularios
{
    /// <summary>
    /// Ingreso al sistema. El DISEÑO (posiciones, tamaños, textos) está en FrmLogin.Designer.cs
    /// y se edita con el Diseñador de Visual Studio. Acá va solo la LÓGICA.
    /// </summary>
    public partial class FrmLogin : Form
    {
        private int intentosFallidos;
        private DateTime bloqueadoHasta = DateTime.MinValue;

        public FrmLogin()
        {
            InitializeComponent();
            AplicarTema();
        }

        /// <summary>Colores, logo e ícono salen de Diseno/Tema.cs (así cambian en todo el programa a la vez).</summary>
        private void AplicarTema()
        {
            Text = "Ingreso — " + UI.Negocio;
            if (Tema.Icono != null) Icon = Tema.Icono;
            picLogo.Image = Tema.Logo;
            UI.ColorearBoton(btnIngresar, Tema.Principal, destacado: true);
            lblMensaje.ForeColor = Tema.Peligro;
            lnkCrearCuenta.LinkColor = Tema.Principal;
            lnkConexion.LinkColor = Tema.Neutro;
            lblTitulo.ForeColor = Tema.Principal;
            BackColor = Tema.Fondo;
            // Logo de fondo (marca de agua) en toda la pantalla
            if (Tema.MarcaDeAgua != null)
            {
                BackgroundImage = Tema.MarcaDeAgua;
                BackgroundImageLayout = ImageLayout.Zoom;
            }
            if (Tema.MostrarRecuadro)
                Paint += (s, e) => Marco.DibujarRecuadro(this, e.Graphics);
        }

        /// <summary>Mantiene la tarjeta centrada en la pantalla.</summary>
        private void Centrar()
        {
            pnlTarjeta.Left = Math.Max(10, (ClientSize.Width - pnlTarjeta.Width) / 2);
            pnlTarjeta.Top = Math.Max(10, (ClientSize.Height - pnlTarjeta.Height) / 2);
            Invalidate();
        }

        // ================= Eventos =================

        private void FrmLogin_Shown(object sender, EventArgs e)
        {
            Centrar();
            txtUsuario.Focus();
        }

        private void FrmLogin_Resize(object sender, EventArgs e) => Centrar();

        /// <summary>Borde fino gris alrededor de la tarjeta.</summary>
        private void pnlTarjeta_Paint(object sender, PaintEventArgs e)
        {
            using var lapiz = new Pen(Color.FromArgb(215, 222, 232));
            e.Graphics.DrawRectangle(lapiz, 0, 0, pnlTarjeta.Width - 1, pnlTarjeta.Height - 1);
        }

        private void chkVerClave_CheckedChanged(object sender, EventArgs e) =>
            txtClave.UseSystemPasswordChar = !chkVerClave.Checked;

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            Ingresar();
            Centrar();
        }

        private void lnkCrearCuenta_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using var f = new FrmCrearCuenta();
            if (f.ShowDialog(this) != DialogResult.OK) return;
            txtUsuario.Text = f.UsuarioCreado;
            txtClave.Clear();
            lblMensaje.ForeColor = Tema.Exito;
            lblMensaje.Text = f.QuedoActiva
                ? "Cuenta creada. Ya podés ingresar."
                : "Cuenta creada. Un administrador tiene que activarla en \"Usuarios\".";
            txtClave.Focus();
        }

        private void lnkConexion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using var f = new FrmConexion(null);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            UI.Intentar(UsuarioRepo.CrearAdminSiNoHay);
            Text = "Ingreso — " + UI.Negocio;
            lblMensaje.ForeColor = Tema.Exito;
            lblMensaje.Text = "Conexión guardada.";
        }

        // ================= Lógica =================

        private void Ingresar()
        {
            lblMensaje.ForeColor = Tema.Peligro;
            if (DateTime.Now < bloqueadoHasta)
            {
                lblMensaje.Text = $"Demasiados intentos. Esperá {(int)(bloqueadoHasta - DateTime.Now).TotalSeconds + 1} segundos.";
                return;
            }
            string usuario = txtUsuario.Text.Trim();
            if (usuario == "" || txtClave.Text == "")
            {
                lblMensaje.Text = "Ingresá usuario y contraseña.";
                return;
            }

            Usuario u;
            try
            {
                u = UsuarioRepo.Validar(usuario, txtClave.Text);
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "No hay conexión con el servidor. Tocá \"Configurar conexión al servidor\".";
                UI.Error("Error de conexión: " + ex.Message);
                return;
            }

            if (u == null)
            {
                // Si el usuario existe pero está inactivo, avisamos sin contar como intento fallido
                bool? estado = null;
                try { estado = UsuarioRepo.Estado(usuario); } catch { }
                if (estado == false)
                {
                    lblMensaje.Text = "Tu cuenta todavía no está activa. Pedile al administrador que la active.";
                    return;
                }

                intentosFallidos++;
                txtClave.Clear();
                txtClave.Focus();
                if (intentosFallidos >= 5)
                {
                    bloqueadoHasta = DateTime.Now.AddSeconds(30);
                    intentosFallidos = 0;
                    lblMensaje.Text = "Demasiados intentos fallidos. Esperá 30 segundos.";
                }
                else
                {
                    lblMensaje.Text = "Usuario o contraseña incorrectos.";
                }
                return;
            }

            Sesion.Usuario = u;

            if (u.NombreUsuario == "admin" && txtClave.Text == "admin123")
                UI.Advertir("Estás usando la contraseña inicial.\nPor seguridad, cambiala desde \"Mi cuenta\".");

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
