namespace ConferenceExample.ArchitectureTests;

public class ApiDependencyRules : ArchitectureTest
{
    [Fact]
    public void ApiControllers_ShouldOnlyDependOn_ApplicationLayerAndAuthentication()
    {
        Dependencies.Check(
            Architecture,
            "ConferenceExample.API.Controllers",
            [ConferenceApplication, TalkApplication, Authentication],
            "System",
            "Microsoft"
        );
    }
}
