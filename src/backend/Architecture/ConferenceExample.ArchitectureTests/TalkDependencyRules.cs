using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class TalkDependencyRules : ArchitectureTest
{
    [Fact]
    public void TalkDomain_ShouldOnlyDependOnItself()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(TalkDomain)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(TalkDomain)
                    .Or()
                    .ResideInNamespaceMatching("System*")
            );

        rule.Check(Architecture);
    }

    [Fact]
    public void TalkApplication_ShouldOnlyDependOnItselfAndDomain()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(TalkApplication)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(TalkApplication)
                    .Or()
                    .ResideInAssembly(TalkDomain)
                    .Or()
                    .ResideInNamespaceMatching("System*")
            );

        rule.Check(Architecture);
    }

    [Fact]
    public void TalkPersistence_ShouldOnlyDependOnItselfAndDomainAndEventStore()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(TalkPersistence)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(TalkPersistence)
                    .Or()
                    .ResideInAssembly(TalkDomain)
                    .Or()
                    .ResideInAssembly(EventStore)
                    .Or()
                    .ResideInAssemblyMatching("System*")
            );

        rule.Check(Architecture);
    }
}
