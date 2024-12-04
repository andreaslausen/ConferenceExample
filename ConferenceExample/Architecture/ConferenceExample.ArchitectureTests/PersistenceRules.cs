using ArchUnitNET.Domain.Dependencies;
using ArchUnitNET.Domain.Extensions;

namespace ConferenceExample.ArchitectureTests;

public class PersistenceRules : ArchitectureTest
{
    [Fact]
    public void Repositories_ShouldNotUseOtherRepositories()
    {
        var services =
            Architecture.Classes.Where(s => s.Namespace.NameContains("Persistence") && s.NameEndsWith("Repository"));

        Assert.All(services, c => Assert.True(
            c.Dependencies.Find(d =>
                d is not ImplementsInterfaceDependency &&
                d.Origin.FullName != d.Target.FullName &&
                d.Target.NameEndsWith("Repository"))
            == null));
    }
}