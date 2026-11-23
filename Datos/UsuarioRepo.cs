namespace KioscoLaFamilia.Datos;

public static class UsuarioRepo
{
    private static Usuario Mapear(DataRow r) => new Usuario
    {
        Id = r.Int("id"),
        NombreUsuario = r.Str("usuario"),
        Nombre = r.Str("nombre"),
        Rol = r.Str("rol")
    };

    public static void CrearAdminSiNoHay()
    {
        if (Convert.ToInt32(Db.Escalar("SELECT COUNT(*) FROM usuarios")) == 0)
            Crear("admin", "Administrador", "admin123", "admin");
    }

    public static void Crear(string usuario, string nombre, string clave, string rol, bool activo = true)
    {
        var (hash, salt) = Seguridad.Hashear(clave);
        Db.Ejecutar("INSERT INTO usuarios (usuario, nombre, hash, salt, rol, activo) VALUES (@u, @n, @h, @s, @r, @a)",
            ("@u", usuario), ("@n", nombre), ("@h", hash), ("@s", salt), ("@r", rol), ("@a", activo));
    }

    /// <summary>null = no existe; true = activo; false = desactivado o esperando aprobación.</summary>
    public static bool? Estado(string usuario)
    {
        var v = Db.Escalar("SELECT activo FROM usuarios WHERE usuario = @u", ("@u", usuario));
        return v == null || v == DBNull.Value ? null : Convert.ToInt32(v) == 1;
    }

    /// <summary>Devuelve el usuario si las credenciales son correctas; si no, null.</summary>
    public static Usuario Validar(string usuario, string clave)
    {
        var t = Db.Tabla("SELECT * FROM usuarios WHERE usuario = @u AND activo = 1", ("@u", usuario));
        if (t.Rows.Count == 0) return null;
        var r = t.Rows[0];
        return Seguridad.Verificar(clave, r.Str("hash"), r.Str("salt")) ? Mapear(r) : null;
    }

    public static bool Existe(string usuario, int excluirId = 0) =>
        Convert.ToInt32(Db.Escalar("SELECT COUNT(*) FROM usuarios WHERE usuario = @u AND id <> @id",
            ("@u", usuario), ("@id", excluirId))) > 0;

    /// <summary>Cambia nombre de usuario y, si nuevaClave no es null, también la contraseña.</summary>
    public static void CambiarCredenciales(int id, string nuevoUsuario, string nuevaClave)
    {
        if (string.IsNullOrEmpty(nuevaClave))
        {
            Db.Ejecutar("UPDATE usuarios SET usuario = @u WHERE id = @id", ("@u", nuevoUsuario), ("@id", id));
        }
        else
        {
            var (hash, salt) = Seguridad.Hashear(nuevaClave);
            Db.Ejecutar("UPDATE usuarios SET usuario = @u, hash = @h, salt = @s WHERE id = @id",
                ("@u", nuevoUsuario), ("@h", hash), ("@s", salt), ("@id", id));
        }
    }

    public static void ResetearClave(int id, string nuevaClave)
    {
        var (hash, salt) = Seguridad.Hashear(nuevaClave);
        Db.Ejecutar("UPDATE usuarios SET hash = @h, salt = @s WHERE id = @id", ("@h", hash), ("@s", salt), ("@id", id));
    }

    public static DataTable Listar() => Db.Tabla(
        "SELECT id, usuario AS `Usuario`, nombre AS `Nombre`, rol AS `Rol`, " +
        "IF(activo = 1, 'Sí', 'No') AS `Activo`, creado AS `Creado` FROM usuarios ORDER BY usuario");

    public static void CambiarEstado(int id, bool activo) =>
        Db.Ejecutar("UPDATE usuarios SET activo = @a WHERE id = @id", ("@a", activo), ("@id", id));

    public static void CambiarRol(int id, string rol) =>
        Db.Ejecutar("UPDATE usuarios SET rol = @r WHERE id = @id", ("@r", rol), ("@id", id));

    public static bool EstaActivo(int id) =>
        Convert.ToInt32(Db.Escalar("SELECT activo FROM usuarios WHERE id = @id", ("@id", id))) == 1;

    public static string Rol(int id) => Convert.ToString(Db.Escalar("SELECT rol FROM usuarios WHERE id = @id", ("@id", id)));

    public static int CantidadAdminsActivos() =>
        Convert.ToInt32(Db.Escalar("SELECT COUNT(*) FROM usuarios WHERE rol = 'admin' AND activo = 1"));
}
