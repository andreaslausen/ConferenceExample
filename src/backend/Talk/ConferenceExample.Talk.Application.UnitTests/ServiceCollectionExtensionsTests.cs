using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.TalkManagement;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Application.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTalkApplication_RegistersSubmitTalkCommandHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkApplication();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ISubmitTalkCommandHandler)
        );
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(SubmitTalkCommandHandler), descriptor!.ImplementationType);
    }

    [Fact]
    public void AddTalkApplication_RegistersEditTalkCommandHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkApplication();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IEditTalkCommandHandler)
        );
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(EditTalkCommandHandler), descriptor!.ImplementationType);
    }

    [Fact]
    public void AddTalkApplication_RegistersGetMyTalksQueryHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkApplication();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IGetMyTalksQueryHandler)
        );
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(GetMyTalksQueryHandler), descriptor!.ImplementationType);
    }

    [Fact]
    public void AddTalkApplication_RegistersTalkService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkApplication();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITalkService));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(TalkService), descriptor!.ImplementationType);
    }

    [Fact]
    public void AddTalkApplication_RegistersAllServicesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkApplication();

        // Assert
        var commandHandlerDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ISubmitTalkCommandHandler)
        );
        Assert.NotNull(commandHandlerDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, commandHandlerDescriptor!.Lifetime);

        var serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITalkService));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor!.Lifetime);
    }

    [Fact]
    public void AddTalkApplication_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddTalkApplication();

        // Assert
        Assert.Same(services, result);
    }
}
