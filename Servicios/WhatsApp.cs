namespace KioscoLaFamilia.Servicios;

/// <summary>
/// Envío por WhatsApp y validación de números de teléfono.
/// Por defecto abre WhatsApp WEB directo en el navegador (sin la página de "descargá la aplicación").
/// En Configuración se puede elegir la aplicación de escritorio.
/// </summary>
public static class WhatsApp
{
    private static bool EsArgentina => ConfigRepo.Obtener("codigo_pais", "549") == "549";

    /// <summary>Deja solo los números.</summary>
    public static string SoloDigitos(string texto) => new string((texto ?? "").Where(char.IsDigit).ToArray());

    /// <summary>
    /// Número local sin código de país (en Argentina: 10 números, código de área sin 0 + número sin 15).
    /// Acepta que lo escriban con +54 9, con 0 adelante, con espacios o guiones.
    /// </summary>
    public static string Local(string telefono)
    {
        var d = SoloDigitos(telefono);
        if (d.StartsWith("00")) d = d.Substring(2);
        if (EsArgentina)
        {
            if (d.StartsWith("549") && d.Length == 13) d = d.Substring(3);
            else if (d.StartsWith("54") && d.Length == 12) d = d.Substring(2);
        }
        return d;
    }

    /// <summary>Devuelve null si el número está bien; si no, el motivo en palabras simples.</summary>
    public static string Validar(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono)) return "Escribí el número de teléfono.";
        if (telefono.Any(char.IsLetter)) return "El teléfono solo puede tener números (sin letras).";

        var d = Local(telefono);
        if (!EsArgentina)
            return d.Length >= 6 && d.Length <= 15 ? null : "El número tiene que tener entre 6 y 15 cifras.";

        if (d.StartsWith("0"))
            return "Sacale el 0 del código de área. Ejemplo: 11 en vez de 011.";
        if (d.Length == 12 && d.Substring(2, 2) == "15")
            return "Sacale el 15. Ejemplo: 1181077056 en vez de 111581077056.";
        if (d.Length != 10)
            return $"El número tiene {d.Length} cifras y tienen que ser 10:\n" +
                   "código de área sin 0 + número sin 15.\nEjemplos: 1181077056 (Buenos Aires) o 3794111222 (Corrientes).";
        return null;
    }

    /// <summary>Formato internacional que usa WhatsApp. Ej: 1181077056 → 5491181077056.</summary>
    public static string Internacional(string telefono) => ConfigRepo.Obtener("codigo_pais", "549") + Local(telefono);

    /// <summary>
    /// Abre WhatsApp con el mensaje ya escrito. Devuelve false si el número no es válido.
    /// El texto también queda copiado por si hace falta pegarlo.
    /// </summary>
    public static bool Enviar(string telefono, string texto)
    {
        if (Validar(telefono) != null) return false;
        var numero = Internacional(telefono);

        // Bloque monoespaciado para que las columnas del ticket queden alineadas
        var mensaje = Uri.EscapeDataString("```" + texto.TrimEnd() + "```");
        bool usarApp = ConfigRepo.Obtener("whatsapp_modo", "web") == "app";
        var url = usarApp
            ? $"whatsapp://send?phone={numero}&text={mensaje}"                 // aplicación de escritorio
            : $"https://web.whatsapp.com/send?phone={numero}&text={mensaje}";  // WhatsApp Web directo

        try { Clipboard.SetText(texto); } catch { /* no es crítico */ }
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch when (usarApp)
        {
            // Si la aplicación no está instalada, se usa WhatsApp Web
            Process.Start(new ProcessStartInfo($"https://web.whatsapp.com/send?phone={numero}&text={mensaje}") { UseShellExecute = true });
        }
        return true;
    }
}
