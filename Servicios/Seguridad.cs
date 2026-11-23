using System.Security.Cryptography;

namespace KioscoLaFamilia.Servicios;

/// <summary>Hash de contraseñas con PBKDF2-SHA256 + salt aleatorio.</summary>
public static class Seguridad
{
    private const int Iteraciones = 100_000;
    private const int Largo = 32;

    public static (string Hash, string Salt) Hashear(string clave)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(clave, salt, Iteraciones, HashAlgorithmName.SHA256, Largo);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool Verificar(string clave, string hashGuardado, string saltGuardado)
    {
        try
        {
            byte[] salt = Convert.FromBase64String(saltGuardado);
            byte[] esperado = Convert.FromBase64String(hashGuardado);
            byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(clave, salt, Iteraciones, HashAlgorithmName.SHA256, Largo);
            return CryptographicOperations.FixedTimeEquals(esperado, calculado);
        }
        catch
        {
            return false;
        }
    }
}
