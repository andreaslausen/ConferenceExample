using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class DomainRules : ArchitectureTest
{
    [Fact]
    public void Repositories_AreNotUsedInDomain()
    {
        var repositoryInterfaces = Interfaces()
            .That()
            .ResideInAssembly(ConferenceDomain,
                SessionDomain)
            .And()
            .HaveNameEndingWith("Repository");

        var rule = Classes()
            .That()
            .ResideInAssembly(ConferenceDomain,
                SessionDomain)
            .Should()
            .NotCallAny(
                MethodMembers()
                    .That()
                    .ArePublic()
                    .And()
                    .AreDeclaredIn(repositoryInterfaces)
            )
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }
}