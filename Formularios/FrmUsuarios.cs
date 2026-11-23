namespace KioscoLaFamilia.Formularios;

/// <summary>Administración de usuarios (solo administrador).</summary>
public class FrmUsuarios : Form
{
    private readonly DataGridView grid;

    public FrmUsuarios()
    {
        UI.PrepararForm(this, "Usuarios del sistema", 860, 500);

        grid = UI.Grilla();
        UI.Configurar(grid, ocultas: new[] { "id" }, fechas: new[] { "Creado" });

        var barra = UI.BarraBotones();
        barra.Controls.Add(UI.BotonPrincipal("Nuevo usuario", Nuevo, 180));
        // El texto cambia según el usuario elegido: "Activar" (cuentas nuevas o dadas de baja) o "Desactivar"
        var btnEstado = UI.BotonSecundario("Activar / desactivar", CambiarEstado, 190);
        barra.Controls.Add(btnEstado);
        grid.SelectionChanged += (s, e) =>
        {
            if (grid.CurrentRow == null || !grid.Columns.Contains("Activo")) return;
            btnEstado.Text = Convert.ToString(grid.CurrentRow.Cells["Activo"].Value) == "Sí" ? "Desactivar usuario" : "Activar usuario";
        };
        var opciones = new[]
        {
            new UI.Opcion("Restablecer contraseña", Resetear),
            new UI.Opcion("Cambiar rol (cajero / admin)", CambiarRol)
        };
        barra.Controls.Add(UI.BotonMas(opciones));
        UI.MenuEnGrilla(grid, opciones);

        UI.Ensamblar(this, grid, barra);
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        Load += (s, e) => Cargar();
    }


    private void Cargar() => UI.Intentar(() => grid.DataSource = UsuarioRepo.Listar());

    private void Nuevo()
    {
        var usuario = Dialogos.PedirTexto(this, "Nuevo usuario", "Nombre de usuario (para ingresar):");
        if (string.IsNullOrWhiteSpace(usuario)) return;
        if (UsuarioRepo.Existe(usuario)) { UI.Advertir("Ese nombre de usuario ya existe."); return; }

        var nombre = Dialogos.PedirTexto(this, "Nuevo usuario", "Nombre y apellido de la persona:");
        if (string.IsNullOrWhiteSpace(nombre)) return;

        var clave = Dialogos.PedirTexto(this, "Nuevo usuario", "Contraseña inicial (mínimo 4 caracteres):", "", true);
        if (string.IsNullOrEmpty(clave)) return;
        if (clave.Length < 4) { UI.Advertir("La contraseña debe tener al menos 4 caracteres."); return; }

        var rol = Dialogos.Elegir(this, "Nuevo usuario", "Rol:", new[] { "cajero", "admin" });
        if (rol == null) return;

        if (UI.Intentar(() => UsuarioRepo.Crear(usuario, nombre, clave, rol)))
        {
            UI.Info($"Usuario \"{usuario}\" creado.");
            Cargar();
        }
    }

    private void Resetear()
    {
        var id = UI.IdSeleccionado(grid);
        if (id == null) return;
        var clave = Dialogos.PedirTexto(this, "Resetear contraseña", "Nueva contraseña (mínimo 4 caracteres):", "", true);
        if (string.IsNullOrEmpty(clave)) return;
        if (clave.Length < 4) { UI.Advertir("La contraseña debe tener al menos 4 caracteres."); return; }
        if (UI.Intentar(() => UsuarioRepo.ResetearClave(id.Value, clave)))
            UI.Info("Contraseña actualizada.");
    }

    private void CambiarEstado()
    {
        var id = UI.IdSeleccionado(grid);
        if (id == null) return;
        if (id.Value == Sesion.Usuario.Id) { UI.Advertir("No podés desactivar tu propio usuario."); return; }

        bool activo = UsuarioRepo.EstaActivo(id.Value);
        if (activo && UsuarioRepo.Rol(id.Value) == "admin" && UsuarioRepo.CantidadAdminsActivos() <= 1)
        {
            UI.Advertir("Tiene que quedar al menos un administrador activo.");
            return;
        }
        if (UI.Intentar(() => UsuarioRepo.CambiarEstado(id.Value, !activo))) Cargar();
    }

    private void CambiarRol()
    {
        var id = UI.IdSeleccionado(grid);
        if (id == null) return;
        if (id.Value == Sesion.Usuario.Id) { UI.Advertir("No podés cambiar tu propio rol."); return; }
        var rol = Dialogos.Elegir(this, "Cambiar rol", "Nuevo rol:", new[] { "cajero", "admin" }, UsuarioRepo.Rol(id.Value));
        if (rol == null) return;
        if (UI.Intentar(() => UsuarioRepo.CambiarRol(id.Value, rol))) Cargar();
    }
}
