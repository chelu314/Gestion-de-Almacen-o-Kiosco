using System.Security.Cryptography;

namespace KioscoLaFamilia.Servicios;

/// <summary>
/// Hash de contraseñas con PBKDF2-SHA256 + salt aleatorio.
/// Primero usa las funciones de Windows (rápidas). Si fallan —pasa en Linux con Wine,
/// que no las imita bien— usa una versión hecha acá mismo, en C#, que da EXACTAMENTE
/// el mismo resultado. Así una contraseña creada en Windows sirve en Linux y al revés.
/// </summary>
public static class Seguridad
{
    private const int Iteraciones = 100_000;
    private const int Largo = 32;

    public static (string Hash, string Salt) Hashear(string clave)
    {
        byte[] salt = SaltAleatorio();
        byte[] hash = Pbkdf2(clave, salt);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool Verificar(string clave, string hashGuardado, string saltGuardado)
    {
        try
        {
            byte[] salt = Convert.FromBase64String(saltGuardado);
            byte[] esperado = Convert.FromBase64String(hashGuardado);
            byte[] calculado = Pbkdf2(clave, salt);
            return CryptographicOperations.FixedTimeEquals(esperado, calculado);
        }
        catch
        {
            return false;
        }
    }

    // ===================================================================
    // Intento rápido con Windows; si falla, la versión propia
    // ===================================================================

    private static byte[] Pbkdf2(string clave, byte[] salt)
    {
        try
        {
            return Rfc2898DeriveBytes.Pbkdf2(clave, salt, Iteraciones, HashAlgorithmName.SHA256, Largo);
        }
        catch
        {
            return Pbkdf2Propio(Encoding.UTF8.GetBytes(clave), salt, Iteraciones);
        }
    }

    private static byte[] SaltAleatorio()
    {
        try
        {
            return RandomNumberGenerator.GetBytes(16);
        }
        catch
        {
            // Un Guid nuevo son 16 bytes al azar: sirve igual como salt
            return Guid.NewGuid().ToByteArray();
        }
    }

    // ===================================================================
    // Versión propia: PBKDF2 → HMAC → SHA-256 (las fórmulas oficiales)
    // ===================================================================

    /// <summary>PBKDF2 con un solo bloque (32 bytes es justo el tamaño de un SHA-256).</summary>
    private static byte[] Pbkdf2Propio(byte[] clave, byte[] salt, int iteraciones)
    {
        // Primera vuelta: HMAC(clave, salt + número de bloque 1 en 4 bytes)
        var mensaje = new byte[salt.Length + 4];
        Buffer.BlockCopy(salt, 0, mensaje, 0, salt.Length);
        mensaje[^1] = 1;

        byte[] u = Hmac(clave, mensaje);
        byte[] resultado = (byte[])u.Clone();

        // Las demás vueltas: cada resultado se mezcla (XOR) con el acumulado
        for (int i = 1; i < iteraciones; i++)
        {
            u = Hmac(clave, u);
            for (int j = 0; j < resultado.Length; j++) resultado[j] ^= u[j];
        }
        return resultado;
    }

    /// <summary>HMAC-SHA256: un SHA-256 "con llave".</summary>
    private static byte[] Hmac(byte[] llave, byte[] mensaje)
    {
        if (llave.Length > 64) llave = Sha256(llave);
        var interno = new byte[64 + mensaje.Length];
        var externo = new byte[64 + 32];
        for (int i = 0; i < 64; i++)
        {
            byte k = i < llave.Length ? llave[i] : (byte)0;
            interno[i] = (byte)(k ^ 0x36);
            externo[i] = (byte)(k ^ 0x5c);
        }
        Buffer.BlockCopy(mensaje, 0, interno, 64, mensaje.Length);
        Buffer.BlockCopy(Sha256(interno), 0, externo, 64, 32);
        return Sha256(externo);
    }

    private static readonly uint[] K =
    {
        0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
        0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
        0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
        0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
        0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
        0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
        0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
        0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
    };

    private static uint Rotar(uint x, int n) => (x >> n) | (x << (32 - n));

    /// <summary>SHA-256 escrito a mano (FIPS 180-4).</summary>
    private static byte[] Sha256(byte[] datos)
    {
        uint[] h =
        {
            0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
            0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19
        };

        // Relleno: un 1, ceros, y el largo en bits (8 bytes) → múltiplo de 64 bytes
        int total = ((datos.Length + 9 + 63) / 64) * 64;
        var m = new byte[total];
        Buffer.BlockCopy(datos, 0, m, 0, datos.Length);
        m[datos.Length] = 0x80;
        ulong bits = (ulong)datos.Length * 8;
        for (int i = 0; i < 8; i++) m[total - 1 - i] = (byte)(bits >> (8 * i));

        var w = new uint[64];
        for (int bloque = 0; bloque < total; bloque += 64)
        {
            for (int i = 0; i < 16; i++)
                w[i] = (uint)(m[bloque + i * 4] << 24 | m[bloque + i * 4 + 1] << 16 |
                              m[bloque + i * 4 + 2] << 8 | m[bloque + i * 4 + 3]);
            for (int i = 16; i < 64; i++)
            {
                uint s0 = Rotar(w[i - 15], 7) ^ Rotar(w[i - 15], 18) ^ (w[i - 15] >> 3);
                uint s1 = Rotar(w[i - 2], 17) ^ Rotar(w[i - 2], 19) ^ (w[i - 2] >> 10);
                w[i] = w[i - 16] + s0 + w[i - 7] + s1;
            }

            uint a = h[0], b = h[1], c = h[2], d = h[3], e = h[4], f = h[5], g = h[6], hh = h[7];
            for (int i = 0; i < 64; i++)
            {
                uint S1 = Rotar(e, 6) ^ Rotar(e, 11) ^ Rotar(e, 25);
                uint ch = (e & f) ^ (~e & g);
                uint t1 = hh + S1 + ch + K[i] + w[i];
                uint S0 = Rotar(a, 2) ^ Rotar(a, 13) ^ Rotar(a, 22);
                uint maj = (a & b) ^ (a & c) ^ (b & c);
                uint t2 = S0 + maj;
                hh = g; g = f; f = e; e = d + t1; d = c; c = b; b = a; a = t1 + t2;
            }
            h[0] += a; h[1] += b; h[2] += c; h[3] += d; h[4] += e; h[5] += f; h[6] += g; h[7] += hh;
        }

        var salida = new byte[32];
        for (int i = 0; i < 8; i++)
        {
            salida[i * 4] = (byte)(h[i] >> 24);
            salida[i * 4 + 1] = (byte)(h[i] >> 16);
            salida[i * 4 + 2] = (byte)(h[i] >> 8);
            salida[i * 4 + 3] = (byte)h[i];
        }
        return salida;
    }
}