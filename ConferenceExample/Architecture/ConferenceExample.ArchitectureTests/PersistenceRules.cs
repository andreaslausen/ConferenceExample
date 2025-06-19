using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class PersistenceRules : ArchitectureTest
{
    [Fact]
    public void Repositories_ShouldNotUseOtherRepositories()
    {
        var repositories = Types()
            .That()
            .ResideInAssembly(SessionDomain,
                SessionPersistence,
                ConferenceDomain,
                ConferencePersistence)
            .And()
            .HaveNameEndingWith("Repository");

        var rule = repositories.Should()
            .NotCallAny(MethodMembers()
                .That()
                .ArePublic()
                .And()
                .AreDeclaredIn(repositories))
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }
}