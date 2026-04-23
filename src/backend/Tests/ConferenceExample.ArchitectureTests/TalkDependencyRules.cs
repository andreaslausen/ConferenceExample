namespace ConferenceExample.ArchitectureTests;

public class TalkDependencyRules : ArchitectureTest
{
    [Fact]
    public void TalkDomain_ShouldOnlyDependOnItself()
    {
        Dependencies.Check(Architecture, "ConferenceExample.Talk.Domain", [], "System");
    }

    [Fact]
    public void TalkApplication_ShouldOnlyDependOnItselfAndTalkDomain()
    {
        Dependencies.Check(
            TalkApplication,
            [TalkDomain],
            "System",
            "Microsoft.Extensions"
        );
    }

    [Fact]
    public void TalkPersistence_ShouldOnlyDependOnItselfAndTalkDomainAndEventStore()
    {
        Dependencies.Check(
            TalkPersistence,
            [TalkDomain, EventStore],
            "System",
            "Microsoft.Extensions.DependencyInjection",
            "MongoDB"
        );
    }
}
