namespace DataTicket.Domain.Users;

/// <summary>
/// Reglas de acceso de una cuenta (contrato T-01 de HU-003/HU-004, «Perfiles válidos»).
/// </summary>
public static class AccountAccess
{
    /// <summary>
    /// Superficie del perfil, o <c>null</c> si el perfil no es válido:
    /// <list type="bullet">
    /// <item>Cliente: algún rol de cliente, ningún rol interno y con empresa.</item>
    /// <item>Interno: algún rol interno, ningún rol de cliente y sin empresa.</item>
    /// </list>
    /// Un perfil mixto, sin roles o incoherente con la empresa falla cerrado.
    /// </summary>
    public static Surface? ResolveSurface(IReadOnlyCollection<string> roles, Guid? companyId)
    {
        ArgumentNullException.ThrowIfNull(roles);

        var hasClientRole = roles.Any(RoleNames.Client.Contains);
        var hasInternalRole = roles.Any(RoleNames.Internal.Contains);

        return (hasClientRole, hasInternalRole, companyId) switch
        {
            (true, false, { } id) when id != Guid.Empty => Surface.Client,
            (false, true, null) => Surface.Internal,
            _ => null,
        };
    }

    /// <summary>
    /// Una cuenta puede tener sesión solo si está <see cref="AccountStatus.Active"/> y su perfil es válido.
    /// Se evalúa al iniciar sesión y en cada revalidación del sello de seguridad.
    /// </summary>
    public static bool CanSignIn(AccountStatus status, IReadOnlyCollection<string> roles, Guid? companyId) =>
        status == AccountStatus.Active && ResolveSurface(roles, companyId) is not null;
}
