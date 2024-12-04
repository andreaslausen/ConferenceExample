using ArchUnitNET.Domain.Extensions;

namespace ConferenceExample.ArchitectureTests;

public class DomainRules : ArchitectureTest
{
    [Fact]
    public void Repositories_AreNotUsedInDomain()
    {
        var classes = Architecture.Classes.Where(s => s.Namespace.NameContains("Domain"));
        Assert.All(classes,
            c => Assert.Null(c.Dependencies.Find(d =>
                d.Origin.FullName != d.Target.FullName &&
                d.Target.NameEndsWith("Repository")
            )));
    }
}