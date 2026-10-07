using System.Reflection;

namespace DataTicket.Domain;

/// <summary>
/// Punto de anclaje del ensamblado de dominio (usado por las pruebas de arquitectura).
/// </summary>
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
