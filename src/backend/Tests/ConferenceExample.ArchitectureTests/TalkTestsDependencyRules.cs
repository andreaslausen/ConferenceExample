namespace ConferenceExample.ArchitectureTests;

public class TalkTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void TalkDomainUnitTests_ShouldOnlyDependOn_TalkDomain()
    {
        Dependencies.Check(TalkDomainUnitTests, [TalkDomain], "System", "Xunit");
    }
}
