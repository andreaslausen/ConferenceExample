namespace ConferenceExample.ArchitectureTests;

public class TalkTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void TalkDomainUnitTests_ShouldOnlyDependOn_TalkDomain()
    {
        Dependencies.Check(TalkDomainUnitTests, [TalkDomain], "System", "Xunit");
    }

    [Fact]
    public void TalkApplicationUnitTests_ShouldOnlyDependOn_TalkApplicationAndDomain()
    {
        Dependencies.Check(
            TalkApplicationUnitTests,
            [TalkApplication, TalkDomain],
            "System",
            "Xunit",
            "NSubstitute"
        );
    }

    [Fact]
    public void TalkPersistenceUnitTests_ShouldOnlyDependOn_TalkPersistenceAndDomainAndEventStore()
    {
        Dependencies.Check(
            TalkPersistenceUnitTests,
            [TalkPersistence, TalkDomain, EventStore],
            "System",
            "Xunit",
            "NSubstitute"
        );
    }

    [Fact]
    public void TalkAcceptanceTests_ShouldOnlyDependOn_TalkLayersAndEventStore()
    {
        Dependencies.Check(
            TalkAcceptanceTests,
            [TalkApplication, TalkDomain, TalkPersistence, EventStore, Authentication],
            "System",
            "Xunit",
            "Reqnroll",
            "Microsoft",
            "NSubstitute"
        );
    }
}
