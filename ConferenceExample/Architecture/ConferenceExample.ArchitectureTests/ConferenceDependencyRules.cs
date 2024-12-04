using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class ConferenceDependencyRules : ArchitectureTest
{
    [Fact]
    public void ConferenceDomain_ShouldOnlyDependOnItself()
    {
        var rule = Types().That().ResideInAssembly(ConferenceDomain)
            .Should()
            .OnlyDependOn(Types().That().ResideInAssembly(ConferenceDomain).Or().ResideInNamespace("System.*", true));

        rule.Check(Architecture);
    }

    [Fact]
    public void ConferenceApplication_ShouldOnlyDependOnItselfAndDomain()
    {
        var rule = Types().That().ResideInAssembly(ConferenceApplication)
            .Should()
            .OnlyDependOn(Types().That().ResideInAssembly(ConferenceApplication).Or().ResideInAssembly(ConferenceDomain).Or().ResideInNamespace("System.*", true));

        rule.Check(Architecture);
    }

    [Fact]
    public void ConferencePersistence_ShouldOnlyDependOnItselfAndDomainAndPersistence()
    {
        var rule = Types().That().ResideInAssembly(ConferencePersistence)
            .Should()
            .OnlyDependOn(Types().That().ResideInAssembly(ConferencePersistence).Or().ResideInAssembly(Persistence).Or().ResideInAssembly(ConferenceDomain).Or()
                .ResideInNamespace("System.*", true));

        rule.Check(Architecture);
    }
}