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

    private static readonly Type[] PostgreSqlFixtures = [typeof(ApiFixture), typeof(PostgreSqlContainerFixture)];

    private static bool UsesPostgreSql(Type type)
    {
        // Las propias fixtures no son clases de prueba.
        if (PostgreSqlFixtures.Contains(type))
        {
            return false;
        }

        var usesFixtureInterface = type.GetInterfaces().Any(contract =>
            contract.IsGenericType
            && (contract.GetGenericTypeDefinition() == typeof(IClassFixture<>)
                || contract.GetGenericTypeDefinition() == typeof(ICollectionFixture<>))
            && PostgreSqlFixtures.Contains(contract.GetGenericArguments()[0]));

        var receivesFixture = type.GetConstructors()
            .SelectMany(constructor => constructor.GetParameters())
            .Any(parameter => PostgreSqlFixtures.Contains(parameter.ParameterType));

        return usesFixtureInterface || receivesFixture;
    }

    private static bool HasRequiresDockerTrait(Type type) =>
        type.GetCustomAttributes<TraitAttribute>().Any(trait =>
            trait.Name == TestCategories.Name && trait.Value == TestCategories.RequiresDocker);
}
