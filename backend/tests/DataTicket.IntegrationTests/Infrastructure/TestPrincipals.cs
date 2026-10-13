using System.Security.Claims;
using DataTicket.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Principales construidos a mano, con la misma forma que la cookie de sesión (contrato T-01 de HU-004):
/// id en <c>NameIdentifier</c>, un <c>Role</c> por rol o equipo y <c>company_id</c> solo si se indica.
/// </summary>
public static class TestPrincipals
{
    public static readonly Guid UserId = Guid.Parse("b0000000-0000-7000-8000-000001000001");

    public static ClaimsPrincipal Anonymous { get; } = new(new ClaimsIdentity());

    public static ClaimsPrincipal Create(IEnumerable<string> roles, Guid? companyId = null, params Claim[] extraClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, UserId.ToString()),
            new(ClaimTypes.Email, "usuario@empresa-sintetica-01.test"),
            new(DataTicketClaimTypes.DisplayName, "Usuario Sintético"),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        if (companyId is { } id)
        {
            claims.Add(new Claim(DataTicketClaimTypes.CompanyId, id.ToString()));
        }

        claims.AddRange(extraClaims);
        return new ClaimsPrincipal(new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme));
    }
}
