namespace ConferenceExample.ArchitectureTests;

public class EventStoreDependencyRules : ArchitectureTest
{
    [Fact]
    public void EventStore_ShouldNotDependOn_AnyBoundedContext()
    {
        Dependencies.Check(Architecture, "ConferenceExample.EventStore", [], "System", "MongoDB");
    }
}
