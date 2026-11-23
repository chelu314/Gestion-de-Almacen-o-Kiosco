namespace KioscoLaFamilia.Datos;

public static class ConfigRepo
{
    private static Dictionary<string, string> _cache;

    private static void Cargar()
    {
        _cache = new Dictionary<string, string>();
        try
        {
            foreach (DataRow r in Db.Tabla("SELECT clave, valor FROM configuracion").Rows)
                _cache[r.Str("clave")] = r.Str("valor");
        }
        catch { /* si no hay conexión se usan los valores por defecto */ }
    }

    /// <summary>Olvida lo leído (por ejemplo después de cambiar de servidor).</summary>
    public static void Reiniciar() => _cache = null;

    public static string Obtener(string clave, string porDefecto = "")
    {
        if (_cache == null) Cargar();
        return _cache.TryGetValue(clave, out var v) && v != "" ? v : porDefecto;
    }

    public static bool ObtenerBool(string clave, bool porDefecto) =>
        Obtener(clave, porDefecto ? "1" : "0") == "1";

    public static int ObtenerInt(string clave, int porDefecto) =>
        int.TryParse(Obtener(clave, porDefecto.ToString()), out var n) ? n : porDefecto;

    public static void Guardar(string clave, string valor)
    {
        Db.Ejecutar("INSERT INTO configuracion (clave, valor) VALUES (@c, @v) ON DUPLICATE KEY UPDATE valor = @v",
            ("@c", clave), ("@v", valor ?? ""));
        if (_cache == null) Cargar();
        _cache[clave] = valor ?? "";
    }
}
