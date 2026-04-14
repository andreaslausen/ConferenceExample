namespace ConferenceExample.ArchitectureTests;

public class EventStoreTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void EventStoreUnitTests_ShouldOnlyDependOn_EventStore()
    {
        Dependencies.Check(
            EventStoreUnitTests,
            [EventStore],
            "System",
            "Xunit",
            "NSubstitute",
            "MongoDB"
        );
    }
}
