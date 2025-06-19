using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using ConferenceExample.Session.Domain.Extensions;
using ConferenceExample.Session.Domain.ValueObjects;
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
            .ResideInNamespace("ConferenceExample*",
                true)
            .And()
            .AreNot(typeof(GuidV7))
            .And()
            .AreNot(typeof(Conference.Domain.ValueObjects.Ids.GuidV7))
            .And()
            .AreNot(typeof(GuidExtensions))
            .And()
            .AreNot(typeof(StringExtensions))
            .And()
            .AreNot(typeof(Conference.Domain.Extensions.GuidExtensions))
            .And()
            .AreNot(typeof(Conference.Domain.Extensions.StringExtensions))
            .Should()
            .NotCallAny(MethodMembers()
                .That()
                .AreDeclaredIn(typeof(Guid)))
            .WithoutRequiringPositiveResults();

        rule.Check(architecture);
    }
}