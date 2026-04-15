using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Application.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTalkContext_RegistersTalkRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkContext();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<ITalkRepository>();
        Assert.NotNull(repository);
        Assert.IsType<TalkRepository>(repository);
    }

    [Fact]
    public void AddTalkContext_RegistersSubmitTalkCommandHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkContext();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var handler = serviceProvider.GetService<ISubmitTalkCommandHandler>();
        Assert.NotNull(handler);
        Assert.IsType<SubmitTalkCommandHandler>(handler);
    }

    [Fact]
    public void AddTalkContext_RegistersEditTalkCommandHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkContext();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var handler = serviceProvider.GetService<IEditTalkCommandHandler>();
        Assert.NotNull(handler);
        Assert.IsType<EditTalkCommandHandler>(handler);
    }

    [Fact]
    public void AddTalkContext_RegistersGetMyTalksQueryHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkContext();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var handler = serviceProvider.GetService<IGetMyTalksQueryHandler>();
        Assert.NotNull(handler);
        Assert.IsType<GetMyTalksQueryHandler>(handler);
    }

    [Fact]
    public void AddTalkContext_RegistersTalkService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkContext();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var service = serviceProvider.GetService<ITalkService>();
        Assert.NotNull(service);
        Assert.IsType<TalkService>(service);
    }

    [Fact]
    public void AddTalkContext_RegistersAllServicesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTalkContext();

        // Assert
        var repositoryDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ITalkRepository)
        );
        Assert.NotNull(repositoryDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, repositoryDescriptor!.Lifetime);

        var serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITalkService));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor!.Lifetime);
    }

    [Fact]
    public void AddTalkContext_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddTalkContext();

        // Assert
        Assert.Same(services, result);
    }
}
