using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class EventStoreTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void EventStoreUnitTests_ShouldOnlyDependOn_EventStore()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(EventStoreUnitTests)
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInAssembly(EventStoreUnitTests)
                    .Or()
                    .ResideInAssembly(EventStore)
                    .Or()
                    .ResideInNamespaceMatching("System.*")
                    .Or()
                    .ResideInNamespaceMatching("Xunit.*")
            );

        rule.Check(TestArchitecture);
    }
}
