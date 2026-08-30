namespace ConferenceExample.ArchitectureTests;

public class ConferenceTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void ConferenceDomainUnitTests_ShouldOnlyDependOn_ConferenceDomain()
    {
        Dependencies.Check(ConferenceDomainUnitTests, [ConferenceDomain], "System", "Xunit");
    }
}
