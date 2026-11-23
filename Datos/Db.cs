namespace KioscoLaFamilia.Datos;

/// <summary>
/// Acceso a MySQL con consultas parametrizadas.
/// La cadena de conexión se lee de conexion.txt (junto al .exe).
/// </summary>
public static class Db
{
    public const string CadenaPorDefecto =
        "server=localhost;port=3306;database=kiosco_la_familia;uid=root;pwd=;CharSet=utf8mb4;AllowPublicKeyRetrieval=True;";

    private static string _cadena;

    /// <summary>Archivo donde cada PC guarda los datos de SU servidor MySQL.</summary>
    public static string RutaConexion => Path.Combine(AppContext.BaseDirectory, "conexion.txt");

    public static string Cadena
    {
        get
        {
            if (_cadena == null)
            {
                var ruta = RutaConexion;
                _cadena = File.Exists(ruta) && File.ReadAllText(ruta).Trim().Length > 0
                    ? File.ReadAllText(ruta).Trim()
                    : CadenaPorDefecto;
            }
            return _cadena;
        }
    }

    /// <summary>Guarda una nueva cadena de conexión en conexion.txt y la empieza a usar.</summary>
    public static void GuardarCadena(string cadena)
    {
        File.WriteAllText(RutaConexion, cadena);
        _cadena = cadena;
        MySqlConnection.ClearAllPools();
        ConfigRepo.Reiniciar();
    }

    /// <summary>Prueba una cadena de conexión sin guardarla. Lanza excepción si falla.</summary>
    public static void Probar(string cadena)
    {
        using var c = new MySqlConnection(cadena);
        c.Open();
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM configuracion", c);
        cmd.ExecuteScalar();
    }

    /// <summary>
    /// Crea la base de datos en el servidor indicado ejecutando un script de la carpeta BaseDeDatos.
    /// El nombre de la base del script se reemplaza por el elegido.
    /// </summary>
    public static void EjecutarScript(string cadena, string archivoScript, string nombreBase)
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, "BaseDeDatos", archivoScript);
        if (!File.Exists(ruta)) throw new FileNotFoundException("No se encontró el script " + archivoScript, ruta);
        var sql = File.ReadAllText(ruta).Replace("kiosco_la_familia", nombreBase);

        // Conexión al servidor sin elegir base (todavía puede no existir)
        var b = new MySqlConnectionStringBuilder(cadena) { Database = "" };
        using var c = new MySqlConnection(b.ConnectionString);
        c.Open();
        new MySqlScript(c, sql).Execute();
    }

    public static MySqlConnection Abrir()
    {
        var c = new MySqlConnection(Cadena);
        c.Open();
        return c;
    }

    public static void ProbarConexion()
    {
        using var c = Abrir();
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM configuracion", c);
        cmd.ExecuteScalar();
    }

    private static MySqlCommand Comando(MySqlConnection c, MySqlTransaction tx, string sql, (string, object)[] ps)
    {
        var cmd = new MySqlCommand(sql, c, tx);
        foreach (var (nombre, valor) in ps)
            cmd.Parameters.AddWithValue(nombre, valor ?? DBNull.Value);
        return cmd;
    }

    // ----- Operaciones con conexión propia -----

    public static DataTable Tabla(string sql, params (string, object)[] ps)
    {
        using var c = Abrir();
        using var cmd = Comando(c, null, sql, ps);
        using var da = new MySqlDataAdapter(cmd);
        var t = new DataTable();
        da.Fill(t);
        return t;
    }

    public static int Ejecutar(string sql, params (string, object)[] ps)
    {
        using var c = Abrir();
        using var cmd = Comando(c, null, sql, ps);
        return cmd.ExecuteNonQuery();
    }

    public static object Escalar(string sql, params (string, object)[] ps)
    {
        using var c = Abrir();
        using var cmd = Comando(c, null, sql, ps);
        return cmd.ExecuteScalar();
    }

    // ----- Operaciones dentro de una transacción -----

    public static int Ejecutar(MySqlConnection c, MySqlTransaction tx, string sql, params (string, object)[] ps)
    {
        using var cmd = Comando(c, tx, sql, ps);
        return cmd.ExecuteNonQuery();
    }

    public static object Escalar(MySqlConnection c, MySqlTransaction tx, string sql, params (string, object)[] ps)
    {
        using var cmd = Comando(c, tx, sql, ps);
        return cmd.ExecuteScalar();
    }

    public static long Insertar(MySqlConnection c, MySqlTransaction tx, string sql, params (string, object)[] ps)
    {
        using var cmd = Comando(c, tx, sql, ps);
        cmd.ExecuteNonQuery();
        return cmd.LastInsertedId;
    }

    public static DataTable Tabla(MySqlConnection c, MySqlTransaction tx, string sql, params (string, object)[] ps)
    {
        using var cmd = Comando(c, tx, sql, ps);
        using var da = new MySqlDataAdapter(cmd);
        var t = new DataTable();
        da.Fill(t);
        return t;
    }

    /// <summary>True si el error es de clave única duplicada (ej. código de barras repetido).</summary>
    public static bool EsDuplicado(Exception ex) => ex is MySqlException m && m.Number == 1062;

    // ----- Lectura cómoda de DataRow -----

    public static string Str(this DataRow r, string col) => r[col] == DBNull.Value ? "" : r[col].ToString();
    public static int Int(this DataRow r, string col) => r[col] == DBNull.Value ? 0 : Convert.ToInt32(r[col]);
    public static decimal Dec(this DataRow r, string col) => r[col] == DBNull.Value ? 0m : Convert.ToDecimal(r[col]);
    public static bool Bool(this DataRow r, string col) => r[col] != DBNull.Value && Convert.ToBoolean(r[col]);
    public static DateTime Fecha(this DataRow r, string col) => r[col] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r[col]);
    public static decimal ADecimal(object o) => o == null || o == DBNull.Value ? 0m : Convert.ToDecimal(o);
}
