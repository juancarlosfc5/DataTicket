using System.Reflection;

namespace DataTicket.Infrastructure;

/// <summary>
/// Punto de anclaje del ensamblado de infraestructura (usado por las pruebas de arquitectura).
/// </summary>
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
