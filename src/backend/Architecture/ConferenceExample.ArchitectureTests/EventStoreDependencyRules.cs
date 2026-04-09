using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class EventStoreDependencyRules : ArchitectureTest
{
    [Fact]
    public void EventStore_ShouldNotDependOn_AnyBoundedContext()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(EventStore)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(EventStore)
                    .Or()
                    .ResideInNamespaceMatching("System.*")
            );

        rule.Check(Architecture);
    }
}
