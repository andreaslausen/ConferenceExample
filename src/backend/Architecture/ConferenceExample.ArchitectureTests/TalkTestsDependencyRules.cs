using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class TalkTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void TalkDomainUnitTests_ShouldOnlyDependOn_TalkDomain()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(TalkDomainUnitTests)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(TalkDomainUnitTests)
                    .Or()
                    .ResideInAssembly(TalkDomain)
                    .Or()
                    .ResideInNamespaceMatching("System.*")
                    .Or()
                    .ResideInNamespaceMatching("Xunit.*")
            );

        rule.Check(TestArchitecture);
    }

    [Fact]
    public void TalkApplicationUnitTests_ShouldOnlyDependOn_TalkApplicationAndDomain()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(TalkApplicationUnitTests)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(TalkApplicationUnitTests)
                    .Or()
                    .ResideInAssembly(TalkApplication)
                    .Or()
                    .ResideInAssembly(TalkDomain)
                    .Or()
                    .ResideInNamespaceMatching("System.*")
                    .Or()
                    .ResideInNamespaceMatching("Xunit.*")
                    .Or()
                    .ResideInNamespaceMatching("NSubstitute.*")
            );

        rule.Check(TestArchitecture);
    }

    [Fact]
    public void TalkPersistenceUnitTests_ShouldOnlyDependOn_TalkPersistenceAndDomainAndEventStore()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(TalkPersistenceUnitTests)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(TalkPersistenceUnitTests)
                    .Or()
                    .ResideInAssembly(TalkPersistence)
                    .Or()
                    .ResideInAssembly(TalkDomain)
                    .Or()
                    .ResideInAssembly(EventStore)
                    .Or()
                    .ResideInNamespaceMatching("System.*")
                    .Or()
                    .ResideInNamespaceMatching("Xunit.*")
                    .Or()
                    .ResideInNamespaceMatching("NSubstitute.*")
            );

        rule.Check(TestArchitecture);
    }

    [Fact]
    public void TalkAcceptanceTests_ShouldOnlyDependOn_TalkLayersAndEventStore()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(TalkAcceptanceTests)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(TalkAcceptanceTests)
                    .Or()
                    .ResideInAssembly(TalkApplication)
                    .Or()
                    .ResideInAssembly(TalkDomain)
                    .Or()
                    .ResideInAssembly(TalkPersistence)
                    .Or()
                    .ResideInAssembly(EventStore)
                    .Or()
                    .ResideInNamespaceMatching("System.*")
                    .Or()
                    .ResideInNamespaceMatching("Xunit.*")
                    .Or()
                    .ResideInNamespaceMatching("Reqnroll.*")
                    .Or()
                    .ResideInNamespaceMatching("Microsoft.*")
            );

        rule.Check(TestArchitecture);
    }
}
