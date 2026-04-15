namespace ConferenceExample.ArchitectureTests;

public class EventStoreDependencyRules : ArchitectureTest
{
    [Fact]
    public void EventStore_ShouldOnlyDependOn_Itself()
    {
        Dependencies.Check(EventStore, [], "System", "MongoDB", "Microsoft.Extensions");
    }
}
