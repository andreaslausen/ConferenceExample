namespace ConferenceExample.ArchitectureTests;

public class ApiDependencyRules : ArchitectureTest
{
    [Fact]
    public void ApiControllers_ShouldOnlyDependOn_ApplicationLayer()
    {
        Dependencies.Check(
            Architecture,
            "ConferenceExample.API.Controllers",
            [ConferenceApplication, TalkApplication],
            "System",
            "Microsoft"
        );
    }
}
