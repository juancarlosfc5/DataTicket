namespace DataTicket.Api.Auth;

/// <summary>
/// Metadato marcador: el endpoint pasa por <see cref="AntiforgeryValidationFilter"/>. Solo lo añade
/// <see cref="AntiforgeryValidationExtensions.RequireAntiforgeryValidation"/>, junto con el filtro, para que
/// una prueba de convención detecte cualquier mutación mapeada fuera del grupo raíz, sea cual sea su ruta.
/// </summary>
public sealed class AntiforgeryValidationMetadata
{
    public static AntiforgeryValidationMetadata Instance { get; } = new();

    private AntiforgeryValidationMetadata()
    {
    }
}

public static class AntiforgeryValidationExtensions
{
    /// <summary>
    /// Aplica el filtro antiforgery y su marcador a todos los endpoints del grupo (presentes y futuros).
    /// </summary>
    public static RouteGroupBuilder RequireAntiforgeryValidation(this RouteGroupBuilder group) =>
        group.AddEndpointFilter<AntiforgeryValidationFilter>().WithMetadata(AntiforgeryValidationMetadata.Instance);
}
