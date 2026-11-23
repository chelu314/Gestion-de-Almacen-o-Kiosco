using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace KioscoLaFamilia.Servicios;

/// <summary>
/// Se fija en GitHub si hay una versión más nueva del programa y, si la hay, avisa.
/// No instala nada: solo abre la página para descargarla.
/// </summary>
public static class Actualizaciones
{
    // Dirección de la API de GitHub que devuelve el ÚLTIMO release publicado del repositorio.
    // (Funciona solo si el repositorio es público).
    private const string UrlUltimoRelease =
        "https://api.github.com/repos/chelu314/Gestion-de-Almacen-o-Kiosco/releases/latest";

    /// <summary>La versión de ESTE programa: sale de &lt;Version&gt; en el .csproj.</summary>
    public static Version VersionActual
    {
        get
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
            return new Version(v.Major, v.Minor, Math.Max(0, v.Build));   // 1.0.0 (sin el 4° número)
        }
    }

    /// <summary>
    /// "async" = trabaja en segundo plano: el programa no se congela mientras pregunta a GitHub.
    /// Si no hay internet o GitHub no responde, no pasa nada: el programa sigue normal.
    /// </summary>
    public static async Task BuscarAsync(IWin32Window owner)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            // GitHub exige que cada programa se identifique con un nombre (User-Agent)
            http.DefaultRequestHeaders.UserAgent.ParseAdd("AlmacenLaTutti");

            string json = await http.GetStringAsync(UrlUltimoRelease);

            // De la respuesta nos interesan 2 datos: el número de versión (tag) y la página
            using var doc = JsonDocument.Parse(json);
            string etiqueta = doc.RootElement.GetProperty("tag_name").GetString();   // ej: "v1.1.0"
            string pagina = doc.RootElement.GetProperty("html_url").GetString();

            // "v1.1.0" → quitamos la "v" y lo convertimos a Version para poder comparar
            if (!Version.TryParse(etiqueta.TrimStart('v', 'V'), out var nueva)) return;
            if (nueva <= VersionActual) return;   // ya tiene la última: no molestamos

            var r = MessageBox.Show(owner,
                $"Hay una versión nueva del programa: {nueva}\n" +
                $"Tenés la versión {VersionActual}.\n\n" +
                "¿Querés abrir la página para descargarla?\n" +
                "(Tus datos no se pierden: están guardados en MySQL).",
                "Actualización disponible", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (r == DialogResult.Yes)
                Process.Start(new ProcessStartInfo(pagina) { UseShellExecute = true });   // abre el navegador
        }
        catch
        {
            // Sin internet, GitHub caído, repositorio privado...: no se avisa nada.
        }
    }
}