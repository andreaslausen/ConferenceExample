namespace ConferenceExample.ArchitectureTests;

public class TalkDependencyRules : ArchitectureTest
{
    [Fact]
    public void TalkDomain_ShouldOnlyDependOnItself()
    {
        Dependencies.Check(Architecture, "ConferenceExample.Talk.Domain", [], "System");
    }

    [Fact]
    public void TalkApplication_ShouldOnlyDependOnItselfAndDomain()
    {
        Dependencies.Check(
            Architecture,
            "ConferenceExample.Talk.Application",
            [TalkDomain, Authentication],
            "System",
            "Microsoft.Extensions"
        );
    }

    [Fact]
    public void TalkPersistence_ShouldOnlyDependOnItselfAndDomainAndEventStore()
    {
        Dependencies.Check(
            Architecture,
            "ConferenceExample.Talk.Persistence",
            [TalkDomain, EventStore],
            "System"
        );
    }
}
