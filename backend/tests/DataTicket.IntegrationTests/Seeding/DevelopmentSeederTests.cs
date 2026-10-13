using System.Net;
using DataTicket.Infrastructure.Identity;
using DataTicket.Infrastructure.Seeding;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DataTicket.IntegrationTests.Seeding;

/// <summary>
/// Sembrador de Development (B7 del goal del login): al arrancar en Development con <c>Seed:Enabled</c>,
/// crea de forma idempotente el subconjunto mínimo de <c>db.sql</c> §13.1–13.4 con sus mismos IDs. Cada
/// prueba usa una base propia, migrada y SIN el catálogo de pruebas, y lee el resultado con SQL directo.
/// La contraseña sintética nunca se escribe en un mensaje de aserción.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class DevelopmentSeederTests(PostgreSqlContainerFixture postgres)
{
    private const string GenericLoginTitle = "Correo o contraseña incorrectos.";
    private const string SeederLogCategory = "DataTicket.Infrastructure.Seeding.DevelopmentSeeder";

    /// <summary>Las cinco cuentas del subconjunto mínimo, tal como las define <c>db.sql</c> §13.2–13.4.</summary>
    private static readonly TestAccount[] SeededAccounts =
    [
        TestAccounts.Elizabeth, TestAccounts.Laura, TestAccounts.AnaPerez, TestAccounts.Company01Invited,
        TestAccounts.Company01Deactivated,
    ];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Sembrador_crea_el_subconjunto_minimo_en_Development()
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);
        var logs = new CapturingLoggerProvider();
        await using var host = CreateHost(database, Environments.Development, seedEnabled: true, logs: logs);
        Start(host);

        var snapshot = await DatabaseSnapshot.ReadAsync(database, Ct);

        // Exactamente la Empresa 01 y las cinco cuentas, con los IDs, correos, nombres y estados de db.sql.
        Assert.Equal([$"{TestCompanies.Company01.Id}|{TestCompanies.Company01.Name}|True"], snapshot.Companies);
        Assert.Equal(
            SeededAccounts.OrderBy(account => account.Id).Select(Describe),
            snapshot.Users.Select(user =>
                $"{user.Id}|{user.Email}|{user.DisplayName}|{user.AccountStatus}|{user.CompanyId}|confirmado={user.EmailConfirmed}"));
        Assert.Equal(
            SeededAccounts.SelectMany(account => account.Roles.Select(role => $"{account.Id}|{role}")).Order(StringComparer.Ordinal),
            snapshot.UserRoles);

        await using var scope = host.Services.CreateAsyncScope();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        foreach (var user in snapshot.Users)
        {
            var account = SeededAccounts.Single(candidate => candidate.Id == user.Id);
            Assert.Equal(user.Email, user.UserName);
            Assert.Equal(account.Email.ToUpperInvariant(), user.NormalizedEmail);
            Assert.Equal(account.Email.ToUpperInvariant(), user.NormalizedUserName);
            Assert.True(user.LockoutEnabled);
            Assert.False(string.IsNullOrEmpty(user.SecurityStamp));

            if (!account.HasPassword)
            {
                // La cuenta Invited no tiene contraseña hasta activarse (db.sql §13.3, HU-005).
                Assert.Null(user.PasswordHash);
                continue;
            }

            // Hash de Identity (formato V3), nunca la contraseña en claro, y calculado a partir de ella.
            Assert.NotNull(user.PasswordHash);
            Assert.StartsWith("AQAAAA", user.PasswordHash, StringComparison.Ordinal);
            Assert.False(user.PasswordHash.Contains(TestAccounts.SyntheticPassword, StringComparison.Ordinal), "El hash contiene la contraseña.");
            var verification = hasher.VerifyHashedPassword(new ApplicationUser(), user.PasswordHash, TestAccounts.SyntheticPassword);
            Assert.NotEqual(PasswordVerificationResult.Failed, verification);
        }

        // El log resume lo sembrado y no contiene la contraseña.
        Assert.Contains(logs.Entries, entry => entry is { Category: SeederLogCategory, Level: LogLevel.Information });
        Assert.False(
            logs.Entries.Any(entry => entry.Message.Contains(TestAccounts.SyntheticPassword, StringComparison.Ordinal)),
            "El log del host contiene la contraseña sintética.");
    }

    [Fact]
    public async Task Sembrador_es_idempotente_entre_arranques()
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);

        await using (var first = CreateHost(database, Environments.Development, seedEnabled: true))
        {
            Start(first);
        }

        var afterFirstStart = await DatabaseSnapshot.ReadAsync(database, Ct);
        Assert.Equal(SeededAccounts.Length, afterFirstStart.Users.Count);

        await using (var second = CreateHost(database, Environments.Development, seedEnabled: true))
        {
            Start(second);
        }

        // Mismas filas, columna por columna: ni duplicados ni cambios de hash, sello de seguridad o fechas.
        var afterSecondStart = await DatabaseSnapshot.ReadAsync(database, Ct);
        Assert.Equal(afterFirstStart.Users, afterSecondStart.Users);
        Assert.Equal(afterFirstStart.UserRoles, afterSecondStart.UserRoles);
        Assert.Equal(afterFirstStart.Companies, afterSecondStart.Companies);
    }

    /// <summary>Lo que ya existe no se toca (aunque alguien lo haya editado); lo que falta se repone.</summary>
    [Fact]
    public async Task Sembrador_no_modifica_lo_existente_y_repone_lo_que_falta()
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);
        await using (var first = CreateHost(database, Environments.Development, seedEnabled: true))
        {
            Start(first);
        }

        var ana = TestAccounts.AnaPerez;
        var laura = TestAccounts.Laura;
        await ExecuteAsync(database, $"UPDATE identity.users SET display_name = 'Ana Editada' WHERE id = '{ana.Id}'");
        await ExecuteAsync(database, $"DELETE FROM identity.users WHERE id = '{laura.Id}'");
        var edited = await DatabaseSnapshot.ReadAsync(database, Ct);
        Assert.Equal(SeededAccounts.Length - 1, edited.Users.Count);

        await using (var second = CreateHost(database, Environments.Development, seedEnabled: true))
        {
            Start(second);
        }

        var restored = await DatabaseSnapshot.ReadAsync(database, Ct);
        Assert.Equal(SeededAccounts.Length, restored.Users.Count);
        Assert.Equal("Ana Editada", restored.Users.Single(user => user.Id == ana.Id).DisplayName);
        Assert.Equal(edited.Users, restored.Users.Where(user => user.Id != laura.Id));
        Assert.Equal(laura.Email, restored.Users.Single(user => user.Id == laura.Id).Email);
        Assert.Contains($"{laura.Id}|Development", restored.UserRoles);
    }

    /// <summary>
    /// Primera guarda (entorno): con <c>Seed:Enabled = true</c> y la contraseña configurada, fuera de
    /// Development no se crea ninguna empresa ni cuenta, y el log deja una advertencia.
    /// </summary>
    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task Sembrador_no_crea_cuentas_fuera_de_Development(string environment)
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);
        await MigrateAsync(database);
        var logs = new CapturingLoggerProvider();

        await using (var host = CreateHost(
            database, environment, seedEnabled: true, seedPassword: TestAccounts.SyntheticPassword, logs: logs))
        {
            Start(host);
            Assert.True(host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<DevelopmentSeedOptions>>().Value.Enabled);
        }

        var snapshot = await DatabaseSnapshot.ReadAsync(database, Ct);
        Assert.Empty(snapshot.Users);
        Assert.Empty(snapshot.UserRoles);
        Assert.Empty(snapshot.Companies);
        Assert.Contains(logs.Entries, entry => entry is { Category: SeederLogCategory, Level: LogLevel.Warning });
    }

    /// <summary>Segunda guarda (configuración): en Development sin <c>Seed:Enabled</c> tampoco se siembra.</summary>
    [Fact]
    public async Task Sembrador_no_actua_en_Development_sin_la_clave_de_configuracion()
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);

        await using (var host = CreateHost(database, Environments.Development, seedEnabled: false))
        {
            Start(host);
        }

        var snapshot = await DatabaseSnapshot.ReadAsync(database, Ct);
        Assert.Empty(snapshot.Users);
        Assert.Empty(snapshot.Companies);
    }

    /// <summary>
    /// Defensa en profundidad: el propio sembrador se niega a ejecutarse fuera de Development aunque alguien
    /// lo invoque directamente, sin pasar por las guardas del arranque.
    /// </summary>
    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task Sembrador_invocado_directamente_fuera_de_Development_se_niega(string environment)
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);
        await MigrateAsync(database);
        await using var host = CreateHost(database, environment, seedEnabled: false, seedPassword: TestAccounts.SyntheticPassword);
        Start(host);

        await using (var scope = host.Services.CreateAsyncScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentSeeder>();
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => seeder.SeedAsync(Ct));
            Assert.Contains("solo se ejecuta en el entorno Development", exception.Message, StringComparison.Ordinal);
        }

        var snapshot = await DatabaseSnapshot.ReadAsync(database, Ct);
        Assert.Empty(snapshot.Users);
        Assert.Empty(snapshot.Companies);
    }

    [Fact]
    public async Task Sembrador_falla_con_mensaje_claro_si_falta_la_contrasena()
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);
        await using var host = CreateHost(database, Environments.Development, seedEnabled: true, seedPassword: string.Empty);

        // El arranque del backend falla: no queda un stack «a medias» sin cuentas con las que entrar.
        var exception = Assert.ThrowsAny<Exception>(() => Start(host));

        var messages = Flatten(exception).Select(inner => inner.Message).ToList();
        Assert.Contains(messages, message => message.Contains(DevelopmentSeedOptions.DefaultPasswordKey, StringComparison.Ordinal));
        var snapshot = await DatabaseSnapshot.ReadAsync(database, Ct);
        Assert.Empty(snapshot.Users);
        Assert.Empty(snapshot.Companies);
    }

    /// <summary>
    /// Las cuentas sembradas sirven para entrar al stack local: las <c>Active</c> inician sesión con la
    /// contraseña de <c>appsettings.Development.json</c> y <c>/me</c> devuelve su perfil; la <c>Invited</c> y
    /// la <c>Deactivated</c> reciben el mismo 401 genérico que un correo inexistente.
    /// </summary>
    [Fact]
    public async Task Cuentas_sembradas_Active_inician_sesion_y_las_demas_reciben_401_generico()
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);
        await using var host = CreateHost(database, Environments.Development, seedEnabled: true);
        Start(host);

        var expectations = new (TestAccount Account, string Surface, string[] Roles, string[] Teams, Guid? CompanyId)[]
        {
            (TestAccounts.AnaPerez, "client", ["Requester"], [], TestCompanies.Company01.Id),
            (TestAccounts.Elizabeth, "internal", ["ProductManager"], ["Production"], null),
            (TestAccounts.Laura, "internal", [], ["Development"], null),
        };
        foreach (var (account, surface, roles, teams, companyId) in expectations)
        {
            using var browser = new TestBrowser(host);
            using var login = await browser.LoginAsync(account.Email, TestAccounts.SyntheticPassword, Ct);
            Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

            using var me = await browser.GetAsync("/api/auth/me", Ct);
            Assert.Equal(HttpStatusCode.OK, me.StatusCode);
            var body = await ProblemResponse.ReadAsync(me, Ct);
            Assert.Equal(account.Id.ToString(), body["id"]!.GetValue<string>());
            Assert.Equal(account.Email, body["email"]!.GetValue<string>());
            Assert.Equal(account.DisplayName, body["displayName"]!.GetValue<string>());
            Assert.Equal(surface, body["surface"]!.GetValue<string>());
            Assert.Equal(roles, body["roles"]!.AsArray().Select(role => role!.GetValue<string>()));
            Assert.Equal(teams, body["teams"]!.AsArray().Select(team => team!.GetValue<string>()));
            Assert.Equal(companyId?.ToString(), body["company"]?["id"]?.GetValue<string>());
        }

        using var anonymous = new TestBrowser(host);
        using var unknown = await anonymous.LoginAsync("nadie@empresa-sintetica-99.test", TestAccounts.SyntheticPassword, Ct);
        await ProblemResponse.AssertAsync(unknown, HttpStatusCode.Unauthorized, GenericLoginTitle, Ct);
        var genericBody = await ProblemResponse.ReadWithoutTraceIdAsync(unknown, Ct);

        foreach (var account in new[] { TestAccounts.Company01Invited, TestAccounts.Company01Deactivated })
        {
            using var rejected = await anonymous.LoginAsync(account.Email, TestAccounts.SyntheticPassword, Ct);
            await ProblemResponse.AssertAsync(rejected, HttpStatusCode.Unauthorized, GenericLoginTitle, Ct);
            Assert.Equal(genericBody, await ProblemResponse.ReadWithoutTraceIdAsync(rejected, Ct));
            Assert.Null(TestBrowser.FindSetCookie(rejected, TestBrowser.AuthCookie));
        }
    }

    private static string Describe(TestAccount account) =>
        $"{account.Id}|{account.Email}|{account.DisplayName}|{account.Status}|{account.CompanyId}|confirmado={account.HasPassword}";

    private static DataTicketApiFactory CreateHost(
        TestDatabase database, string environment, bool seedEnabled, string? seedPassword = null, CapturingLoggerProvider? logs = null) =>
        new(new ApiFactoryOptions
        {
            ConnectionString = database.ConnectionString,
            Environment = environment,
            TimeProvider = TestClock.Create(),
            ApplyMigrationsOnStartup = true,
            SeedEnabled = seedEnabled,
            SeedDefaultPassword = seedPassword,
            LoggerProvider = logs,
        });

    /// <summary>Arranca el host: ejecuta <c>Program</c> (migración en Development y sembrador) hasta quedar escuchando.</summary>
    private static void Start(DataTicketApiFactory host) => _ = host.Services;

    /// <summary>Deja la base migrada y vacía (los hosts que no son de Development no migran al arrancar).</summary>
    private static async Task MigrateAsync(TestDatabase database)
    {
        await using var migrator = CreateHost(database, Environments.Development, seedEnabled: false);
        Start(migrator);
    }

    private static async Task ExecuteAsync(TestDatabase database, string sql)
    {
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private static IEnumerable<Exception> Flatten(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            yield return current;
            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions.SelectMany(Flatten))
                {
                    yield return inner;
                }
            }
        }
    }
}
