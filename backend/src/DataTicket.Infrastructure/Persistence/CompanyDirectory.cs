using DataTicket.Application.Ports.Out;
using Microsoft.EntityFrameworkCore;

namespace DataTicket.Infrastructure.Persistence;

/// <summary>
/// Adaptador de <see cref="ICompanyDirectory"/> sobre EF Core (<c>dataticket.companies</c>).
/// </summary>
public sealed class CompanyDirectory(DataTicketDbContext dbContext) : ICompanyDirectory
{
    public Task<string?> FindNameAsync(Guid companyId, CancellationToken cancellationToken) =>
        dbContext.Companies
            .AsNoTracking()
            .Where(company => company.Id == companyId)
            .Select(company => (string?)company.Name)
            .FirstOrDefaultAsync(cancellationToken);
}
