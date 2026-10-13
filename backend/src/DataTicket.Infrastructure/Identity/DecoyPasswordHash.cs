using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace DataTicket.Infrastructure.Identity;

/// <summary>
/// Hash señuelo del login: el de una clave aleatoria que nadie conoce. Se compara contra él cuando no hay
/// contraseña real que verificar, para que el intento cueste lo mismo que uno normal. Uno por proceso
/// (singleton), generado con el hasher de la aplicación para que comparta sus <c>PasswordHasherOptions</c>
/// (formato e iteraciones) con los hashes reales.
/// </summary>
public sealed class DecoyPasswordHash
{
    private const int RandomPasswordHexLength = 64;

    private string? _value;

    /// <summary>
    /// Devuelve el hash señuelo; la primera vez lo genera con <paramref name="passwordHasher"/> (que es de
    /// ámbito, por eso llega como argumento y no por constructor).
    /// </summary>
    public string GetOrCreate(IPasswordHasher<ApplicationUser> passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(passwordHasher);

        var current = Volatile.Read(ref _value);
        if (current is not null)
        {
            return current;
        }

        // Si dos peticiones llegan a la vez, gana una y la otra descarta su hash: ambos son válidos.
        var created = passwordHasher.HashPassword(
            new ApplicationUser(), RandomNumberGenerator.GetHexString(RandomPasswordHexLength));
        return Interlocked.CompareExchange(ref _value, created, null) ?? created;
    }
}
