using System.Reflection;
using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Auditing;

namespace DataTicket.ArchitectureTests;

/// <summary>
/// HU-011 CHU-04: la bitácora es append-only desde la aplicación. El puerto <see cref="IAuditLog"/> y sus
/// adaptadores solo exponen la inserción; ni actualizan ni borran.
/// </summary>
public sealed class AuditLogPortTests
{
    private static readonly string[] ForbiddenPrefixes = ["Update", "Delete", "Remove", "Clear", "Set", "Modify", "Truncate"];

    private static readonly Assembly[] ProductionAssemblies =
    [
        Domain.AssemblyReference.Assembly,
        Application.AssemblyReference.Assembly,
        Infrastructure.AssemblyReference.Assembly,
        typeof(Api.Auth.HttpContextCurrentUser).Assembly,
    ];

    [Fact]
    public void IAuditLog_ExposesOnlyAppend()
    {
        var port = typeof(IAuditLog);

        Assert.True(port.IsInterface);
        Assert.Equal("DataTicket.Application.Ports.Out", port.Namespace);
        Assert.Empty(port.GetInterfaces());
        Assert.Empty(port.GetProperties());
        Assert.Empty(port.GetEvents());

        var method = Assert.Single(port.GetMethods());
        Assert.Equal("Append", method.Name);
        // Añade a la unidad de trabajo en curso: sin Task (no confirma) y con la entrada como único argumento.
        Assert.Equal(typeof(void), method.ReturnType);
        Assert.Equal([typeof(AuditEntry)], method.GetParameters().Select(parameter => parameter.ParameterType));
    }

    [Fact]
    public void AuditLog_adapters_expose_no_update_or_delete_operation()
    {
        var adapters = ProductionAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IAuditLog).IsAssignableFrom(type))
            .ToList();

        // Si no hubiera ningún adaptador, la regla pasaría sin mirar nada.
        Assert.NotEmpty(adapters);
        var offending = adapters
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName)
                .Where(method => ForbiddenPrefixes.Any(prefix => method.Name.StartsWith(prefix, StringComparison.Ordinal)))
                .Select(method => $"{type.FullName}.{method.Name}"))
            .ToList();

        Assert.True(offending.Count == 0, $"Los adaptadores de IAuditLog solo insertan: {string.Join(", ", offending)}");
        Assert.All(adapters, adapter => Assert.Equal("DataTicket.Infrastructure", adapter.Assembly.GetName().Name));
    }
}
