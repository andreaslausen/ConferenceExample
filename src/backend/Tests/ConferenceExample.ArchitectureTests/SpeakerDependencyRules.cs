namespace ConferenceExample.ArchitectureTests;

public class SpeakerDependencyRules : ArchitectureTest
{
    [Fact]
    public void SpeakerDomain_ShouldOnlyDependOnItself()
    {
        Dependencies.Check(Architecture, "ConferenceExample.Speaker.Domain", [], "System");
    }

    [Fact]
    public void SpeakerApplication_ShouldOnlyDependOnItselfAndSpeakerDomain()
    {
        Dependencies.Check(SpeakerApplication, [SpeakerDomain], "System", "Microsoft.Extensions");
    }

    [Fact]
    public void SpeakerPersistence_ShouldOnlyDependOnItselfAndSpeakerDomainAndEventStore()
    {
        Dependencies.Check(
            SpeakerPersistence,
            [SpeakerDomain, EventStore],
            "System",
            "Microsoft.Extensions.DependencyInjection",
            "MongoDB"
        );
    }
}
