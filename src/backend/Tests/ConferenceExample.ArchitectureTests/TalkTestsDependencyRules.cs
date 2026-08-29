namespace ConferenceExample.ArchitectureTests;

public class TalkTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void TalkDomainUnitTests_ShouldOnlyDependOn_TalkDomain()
    {
        Dependencies.Check(TalkDomainUnitTests, [TalkDomain], "System", "Xunit");
    }

    [Fact]
    public void TalkAcceptanceTests_ShouldOnlyDependOn_AllAssemblies()
    {
        // The acceptance suite drives the real REST API over HTTP (WebApplicationFactory +
        // Testcontainers), so it legitimately needs DTOs/enums from every layer it exercises,
        // not just Talk's own assemblies.
        Dependencies.Check(
            TalkAcceptanceTests,
            AllAssemblies,
            "System",
            "Xunit",
            "Reqnroll",
            "Microsoft",
            "MongoDB",
            "Testcontainers"
        );
    }
}
