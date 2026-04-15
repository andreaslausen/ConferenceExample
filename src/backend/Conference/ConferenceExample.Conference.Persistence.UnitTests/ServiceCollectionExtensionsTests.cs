using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.TalkManagement;
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
    public void AddConferencePersistence_RegistersTalkRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddConferencePersistence();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITalkRepository));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(TalkRepository), descriptor!.ImplementationType);
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

        var talkRepositoryDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ITalkRepository)
        );
        Assert.NotNull(talkRepositoryDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, talkRepositoryDescriptor!.Lifetime);
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
