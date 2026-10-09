using System.Reflection;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Toda clase que arranca PostgreSQL debe llevar el rasgo <c>RequiresDocker</c>; si no, la etapa
/// <c>test</c> del Dockerfile (sin Docker) la ejecutaría y fallaría. No necesita Docker.
/// </summary>
public sealed class TestCategoryConventionTests
{
    [Fact]
    public void Clases_que_usan_PostgreSQL_llevan_el_rasgo_RequiresDocker()
    {
        var offenders = typeof(TestCategoryConventionTests).Assembly.GetTypes()
            .Where(UsesPostgreSql)
            .Where(type => !HasRequiresDockerTrait(type))
            .Select(type => type.FullName)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"Añade [Trait(TestCategories.Name, TestCategories.RequiresDocker)] a: {string.Join(", ", offenders)}");
    }

    private static bool UsesPostgreSql(Type type)
    {
        var usesFixtureInterface = type.GetInterfaces().Any(contract =>
            contract.IsGenericType
            && contract.GetGenericTypeDefinition() == typeof(IClassFixture<>)
            && contract.GetGenericArguments()[0] == typeof(ApiFixture));

        var usesContainerDirectly = type != typeof(ApiFixture) && type.GetConstructors()
            .SelectMany(constructor => constructor.GetParameters())
            .Any(parameter => parameter.ParameterType == typeof(PostgreSqlContainerFixture));

        return usesFixtureInterface || usesContainerDirectly;
    }

    private static bool HasRequiresDockerTrait(Type type) =>
        type.GetCustomAttributes<TraitAttribute>().Any(trait =>
            trait.Name == TestCategories.Name && trait.Value == TestCategories.RequiresDocker);
}
