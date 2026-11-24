namespace ConferenceExample.ArchitectureTests;

public class SpeakerTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void SpeakerDomainUnitTests_ShouldOnlyDependOn_SpeakerDomain()
    {
        Dependencies.Check(SpeakerDomainUnitTests, [SpeakerDomain], "System", "Xunit");
    }
}
