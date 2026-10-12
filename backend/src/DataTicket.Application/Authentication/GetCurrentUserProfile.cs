using DataTicket.Application.Ports.In.Authentication;
using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Users;

namespace DataTicket.Application.Authentication;

/// <summary>
/// Compone el perfil del usuario actual desde <see cref="ICurrentUser"/> y el nombre de su empresa (HU-004).
/// No recibe nada de la petición. Falla cerrado: sin sesión, sin superficie válida, con un perfil
/// incoherente o si la empresa del cliente no existe, no hay perfil.
/// </summary>
public sealed class GetCurrentUserProfile(ICurrentUser currentUser, ICompanyDirectory companies) : IGetCurrentUserProfile
{
    public async Task<CurrentUserProfile?> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated
            || currentUser.UserId is not { } userId
            || currentUser.Surface is not { } surface
            || string.IsNullOrWhiteSpace(currentUser.Email))
        {
            return null;
        }

        CurrentUserCompany? company = null;
        if (surface == Surface.Client)
        {
            company = await FindCompanyAsync(currentUser.CompanyId, cancellationToken);
            if (company is null)
            {
                return null;
            }
        }
        else if (currentUser.CompanyId is not null)
        {
            // Un interno con empresa es un perfil incoherente.
            return null;
        }

        var displayName = string.IsNullOrWhiteSpace(currentUser.DisplayName) ? currentUser.Email : currentUser.DisplayName;
        return new CurrentUserProfile(
            userId, displayName, currentUser.Email, surface, currentUser.Roles, currentUser.Teams, company);
    }

    private async Task<CurrentUserCompany?> FindCompanyAsync(Guid? companyId, CancellationToken cancellationToken)
    {
        if (companyId is not { } id)
        {
            return null;
        }

        var name = await companies.FindNameAsync(id, cancellationToken);
        return name is null ? null : new CurrentUserCompany(id, name);
    }
}
