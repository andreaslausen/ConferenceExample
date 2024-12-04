using ArchUnitNET.Domain.Dependencies;
using ArchUnitNET.Domain.Extensions;

namespace ConferenceExample.ArchitectureTests;

public class ApplicationRules : ArchitectureTest
{
    [Fact]
    public void ApplicationServices_ShouldNotCallOtherApplicationServices()
    {
        var applicationServices =
            Architecture.Classes.Where(s => s.Namespace.NameContains("Application") && s.NameEndsWith("Service"));

        Assert.All(applicationServices,
            c => Assert.Null(c.Dependencies.Find(d =>
                d is not ImplementsInterfaceDependency &&
                d.Origin.FullName != d.Target.FullName && d.Target.NameContains("Service") &&
                d.Target.FullNameContains("Application"))));
    }
}