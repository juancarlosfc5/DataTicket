using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Users;

namespace DataTicket.Api.Auth;

/// <summary>
/// Adaptador de <see cref="ICurrentUser"/>: lee los claims del <c>ClaimsPrincipal</c> de la petición en
/// curso (<see cref="PrincipalProfile"/>). Es el único punto por el que los casos de uso conocen al usuario;
/// la empresa sale del claim <c>company_id</c> de la sesión y de ningún otro sitio.
/// </summary>
public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAuthenticated => Profile.IsAuthenticated;

    public Guid? UserId => Profile.UserId;

    public string? DisplayName => Profile.DisplayName;

    public string? Email => Profile.Email;

    public Surface? Surface => Profile.Surface;

    public IReadOnlyList<string> Roles => Profile.Roles;

    public IReadOnlyList<string> Teams => Profile.Teams;

    public Guid? CompanyId => Profile.CompanyId;

    public bool IsInRole(string role) => Profile.IsInRole(role);

    // Se lee en cada acceso: el principal lo fija el middleware de autenticación, que puede ejecutarse
    // después de que alguien resuelva este servicio.
    private PrincipalProfile Profile => PrincipalProfile.From(httpContextAccessor.HttpContext?.User);
}
