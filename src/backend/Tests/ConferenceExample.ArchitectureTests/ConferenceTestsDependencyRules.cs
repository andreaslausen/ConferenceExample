namespace ConferenceExample.ArchitectureTests;

public class ConferenceTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void ConferenceDomainUnitTests_ShouldOnlyDependOn_ConferenceDomain()
    {
        Dependencies.Check(ConferenceDomainUnitTests, [ConferenceDomain], "System", "Xunit");
    }

    [Fact]
    public void ConferenceApplicationUnitTests_ShouldOnlyDependOn_ConferenceApplicationAndConferenceDomainAndAuthentication()
    {
        Dependencies.Check(
            ConferenceApplicationUnitTests,
            [ConferenceApplication, ConferenceDomain, Authentication],
            "System",
            "Xunit",
            "NSubstitute",
            "Microsoft.Extensions.DependencyInjection"
        );
    }

    [Fact]
    public void ConferencePersistenceUnitTests_ShouldOnlyDependOn_ConferencePersistenceAndConferenceDomainAndEventStore()
    {
        Dependencies.Check(
            ConferencePersistenceUnitTests,
            [ConferencePersistence, ConferenceDomain, EventStore],
            "System",
            "Xunit",
            "NSubstitute",
            "Microsoft.Extensions.DependencyInjection",
            "MongoDB"
        );
    }
}
