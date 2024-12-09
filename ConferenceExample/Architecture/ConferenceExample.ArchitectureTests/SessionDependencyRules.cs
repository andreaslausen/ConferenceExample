using ArchUnitNET.Fluent.Syntax.Elements.Types;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

public class SessionDependencyRules : ArchitectureTest
{
    [Fact]
    public void SessionDomain_ShouldOnlyDependOnItself()
    {
        var rule = CheckDependencies(SessionDomain, [], "System");

        rule.Check(Architecture);
    }
    
    [Fact]
    public void SessionApplication_ShouldOnlyDependOnItselfAndDomain()
    {
        var rule = CheckDependencies(SessionApplication, [SessionDomain], "System");

        rule.Check(Architecture);
    }
    
    [Fact]
    public void SessionPersistence_ShouldOnlyDependOnItselfAndDomainAndPersistence()
    {
        var rule = CheckDependencies(SessionPersistence, [SessionDomain, Persistence], "System");

        rule.Check(Architecture);
    }

    private static TypesShouldConjunction CheckDependencies(System.Reflection.Assembly source,
        IEnumerable<System.Reflection.Assembly> target,
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

        return rule;
    }
}