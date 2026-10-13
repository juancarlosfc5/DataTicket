using DataTicket.IntegrationTests.Infrastructure;
using Npgsql;

namespace DataTicket.IntegrationTests.Seeding;

/// <summary>Fila de <c>identity.users</c> con todas las columnas que el sembrador escribe o Identity genera.</summary>
public sealed record SeededUserRow(
    Guid Id,
    string? UserName,
    string? NormalizedUserName,
    string? Email,
    string? NormalizedEmail,
    bool EmailConfirmed,
    string? PasswordHash,
    string? SecurityStamp,
    string? ConcurrencyStamp,
    bool LockoutEnabled,
    int AccessFailedCount,
    string DisplayName,
    string AccountStatus,
    Guid? CompanyId,
    DateTimeOffset CreatedAt);

/// <summary>
/// Foto de las tablas que toca el sembrador, leída con SQL directo (sin EF Core ni Identity) y en orden
/// estable, para comparar dos arranques fila por fila.
/// </summary>
public sealed record DatabaseSnapshot(
    IReadOnlyList<SeededUserRow> Users, IReadOnlyList<string> UserRoles, IReadOnlyList<string> Companies)
{
    private const string UsersSql =
        """
        SELECT id, user_name, normalized_user_name, email, normalized_email, email_confirmed, password_hash,
               security_stamp, concurrency_stamp, lockout_enabled, access_failed_count, display_name,
               account_status, company_id, created_at
        FROM identity.users
        ORDER BY id
        """;

    private const string UserRolesSql =
        """
        SELECT line
        FROM (
            SELECT ur.user_id::text || '|' || r.name AS line
            FROM identity.user_roles AS ur
            JOIN identity.roles AS r ON r.id = ur.role_id
        ) AS lines
        ORDER BY line COLLATE "C"
        """;

    private const string CompaniesSql =
        """
        SELECT line
        FROM (
            SELECT id::text || '|' || name || '|' || CASE WHEN is_active THEN 'True' ELSE 'False' END AS line
            FROM dataticket.companies
        ) AS lines
        ORDER BY line COLLATE "C"
        """;

    public static async Task<DatabaseSnapshot> ReadAsync(TestDatabase database, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        return new DatabaseSnapshot(
            await ReadUsersAsync(connection, cancellationToken),
            await ReadStringsAsync(connection, UserRolesSql, cancellationToken),
            await ReadStringsAsync(connection, CompaniesSql, cancellationToken));
    }

    private static async Task<List<SeededUserRow>> ReadUsersAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(UsersSql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<SeededUserRow>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new SeededUserRow(
                reader.GetGuid(0),
                Text(reader, 1),
                Text(reader, 2),
                Text(reader, 3),
                Text(reader, 4),
                reader.GetBoolean(5),
                Text(reader, 6),
                Text(reader, 7),
                Text(reader, 8),
                reader.GetBoolean(9),
                reader.GetInt32(10),
                reader.GetString(11),
                reader.GetString(12),
                reader.IsDBNull(13) ? null : reader.GetGuid(13),
                reader.GetFieldValue<DateTimeOffset>(14)));
        }

        return rows;
    }

    private static async Task<List<string>> ReadStringsAsync(NpgsqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var values = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }

    private static string? Text(NpgsqlDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
}
