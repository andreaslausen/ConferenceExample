using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class ConferenceTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void ConferenceUnitTests_ShouldNotDependOn_AnyProductionCode()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(ConferenceUnitTests)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(ConferenceUnitTests)
                    .Or()
                    .ResideInNamespaceMatching("System.*")
                    .Or()
                    .ResideInNamespaceMatching("Xunit.*")
            );

        rule.Check(TestArchitecture);
    }

    [Fact]
    public void ConferenceDomainUnitTests_ShouldOnlyDependOn_ConferenceDomain()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(ConferenceDomainUnitTests)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(ConferenceDomainUnitTests)
                    .Or()
                    .ResideInAssembly(ConferenceDomain)
                    .Or()
                    .ResideInNamespaceMatching("System.*")
                    .Or()
                    .ResideInNamespaceMatching("Xunit.*")
            );

        rule.Check(TestArchitecture);
    }

    [Fact]
    public void ConferenceApplicationUnitTests_ShouldOnlyDependOn_ConferenceApplicationAndDomain()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(ConferenceApplicationUnitTests)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(ConferenceApplicationUnitTests)
                    .Or()
                    .ResideInAssembly(ConferenceApplication)
                    .Or()
                    .ResideInAssembly(ConferenceDomain)
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
    public void ConferencePersistenceUnitTests_ShouldOnlyDependOn_ConferencePersistenceAndDomainAndEventStore()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(ConferencePersistenceUnitTests)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(ConferencePersistenceUnitTests)
                    .Or()
                    .ResideInAssembly(ConferencePersistence)
                    .Or()
                    .ResideInAssembly(ConferenceDomain)
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
}
