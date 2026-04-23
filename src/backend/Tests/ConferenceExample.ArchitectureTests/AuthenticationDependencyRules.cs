namespace ConferenceExample.ArchitectureTests;

public class AuthenticationDependencyRules : ArchitectureTest
{
    [Fact]
    public void Authentication_ShouldOnlyDependOn_ConferenceAndTalkApplication()
    {
        Dependencies.Check(
            Authentication,
            [ConferenceApplication, TalkApplication],
            "System",
            "MongoDB",
            "Microsoft.Extensions",
            "Microsoft.AspNetCore",
            "Microsoft.IdentityModel"
        );
    }
}
