using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceExample.Conference.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Conference.Persistence.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddConferencePersistence_RegistersConferenceRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddConferencePersistence();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IConferenceRepository)
        );
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(ConferenceRepository), descriptor!.ImplementationType);
    }

    [Fact]
    public void AddConferencePersistence_RegistersConferenceReadModelRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddConferencePersistence();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IConferenceReadModelRepository)
        );
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddConferencePersistence_RegistersConferenceTalkReadModelRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddConferencePersistence();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IConferenceTalkReadModelRepository)
        );
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddConferencePersistence_RegistersRepositoriesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddConferencePersistence();

        // Assert
        var conferenceRepositoryDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IConferenceRepository)
        );
        Assert.NotNull(conferenceRepositoryDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, conferenceRepositoryDescriptor!.Lifetime);

        var readModelRepositoryDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IConferenceReadModelRepository)
        );
        Assert.NotNull(readModelRepositoryDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, readModelRepositoryDescriptor!.Lifetime);
    }

    [Fact]
    public void AddConferencePersistence_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddConferencePersistence();

        // Assert
        Assert.Same(services, result);
    }
}
