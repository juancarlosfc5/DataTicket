using DataTicket.Infrastructure.Identity;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>HU-003 CHU-13: contraseñas con el hash estándar de Identity, nunca en claro.</summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class PasswordHashTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task Contrasena_se_guarda_con_hash_estandar_de_Identity()
    {
        // Se lee la fila directamente de la tabla, como lo haría un atacante con acceso a la base.
        await using var connection = new NpgsqlConnection(api.Database.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new NpgsqlCommand(
            "SELECT password_hash FROM identity.users WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", TestAccounts.AnaPerez.Id);
        var stored = (string?)await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrEmpty(stored));
        Assert.DoesNotContain(TestAccounts.SyntheticPassword, stored, StringComparison.Ordinal);
        Assert.Equal(0x01, Convert.FromBase64String(stored)[0]); // Formato V3 de Identity (PBKDF2).
        Assert.Equal(
            PasswordVerificationResult.Success,
            new PasswordHasher<ApplicationUser>().VerifyHashedPassword(new ApplicationUser(), stored, TestAccounts.SyntheticPassword));
        Assert.Equal(
            PasswordVerificationResult.Failed,
            new PasswordHasher<ApplicationUser>().VerifyHashedPassword(new ApplicationUser(), stored, "Clave-Incorrecta-2026"));
    }
}
