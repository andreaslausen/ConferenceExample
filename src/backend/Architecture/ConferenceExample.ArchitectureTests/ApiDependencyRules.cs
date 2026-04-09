using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class ApiDependencyRules : ArchitectureTest
{
    [Fact]
    public void ApiControllers_ShouldOnlyDependOn_ApplicationLayer()
    {
        var rule = Types()
            .That()
            .ResideInNamespaceMatching("ConferenceExample.API.Controllers")
            .Should()
            .OnlyDependOn(
                Types()
                    .That()
                    .ResideInNamespaceMatching("ConferenceExample.API.Controllers")
                    .Or()
                    .ResideInAssembly(ConferenceApplication)
                    .Or()
                    .ResideInAssembly(TalkApplication)
                    .Or()
                    .ResideInNamespaceMatching("System.*")
                    .Or()
                    .ResideInNamespaceMatching("Microsoft.*")
            );

        rule.Check(Architecture);
    }
}
