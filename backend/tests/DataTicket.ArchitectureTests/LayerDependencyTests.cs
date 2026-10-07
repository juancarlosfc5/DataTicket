using System.Reflection;

namespace DataTicket.ArchitectureTests;

/// <summary>
/// Regla de dependencias del hexágono: las flechas apuntan siempre hacia el dominio.
/// Api → Application/Infrastructure, Infrastructure → Application → Domain.
/// </summary>
public sealed class LayerDependencyTests
{
    private const string ApplicationAssembly = "DataTicket.Application";
    private const string InfrastructureAssembly = "DataTicket.Infrastructure";
    private const string ApiAssembly = "DataTicket.Api";

    private static readonly string[] FrameworkPrefixes =
    [
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
    ];

    [Fact]
    public void Domain_DoesNotDependOnOtherLayers()
    {
        AssertDoesNotReference(
            Domain.AssemblyReference.Assembly,
            [ApplicationAssembly, InfrastructureAssembly, ApiAssembly]);
    }

    [Fact]
    public void Domain_DoesNotDependOnFrameworks()
    {
        // El dominio tampoco usa DI, logging ni configuración: es C# puro.
        AssertDoesNotReferencePrefixes(
            Domain.AssemblyReference.Assembly,
            [.. FrameworkPrefixes, "Microsoft.Extensions"]);
    }

    [Fact]
    public void Application_DoesNotDependOnAdapters()
    {
        AssertDoesNotReference(
            Application.AssemblyReference.Assembly,
            [InfrastructureAssembly, ApiAssembly]);
    }

    [Fact]
    public void Application_DoesNotDependOnFrameworks()
    {
        AssertDoesNotReferencePrefixes(Application.AssemblyReference.Assembly, FrameworkPrefixes);
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApi()
    {
        AssertDoesNotReference(Infrastructure.AssemblyReference.Assembly, [ApiAssembly]);
    }

    private static void AssertDoesNotReference(Assembly assembly, IReadOnlyCollection<string> forbidden)
    {
        var violations = ReferencedNames(assembly).Where(forbidden.Contains).ToList();

        Assert.True(
            violations.Count == 0,
            $"{assembly.GetName().Name} no debe depender de: {string.Join(", ", violations)}");
    }

    private static void AssertDoesNotReferencePrefixes(Assembly assembly, IReadOnlyCollection<string> prefixes)
    {
        var violations = ReferencedNames(assembly)
            .Where(name => prefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        Assert.True(
            violations.Count == 0,
            $"{assembly.GetName().Name} no debe depender de frameworks de adaptadores: {string.Join(", ", violations)}");
    }

    private static IEnumerable<string> ReferencedNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? string.Empty);
}
