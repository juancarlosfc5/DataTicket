using DataTicket.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Registro de lo que un host le pide al <see cref="IPasswordHasher{TUser}"/>: qué hashes verificó y cuáles
/// generó. Permite probar la defensa de tiempos del login contando operaciones, sin cronómetros.
/// </summary>
public sealed class PasswordHasherSpy
{
    private readonly Lock _gate = new();
    private readonly List<string> _verifiedHashes = [];
    private readonly HashSet<string> _producedHashes = new(StringComparer.Ordinal);

    /// <summary>Hashes pasados a <c>VerifyHashedPassword</c> desde el último <see cref="ResetVerifications"/>.</summary>
    public IReadOnlyList<string> VerifiedHashes
    {
        get
        {
            lock (_gate)
            {
                return [.. _verifiedHashes];
            }
        }
    }

    /// <summary>Si el hash lo generó el hasher de este host (con sus <see cref="PasswordHasherOptions"/>).</summary>
    public bool Produced(string hash)
    {
        lock (_gate)
        {
            return _producedHashes.Contains(hash);
        }
    }

    public void ResetVerifications()
    {
        lock (_gate)
        {
            _verifiedHashes.Clear();
        }
    }

    /// <summary>
    /// Otro host sobre la misma base y el mismo reloj que <paramref name="factory"/>, cuyo hasher de Identity
    /// queda envuelto por este espía (el hasher real sigue haciendo el trabajo). Quien lo crea lo desecha.
    /// </summary>
    public WebApplicationFactory<Program> Attach(WebApplicationFactory<Program> factory) =>
        factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPasswordHasher<ApplicationUser>>();
            services.AddScoped<IPasswordHasher<ApplicationUser>>(provider => new SpyingPasswordHasher(
                this,
                new PasswordHasher<ApplicationUser>(provider.GetRequiredService<IOptions<PasswordHasherOptions>>())));
        }));

    private void RecordVerification(string hash)
    {
        lock (_gate)
        {
            _verifiedHashes.Add(hash);
        }
    }

    private void RecordProduced(string hash)
    {
        lock (_gate)
        {
            _producedHashes.Add(hash);
        }
    }

    private sealed class SpyingPasswordHasher(PasswordHasherSpy spy, IPasswordHasher<ApplicationUser> inner)
        : IPasswordHasher<ApplicationUser>
    {
        public string HashPassword(ApplicationUser user, string password)
        {
            var hash = inner.HashPassword(user, password);
            spy.RecordProduced(hash);
            return hash;
        }

        public PasswordVerificationResult VerifyHashedPassword(
            ApplicationUser user, string hashedPassword, string providedPassword)
        {
            spy.RecordVerification(hashedPassword);
            return inner.VerifyHashedPassword(user, hashedPassword, providedPassword);
        }
    }
}
