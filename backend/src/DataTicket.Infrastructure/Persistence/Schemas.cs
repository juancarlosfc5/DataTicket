namespace DataTicket.Infrastructure.Persistence;

/// <summary>Esquemas de PostgreSQL (<c>db.sql</c>).</summary>
internal static class Schemas
{
    public const string Identity = "identity";
    public const string DataTicket = "dataticket";

    /// <summary>
    /// Esquema de la tabla de historial de migraciones (<c>__EFMigrationsHistory</c>). Se fija de forma
    /// explícita: sin esquema, EF Core la crea y la busca en el primer esquema existente del
    /// <c>search_path</c>, que cambia en cuanto existe un esquema con el nombre del usuario de PostgreSQL
    /// (<c>"$user", public</c>; en el Compose el usuario se llama <c>dataticket</c>). Entonces el segundo
    /// arranque no encontraba el historial y reaplicaba la migración inicial (42P07).
    /// Es <c>public</c> porque ahí quedó en las bases ya migradas: cambiarlo obligaría a moverla a mano.
    /// </summary>
    public const string MigrationsHistory = "public";
}
