namespace ConferenceExample.ArchitectureTests;

public class ConferenceDependencyRules : ArchitectureTest
{
    [Fact]
    public void ConferenceDomain_ShouldOnlyDependOnItself()
    {
        Dependencies.Check(Architecture, "ConferenceExample.Conference.Domain", [], "System");
    }

    [Fact]
    public void ConferenceApplication_ShouldOnlyDependOnItselfAndDomain()
    {
        Dependencies.Check(
            Architecture,
            "ConferenceExample.Conference.Application",
            [ConferenceDomain, Authentication],
            "System",
            "Microsoft.Extensions"
        );
    }

    [Fact]
    public void ConferencePersistence_ShouldOnlyDependOnItselfAndDomainAndEventStore()
    {
        Dependencies.Check(
            Architecture,
            "ConferenceExample.Conference.Persistence",
            [ConferenceDomain, EventStore],
            "System"
        );
    }
}
