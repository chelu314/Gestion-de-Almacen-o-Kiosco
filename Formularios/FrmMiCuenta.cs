namespace KioscoLaFamilia.Formularios;

/// <summary>Cambio de nombre de usuario y contraseña del usuario logueado.</summary>
public class FrmMiCuenta : Form
{
    private readonly TextBox txtUsuario;
    private readonly TextBox txtActual;
    private readonly TextBox txtNueva;
    private readonly TextBox txtRepetir;

    public FrmMiCuenta()
    {
        UI.PrepararDialogo(this, "Mi cuenta", 540, 340);

        var campos = UI.TablaCampos();
        txtUsuario = UI.Texto(260);
        txtUsuario.Text = Sesion.Usuario.NombreUsuario;
        UI.Campo(campos, "Nombre de usuario", txtUsuario);

        txtActual = UI.Texto(260);
        txtActual.UseSystemPasswordChar = true;
        UI.Campo(campos, "Contraseña actual *", txtActual);

        txtNueva = UI.Texto(260);
        txtNueva.UseSystemPasswordChar = true;
        UI.Campo(campos, "Nueva contraseña", txtNueva);

        txtRepetir = UI.Texto(260);
        txtRepetir.UseSystemPasswordChar = true;
        UI.Campo(campos, "Repetir nueva", txtRepetir);

        var ayuda = UI.Etiqueta("Dejá la nueva contraseña vacía si solo querés cambiar el usuario.", 8.5f);
        ayuda.ForeColor = UI.Neutro;
        UI.Campo(campos, "", ayuda);

        var barra = UI.BarraBotones(DockStyle.Bottom, true);
        var btnGuardar = UI.Boton("Guardar cambios", UI.Exito, 160);
        UI.ColorearBoton(btnGuardar, UI.Exito, destacado: true);
        btnGuardar.Click += (s, e) => Guardar();
        var btnCancelar = UI.Boton("Cancelar", UI.Neutro, 120);
        btnCancelar.Click += (s, e) => Close();
        barra.Controls.Add(btnGuardar);
        barra.Controls.Add(btnCancelar);

        UI.Ensamblar(this, campos, barra);
        AcceptButton = btnGuardar;
        CancelButton = btnCancelar;
        Shown += (s, e) => txtActual.Focus();
    }

    private void Guardar()
    {
        var usuario = txtUsuario.Text.Trim();
        if (usuario.Length < 3) { UI.Advertir("El usuario debe tener al menos 3 caracteres."); return; }

        if (UsuarioRepo.Validar(Sesion.Usuario.NombreUsuario, txtActual.Text) == null)
        {
            UI.Advertir("La contraseña actual no es correcta.");
            txtActual.Clear();
            txtActual.Focus();
            return;
        }

        string nueva = null;
        if (txtNueva.Text != "" || txtRepetir.Text != "")
        {
            if (txtNueva.Text.Length < 4) { UI.Advertir("La nueva contraseña debe tener al menos 4 caracteres."); return; }
            if (txtNueva.Text != txtRepetir.Text) { UI.Advertir("Las contraseñas nuevas no coinciden."); return; }
            nueva = txtNueva.Text;
        }

        if (usuario != Sesion.Usuario.NombreUsuario && UsuarioRepo.Existe(usuario, Sesion.Usuario.Id))
        {
            UI.Advertir("Ese nombre de usuario ya lo usa otra persona.");
            return;
        }

        if (!UI.Intentar(() => UsuarioRepo.CambiarCredenciales(Sesion.Usuario.Id, usuario, nueva))) return;

        Sesion.Usuario.NombreUsuario = usuario;
        UI.Info("Datos actualizados. Usalos la próxima vez que ingreses.");
        Close();
    }
}
