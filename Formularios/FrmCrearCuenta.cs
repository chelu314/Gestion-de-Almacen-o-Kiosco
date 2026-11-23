namespace KioscoLaFamilia.Formularios
{
    /// <summary>
    /// Registro de un usuario nuevo desde el login. Se crea como CAJERO.
    /// Si en Configuración está marcado "las cuentas nuevas necesitan aprobación",
    /// queda inactiva hasta que un administrador la active en "Usuarios".
    /// El diseño está en FrmCrearCuenta.Designer.cs.
    /// </summary>
    public partial class FrmCrearCuenta : Form
    {
        public string UsuarioCreado { get; private set; }
        public bool QuedoActiva { get; private set; }

        private readonly bool requiereAprobacion;

        public FrmCrearCuenta()
        {
            InitializeComponent();
            requiereAprobacion = ConfigRepo.ObtenerBool("registro_requiere_aprobacion", true);
            AplicarTema();
        }

        private void AplicarTema()
        {
            Text = "Crear cuenta — " + UI.Negocio;
            if (Tema.Icono != null) Icon = Tema.Icono;
            lblTitulo.ForeColor = Tema.Principal;
            UI.ColorearBoton(btnCrear, Tema.Exito, destacado: true);
            UI.ColorearBoton(btnCancelar, Tema.Neutro);
            lblAyuda.ForeColor = Tema.Neutro;
            lblAyuda.Text = requiereAprobacion
                ? "La cuenta se crea como cajero y queda pendiente hasta que un administrador la active."
                : "La cuenta se crea como cajero y se puede usar enseguida.";
            if (Tema.MostrarRecuadro)
                Paint += (s, e) => Marco.DibujarRecuadro(this, e.Graphics);
        }

        private void chkVerClave_CheckedChanged(object sender, EventArgs e)
        {
            txtClave.UseSystemPasswordChar = !chkVerClave.Checked;
            txtRepetir.UseSystemPasswordChar = !chkVerClave.Checked;
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string usuario = txtUsuario.Text.Trim();

            if (nombre.Length < 3) { Avisar("Escribí tu nombre y apellido.", txtNombre); return; }
            if (usuario.Length < 3) { Avisar("El usuario debe tener al menos 3 letras.", txtUsuario); return; }
            if (usuario.Contains(' ')) { Avisar("El usuario no puede tener espacios.", txtUsuario); return; }
            if (txtClave.Text.Length < 4) { Avisar("La contraseña debe tener al menos 4 caracteres.", txtClave); return; }
            if (txtClave.Text != txtRepetir.Text) { Avisar("Las contraseñas no coinciden.", txtRepetir); return; }

            try
            {
                if (UsuarioRepo.Existe(usuario)) { Avisar("Ese usuario ya existe. Elegí otro.", txtUsuario); return; }
                UsuarioRepo.Crear(usuario, nombre, txtClave.Text, "cajero", activo: !requiereAprobacion);
            }
            catch (Exception ex)
            {
                UI.Error("No se pudo crear la cuenta:\n" + ex.Message);
                return;
            }

            UsuarioCreado = usuario;
            QuedoActiva = !requiereAprobacion;
            DialogResult = DialogResult.OK;
            Close();
        }

        private static void Avisar(string mensaje, Control campo)
        {
            UI.Advertir(mensaje);
            campo.Focus();
        }
    }
}
