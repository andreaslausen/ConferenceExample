using ConferenceExample.Conference.Application.AcceptTalk;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.GetConferenceTalks;
using ConferenceExample.Conference.Application.RejectTalk;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceExample.EventStore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ConferenceExample.Conference.Application.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddConferenceContext_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Mock IEventStore and ICurrentUserService since they're required by repositories and handlers
        services.AddScoped<Authentication.ICurrentUserService>(_ =>
            Substitute.For<Authentication.ICurrentUserService>()
        );

        // Act
        services.AddConferenceContext();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Repositories
        Assert.NotNull(serviceProvider.GetService<IConferenceRepository>());
        Assert.NotNull(serviceProvider.GetService<ITalkRepository>());

        // Assert - Command Handlers
        Assert.NotNull(serviceProvider.GetService<ICreateConferenceCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<IRenameConferenceCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<IChangeConferenceStatusCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<IAcceptTalkCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<IRejectTalkCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<IScheduleTalkCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<IAssignTalkToRoomCommandHandler>());

        // Assert - Query Handlers
        Assert.NotNull(serviceProvider.GetService<IGetAllConferencesQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<IGetConferenceByIdQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<IGetConferenceSessionsQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<IGetConferenceTalksQueryHandler>());

        // Assert - Services
        Assert.NotNull(serviceProvider.GetService<IConferenceService>());
    }

    [Fact]
    public void AddConferenceContext_RegistersServicesWithCorrectLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddConferenceContext();

        // Assert - All services should be registered as Scoped
        Assert.Contains(
            services,
            sd =>
                sd.ServiceType == typeof(IConferenceRepository)
                && sd.Lifetime == ServiceLifetime.Scoped
        );
        Assert.Contains(
            services,
            sd =>
                sd.ServiceType == typeof(ICreateConferenceCommandHandler)
                && sd.Lifetime == ServiceLifetime.Scoped
        );
        Assert.Contains(
            services,
            sd =>
                sd.ServiceType == typeof(IGetAllConferencesQueryHandler)
                && sd.Lifetime == ServiceLifetime.Scoped
        );
        Assert.Contains(
            services,
            sd =>
                sd.ServiceType == typeof(IConferenceService)
                && sd.Lifetime == ServiceLifetime.Scoped
        );
    }

    [Fact]
    public void AddConferenceContext_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddConferenceContext();

        // Assert
        Assert.Same(services, result);
    }
}
