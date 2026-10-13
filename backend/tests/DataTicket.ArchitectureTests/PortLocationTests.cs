using System.Runtime.CompilerServices;

namespace DataTicket.ArchitectureTests;

/// <summary>
/// Los puertos del hexágono son las interfaces de <c>Application</c> y viven en <c>Ports/In</c> (casos de
/// uso que invocan los adaptadores primarios) o <c>Ports/Out</c> (lo que implementan los secundarios).
/// </summary>
public sealed class PortLocationTests
{
    private static readonly string[] PortNamespaces =
    [
        "DataTicket.Application.Ports.In",
        "DataTicket.Application.Ports.Out",
    ];

    [Fact]
    public void Application_DeclaresItsInterfacesInPortsInOrPortsOut()
    {
        var interfaces = Application.AssemblyReference.Assembly.GetTypes()
            .Where(type => type.IsInterface && !type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            .ToList();

        var misplaced = interfaces
            .Where(type => !IsPortNamespace(type.Namespace))
            .Select(type => type.FullName)
            .ToList();

        // Si no hubiera ninguna interfaz, la regla pasaría sin mirar nada.
        Assert.NotEmpty(interfaces);
        Assert.True(
            misplaced.Count == 0,
            $"Las interfaces de Application deben vivir en Ports.In o Ports.Out: {string.Join(", ", misplaced)}");
    }

    private static bool IsPortNamespace(string? typeNamespace) =>
        typeNamespace is not null
        && PortNamespaces.Any(port =>
            typeNamespace.Equals(port, StringComparison.Ordinal)
            || typeNamespace.StartsWith(port + ".", StringComparison.Ordinal));
}
