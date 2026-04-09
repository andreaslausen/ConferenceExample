using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public static class Dependencies
{
    public static void Check(
        System.Reflection.Assembly source,
        System.Reflection.Assembly[] target,
        params string[] allowedNamespaces
    )
    {
        var allowedTypes = Types().That().ResideInAssembly(source);
        allowedTypes = target.Aggregate(
            allowedTypes,
            (current, assembly) => current.Or().ResideInAssembly(assembly)
        );
        allowedTypes = allowedNamespaces.Aggregate(
            allowedTypes,
            (current, allowedNamespace) =>
                current.Or().ResideInNamespaceMatching($"{allowedNamespace}.*")
        );

        var rule = Types().That().ResideInAssembly(source).Should().OnlyDependOn(allowedTypes);

        var architecture = new ArchLoader()
            .LoadAssemblyIncludingDependencies(source)
            .LoadAssemblies(target)
            .Build();

        rule.Check(architecture);
    }

    public static void Check(
        Architecture architecture,
        string sourceNamespace,
        System.Reflection.Assembly[] target,
        params string[] allowedNamespaces
    )
    {
        var allowedTypes = Types().That().ResideInNamespaceMatching($"{sourceNamespace}.*");
        allowedTypes = target.Aggregate(
            allowedTypes,
            (current, assembly) => current.Or().ResideInAssembly(assembly)
        );
        allowedTypes = allowedNamespaces.Aggregate(
            allowedTypes,
            (current, allowedNamespace) =>
                current.Or().ResideInNamespaceMatching($"{allowedNamespace}.*")
        );

        var rule = Types()
            .That()
            .ResideInNamespaceMatching($"{sourceNamespace}.*")
            .Should()
            .OnlyDependOn(allowedTypes);

        rule.Check(architecture);
    }
}
