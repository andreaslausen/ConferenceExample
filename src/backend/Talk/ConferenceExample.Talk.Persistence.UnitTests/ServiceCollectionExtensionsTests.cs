using ConferenceExample.Talk.Domain.TalkManagement;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Persistence.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTalkPersistence_RegistersTalkRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkPersistence();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITalkRepository));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(TalkRepository), descriptor!.ImplementationType);
    }

    [Fact]
    public void AddTalkPersistence_RegistersRepositoryAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkPersistence();

        // Assert
        var repositoryDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ITalkRepository)
        );
        Assert.NotNull(repositoryDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, repositoryDescriptor!.Lifetime);
    }

    [Fact]
    public void AddTalkPersistence_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddTalkPersistence();

        // Assert
        Assert.Same(services, result);
    }
}
