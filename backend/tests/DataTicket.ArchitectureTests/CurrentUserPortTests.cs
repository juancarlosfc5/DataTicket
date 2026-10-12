using System.Reflection;
using DataTicket.Api.Auth;
using DataTicket.Application.Ports.Out;

namespace DataTicket.ArchitectureTests;

/// <summary>
/// HU-004 CHU-09: <see cref="ICurrentUser"/> es un puerto de salida de <c>Application</c> (sin tipos de
/// ASP.NET Core) y su único adaptador, el que lee el <c>HttpContext</c>, vive en <c>Api</c>.
/// </summary>
public sealed class CurrentUserPortTests
{
    private const string AspNetCorePrefix = "Microsoft.AspNetCore";

    private static readonly Assembly ApiAssembly = typeof(HttpContextCurrentUser).Assembly;

    private static readonly Assembly[] ProductionAssemblies =
    [
        Domain.AssemblyReference.Assembly,
        Application.AssemblyReference.Assembly,
        Infrastructure.AssemblyReference.Assembly,
        ApiAssembly,
    ];

    [Fact]
    public void CurrentUser_port_lives_in_Application_Ports_Out()
    {
        var port = typeof(ICurrentUser);

        Assert.True(port.IsInterface);
        Assert.Same(Application.AssemblyReference.Assembly, port.Assembly);
        Assert.Equal("DataTicket.Application.Ports.Out", port.Namespace);
    }

    [Fact]
    public void CurrentUser_port_does_not_expose_AspNetCore_types()
    {
        var exposed = AspNetCoreTypesExposedBy(typeof(ICurrentUser));

        Assert.True(exposed.Count == 0, $"ICurrentUser no debe exponer tipos de ASP.NET Core: {string.Join(", ", exposed)}");
    }

    /// <summary>
    /// Control de la regla anterior: un tipo de ASP.NET Core escondido en un argumento genérico, un
    /// <c>Nullable&lt;T&gt;</c>, un arreglo o un parámetro por referencia también cuenta como expuesto.
    /// </summary>
    [Fact]
    public void AspNetCore_types_wrapped_in_generics_nullables_arrays_or_references_are_detected()
    {
        var exposed = AspNetCoreTypesExposedBy(typeof(ILeakyPort));

        Assert.Equal(
            [
                "Microsoft.AspNetCore.Http.HostString",
                "Microsoft.AspNetCore.Http.HttpContext",
                "Microsoft.AspNetCore.Http.IFormFile",
                "Microsoft.AspNetCore.Http.PathString",
                "Microsoft.AspNetCore.Http.QueryString",
            ],
            exposed);
    }

    [Fact]
    public void CurrentUser_adapter_lives_only_in_Api()
    {
        var adapters = ProductionAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(ICurrentUser).IsAssignableFrom(type))
            .ToList();

        // Si no hubiera ningún adaptador, la regla pasaría sin mirar nada.
        Assert.NotEmpty(adapters);
        var misplaced = adapters
            .Where(type => type.Assembly != ApiAssembly || type.Namespace != "DataTicket.Api.Auth")
            .Select(type => type.FullName)
            .ToList();
        Assert.True(
            misplaced.Count == 0,
            $"El adaptador de ICurrentUser lee la petición HTTP: debe vivir en DataTicket.Api.Auth, no en: {string.Join(", ", misplaced)}");
    }

    /// <summary>Nombres, sin repetir y en orden, de los tipos de ASP.NET Core que aparecen en la firma del contrato.</summary>
    private static List<string> AspNetCoreTypesExposedBy(Type contract) =>
    [
        .. contract.GetMembers()
            .SelectMany(ExposedTypes)
            .Where(type => type.Namespace?.StartsWith(AspNetCorePrefix, StringComparison.Ordinal) == true
                || type.Assembly.GetName().Name?.StartsWith(AspNetCorePrefix, StringComparison.Ordinal) == true)
            .Select(type => type.FullName ?? type.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal),
    ];

    private static IEnumerable<Type> ExposedTypes(MemberInfo member) => member switch
    {
        PropertyInfo property => Unwrap(property.PropertyType),
        MethodInfo method => new[] { method.ReturnType }
            .Concat(method.GetParameters().Select(parameter => parameter.ParameterType))
            .SelectMany(Unwrap),
        _ => [],
    };

    /// <summary>
    /// El tipo y todo lo que envuelve: el elemento de un arreglo o de un parámetro por referencia y, de forma
    /// recursiva, los argumentos genéricos (<c>Task&lt;T&gt;</c>, <c>Nullable&lt;T&gt;</c>, colecciones, delegados).
    /// </summary>
    private static IEnumerable<Type> Unwrap(Type type)
    {
        if (type.HasElementType)
        {
            return Unwrap(type.GetElementType()!);
        }

        return type.IsGenericType ? [type, .. type.GetGenericArguments().SelectMany(Unwrap)] : [type];
    }

    /// <summary>Puerto de ejemplo que filtra tipos de ASP.NET Core solo a través de envoltorios.</summary>
    private interface ILeakyPort
    {
        Microsoft.AspNetCore.Http.PathString? WrappedInNullable { get; }

        IReadOnlyDictionary<string, Func<Microsoft.AspNetCore.Http.QueryString, int>> NestedInGenerics { get; }

        Task<IReadOnlyList<Microsoft.AspNetCore.Http.HttpContext>> WrappedInGenericsAsync();

        Microsoft.AspNetCore.Http.IFormFile[] WrappedInArray(Guid id);

        void PassedByReference(out Microsoft.AspNetCore.Http.HostString host);
    }
}
