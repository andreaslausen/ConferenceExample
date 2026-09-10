namespace ConferenceExample.ArchitectureTests;

public class ApiDependencyRules : ArchitectureTest
{
    [Fact]
    public void ApiControllers_ShouldOnlyDependOn_ApplicationLayerAndAuthentication()
    {
        Dependencies.Check(
            Architecture,
            "ConferenceExample.API.Controllers",
            [ConferenceApplication, SpeakerApplication, TalkApplication, Authentication],
            "System",
            "Microsoft"
        );
    }
}
