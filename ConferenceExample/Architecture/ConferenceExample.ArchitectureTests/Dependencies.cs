using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
namespace ConferenceExample.ArchitectureTests;

public static class Dependencies
{
    public static void Check(System.Reflection.Assembly source,
        System.Reflection.Assembly[] target,
        params string[] allowedNamespaces)
    {
        var allowedTypes = Types().That().ResideInAssembly(source);
        allowedTypes = target.Aggregate(allowedTypes, (current, assembly)
            => current.Or().ResideInAssembly(assembly));
        allowedTypes = allowedNamespaces.Aggregate(allowedTypes, (current, allowedNamespace)
            => current.Or().ResideInNamespace($"{allowedNamespace}.*", true));
    
        var rule = Types()
            .That()
            .ResideInAssembly(source)
            .Should()
            .OnlyDependOn(allowedTypes);
    
        var architecture = new ArchLoader()
            .LoadAssemblyIncludingDependencies(source)
            .LoadAssemblies(target)
            .Build();
        
        rule.Check(architecture);
    }
}