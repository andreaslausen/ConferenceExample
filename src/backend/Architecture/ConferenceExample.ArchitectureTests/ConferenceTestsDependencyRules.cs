namespace ConferenceExample.ArchitectureTests;

public class ConferenceTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void ConferenceDomainUnitTests_ShouldOnlyDependOn_ConferenceDomain()
    {
        Dependencies.Check(ConferenceDomainUnitTests, [ConferenceDomain], "System", "Xunit");
    }

    [Fact]
    public void ConferenceApplicationUnitTests_ShouldOnlyDependOn_ConferenceApplicationAndDomain()
    {
        Dependencies.Check(
            ConferenceApplicationUnitTests,
            [ConferenceApplication, ConferenceDomain],
            "System",
            "Xunit",
            "NSubstitute"
        );
    }

    [Fact]
    public void ConferencePersistenceUnitTests_ShouldOnlyDependOn_ConferencePersistenceAndDomainAndEventStore()
    {
        Dependencies.Check(
            ConferencePersistenceUnitTests,
            [ConferencePersistence, ConferenceDomain, EventStore],
            "System",
            "Xunit",
            "NSubstitute"
        );
    }
}
