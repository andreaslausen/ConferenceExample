namespace ConferenceExample.ArchitectureTests;

public class AuthenticationTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void AuthenticationUnitTests_ShouldOnlyDependOn_Authentication()
    {
        Dependencies.Check(
            AuthenticationUnitTests,
            [Authentication],
            "System",
            "Xunit",
            "NSubstitute"
        );
    }
}
