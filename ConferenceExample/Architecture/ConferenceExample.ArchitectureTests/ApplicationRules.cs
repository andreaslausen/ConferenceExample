using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class ApplicationRules : ArchitectureTest
{
    [Fact]
    public void ApplicationServices_ShouldNotCallOtherApplicationServices()
    {
        var applicationServices = Types()
            .That()
            .ResideInAssembly(ConferenceApplication,
                SessionApplication)
            .And()
            .HaveNameEndingWith("Service");

        var rule = MethodMembers()
            .That()
            .AreDeclaredIn(applicationServices)
            .Should()
            .NotCallAny(MethodMembers()
                .That()
                .AreDeclaredIn(applicationServices)
                .And()
                .ArePublic())
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }
}