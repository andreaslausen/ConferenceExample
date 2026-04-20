using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class HandlerRules : ArchitectureTest
{
    [Fact]
    public void Handlers_ShouldNotCallMethodsOfOtherHandlers()
    {
        var handlers = Types()
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .ResideInAssembly(
                ConferenceApplication,
                TalkApplication,
                ConferencePersistence,
                TalkPersistence
            );

        MethodMembers()
            .That()
            .AreDeclaredIn(handlers)
            .Should()
            .NotCallAny(MethodMembers().That().ArePublic().And().AreDeclaredIn(handlers))
            .Because(
                "Handlers are independent units of behavior. "
                    + "Orchestration belongs in services, not handlers."
            )
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }
}
