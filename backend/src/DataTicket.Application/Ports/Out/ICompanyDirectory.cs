namespace DataTicket.Application.Ports.Out;

/// <summary>
/// Consulta de datos básicos de las empresas cliente.
/// </summary>
public interface ICompanyDirectory
{
    /// <summary>Nombre de la empresa, o <c>null</c> si no existe.</summary>
    Task<string?> FindNameAsync(Guid companyId, CancellationToken cancellationToken);
}
