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
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Conference.Application.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddConferenceApplication_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddConferenceApplication();

        // Assert - Command Handlers
        Assert.Contains(services, sd => sd.ServiceType == typeof(ICreateConferenceCommandHandler));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IRenameConferenceCommandHandler));
        Assert.Contains(
            services,
            sd => sd.ServiceType == typeof(IChangeConferenceStatusCommandHandler)
        );
        Assert.Contains(services, sd => sd.ServiceType == typeof(IAcceptTalkCommandHandler));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IRejectTalkCommandHandler));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IScheduleTalkCommandHandler));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IAssignTalkToRoomCommandHandler));

        // Assert - Query Handlers
        Assert.Contains(services, sd => sd.ServiceType == typeof(IGetAllConferencesQueryHandler));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IGetConferenceByIdQueryHandler));
        Assert.Contains(
            services,
            sd => sd.ServiceType == typeof(IGetConferenceSessionsQueryHandler)
        );
        Assert.Contains(services, sd => sd.ServiceType == typeof(IGetConferenceTalksQueryHandler));

        // Assert - Services
        Assert.Contains(services, sd => sd.ServiceType == typeof(IConferenceService));
    }

    [Fact]
    public void AddConferenceApplication_RegistersServicesWithCorrectLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddConferenceApplication();

        // Assert - All services should be registered as Scoped
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
    public void AddConferenceApplication_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddConferenceApplication();

        // Assert
        Assert.Same(services, result);
    }
}
