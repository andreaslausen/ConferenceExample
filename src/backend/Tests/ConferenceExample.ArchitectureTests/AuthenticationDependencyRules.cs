namespace ConferenceExample.ArchitectureTests;

public class AuthenticationDependencyRules : ArchitectureTest
{
    [Fact]
    public void Authentication_ShouldOnlyDependOn_ApplicationLayers()
    {
        // Authentication implements each bounded context's ICurrentUserService, so it touches
        // every Application layer and nothing below it.
        Dependencies.Check(
            Authentication,
            [ConferenceApplication, SpeakerApplication, TalkApplication],
            "System",
            "MongoDB",
            "Microsoft.Extensions",
            "Microsoft.AspNetCore",
            "Microsoft.IdentityModel"
        );
    }
}
