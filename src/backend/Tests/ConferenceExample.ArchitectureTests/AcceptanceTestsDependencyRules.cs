namespace ConferenceExample.ArchitectureTests;

public class AcceptanceTestsDependencyRules : ArchitectureTest
{
    [Fact]
    public void AcceptanceTests_ShouldOnlyDependOn_AllAssemblies()
    {
        // The acceptance suite drives the real REST API over HTTP (WebApplicationFactory +
        // Testcontainers) and covers the whole system rather than one bounded context, so it
        // legitimately needs DTOs/enums from every layer it exercises.
        Dependencies.Check(
            AcceptanceTests,
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
