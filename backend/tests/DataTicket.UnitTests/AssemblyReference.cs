using System.Reflection;

namespace DataTicket.UnitTests;

/// <summary>
/// Punto de anclaje del ensamblado de pruebas unitarias (usado por las pruebas de arquitectura).
/// </summary>
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
