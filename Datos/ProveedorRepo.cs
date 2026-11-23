namespace KioscoLaFamilia.Datos;

/// <summary>Un proveedor: la persona o empresa a la que le hacés pedidos.</summary>
public class Proveedor
{
    public int Id { get; set; }              // 0 = proveedor nuevo (todavía no está en la base)
    public string Nombre { get; set; } = "";
    public string Telefono { get; set; } = "";  // WhatsApp, guardado sin 0 ni 15 (ej: 3794111222)
    public string QueVende { get; set; } = "";  // ej: "Lácteos", "Bebidas", "Pan"
}

/// <summary>Todo lo que tiene que ver con la tabla "proveedores" de la base de datos.</summary>
public static class ProveedorRepo
{
    /// <summary>
    /// Crea la tabla si todavía no existe. Se llama al abrir el programa,
    /// así no tenés que correr ningún script a mano en tu base que ya funciona.
    /// </summary>
    public static void CrearTablaSiNoExiste() => Db.Ejecutar(
        "CREATE TABLE IF NOT EXISTS proveedores (" +
        "  id        INT AUTO_INCREMENT PRIMARY KEY," +
        "  nombre    VARCHAR(100) NOT NULL," +
        "  telefono  VARCHAR(30)  NOT NULL DEFAULT ''," +
        "  que_vende VARCHAR(150) NOT NULL DEFAULT ''," +
        "  activo    TINYINT(1)   NOT NULL DEFAULT 1" +
        ")");

    /// <summary>Lista para mostrar en la grilla. "filtro" busca por nombre o por lo que vende.</summary>
    public static DataTable Tabla(string filtro) => Db.Tabla(
        "SELECT id, nombre AS Proveedor, que_vende AS `Qué vende`, telefono AS WhatsApp " +
        "FROM proveedores " +
        "WHERE activo = 1 AND (nombre LIKE @f OR que_vende LIKE @f) " +
        "ORDER BY nombre",
        ("@f", "%" + filtro + "%"));

    /// <summary>Trae un proveedor por su id (o null si no existe).</summary>
    public static Proveedor ObtenerPorId(int id)
    {
        var t = Db.Tabla("SELECT * FROM proveedores WHERE id = @id", ("@id", id));
        if (t.Rows.Count == 0) return null;
        var r = t.Rows[0];
        return new Proveedor
        {
            Id = r.Int("id"),
            Nombre = r.Str("nombre"),
            Telefono = r.Str("telefono"),
            QueVende = r.Str("que_vende")
        };
    }

    /// <summary>Si el Id es 0 lo agrega (INSERT); si no, lo modifica (UPDATE).</summary>
    public static void Guardar(Proveedor p)
    {
        if (p.Id == 0)
        {
            // LAST_INSERT_ID() devuelve el id que MySQL le acaba de dar al proveedor nuevo
            p.Id = Convert.ToInt32(Db.Escalar(
                "INSERT INTO proveedores (nombre, telefono, que_vende) VALUES (@n, @t, @q); SELECT LAST_INSERT_ID();",
                ("@n", p.Nombre), ("@t", p.Telefono), ("@q", p.QueVende)));
        }
        else
        {
            Db.Ejecutar("UPDATE proveedores SET nombre = @n, telefono = @t, que_vende = @q WHERE id = @id",
                ("@n", p.Nombre), ("@t", p.Telefono), ("@q", p.QueVende), ("@id", p.Id));
        }
    }

    /// <summary>No se borra de verdad: se marca como inactivo (igual que con los clientes).</summary>
    public static void Eliminar(int id) =>
        Db.Ejecutar("UPDATE proveedores SET activo = 0 WHERE id = @id", ("@id", id));
}
