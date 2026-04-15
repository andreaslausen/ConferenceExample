namespace ConferenceExample.ArchitectureTests;

public class ConferenceDependencyRules : ArchitectureTest
{
    [Fact]
    public void ConferenceDomain_ShouldOnlyDependOnItself()
    {
        Dependencies.Check(Architecture, "ConferenceExample.Conference.Domain", [], "System");
    }

    [Fact]
    public void ConferenceApplication_ShouldOnlyDependOnItselfAndConferenceDomainAndAuthentication()
    {
        Dependencies.Check(
            ConferenceApplication,
            [ConferenceDomain, Authentication],
            "System",
            "Microsoft.Extensions"
        );
    }

    [Fact]
    public void ConferencePersistence_ShouldOnlyDependOnItselfAndConferenceDomainAndEventStore()
    {
        Dependencies.Check(ConferencePersistence, [ConferenceDomain, EventStore], "System");
    }
}
