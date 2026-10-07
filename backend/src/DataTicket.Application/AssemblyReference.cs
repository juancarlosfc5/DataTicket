using System.Reflection;

namespace DataTicket.Application;

/// <summary>
/// Punto de anclaje del ensamblado de aplicación (usado por las pruebas de arquitectura).
/// </summary>
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
