using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Domain.SharedKernel.Extensions;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class ClassRules : ArchitectureTest
{
    [Fact]
    public void Classes_ShouldNotCallGuidMethods()
    {
        var architecture = new ArchLoader()
            .LoadAssemblies(AllAssemblies)
            .LoadAssembly(typeof(Guid).Assembly)
            .Build();

        var rule = Classes()
            .That()
            .ResideInNamespaceMatching("ConferenceExample*")
            .And()
            .AreNot(typeof(GuidV7))
            .And()
            .AreNot(typeof(Conference.Domain.SharedKernel.ValueObjects.Ids.GuidV7))
            .And()
            .AreNot(typeof(Authentication.SharedKernel.ValueObjects.Ids.GuidV7))
            .And()
            .AreNot(typeof(GuidExtensions))
            .And()
            .AreNot(typeof(StringExtensions))
            .And()
            .AreNot(typeof(Conference.Domain.SharedKernel.Extensions.GuidExtensions))
            .And()
            .AreNot(typeof(Conference.Domain.SharedKernel.Extensions.StringExtensions))
            .And()
            .AreNot(typeof(Authentication.SharedKernel.Extensions.GuidExtensions))
            .And()
            .AreNot(typeof(Authentication.SharedKernel.Extensions.StringExtensions))
            .And()
            .AreNot(typeof(InMemoryEventStore))
            .Should()
            .NotCallAny(MethodMembers().That().AreDeclaredIn(typeof(Guid)))
            .WithoutRequiringPositiveResults();

        rule.Check(architecture);
    }
}
