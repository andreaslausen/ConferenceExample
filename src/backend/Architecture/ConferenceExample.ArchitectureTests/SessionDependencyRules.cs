using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class SessionDependencyRules : ArchitectureTest
{
    [Fact]
    public void SessionDomain_ShouldOnlyDependOnItself()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(SessionDomain)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(SessionDomain)
                    .Or()
                    .ResideInNamespaceMatching("System*")
            );

        rule.Check(Architecture);
    }

    [Fact]
    public void SessionApplication_ShouldOnlyDependOnItselfAndDomain()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(SessionApplication)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(SessionApplication)
                    .Or()
                    .ResideInAssembly(SessionDomain)
                    .Or()
                    .ResideInNamespaceMatching("System*")
            );

        rule.Check(Architecture);
    }

    [Fact]
    public void SessionPersistence_ShouldOnlyDependOnItselfAndDomainAndEventStore()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(SessionPersistence)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(SessionPersistence)
                    .Or()
                    .ResideInAssembly(SessionDomain)
                    .Or()
                    .ResideInAssembly(EventStore)
                    .Or()
                    .ResideInAssemblyMatching("System*")
            );

        rule.Check(Architecture);
    }
}
