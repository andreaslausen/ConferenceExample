using System.Text.Json;
using ConferenceExample.Conference.Application;
using ConferenceExample.Conference.Domain.Repositories;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;
using ConferenceExample.Conference.Persistence;
using ConferenceExample.EventStore;
using ConferenceExample.Session.Application;
using ConferenceExample.Session.Application.Commands;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Persistence;

namespace ConferenceExample.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        services.AddScoped<IConferenceRepository, ConferenceRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        services.AddScoped<ISubmitSessionCommandHandler, SubmitSessionCommandHandler>();

        services.AddScoped<IConferenceService, ConferenceService>();
        services.AddScoped<ISessionService, SessionService>();

        return services;
    }

    public static WebApplication AddEventBusSubscriptions(this WebApplication app)
    {
        var eventBus = app.Services.GetRequiredService<IEventBus>();
        var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

        eventBus.Subscribe(
            "SessionSubmittedEvent",
            storedEvent =>
            {
                var payload = JsonSerializer.Deserialize<SessionSubmittedPayload>(
                    storedEvent.Payload
                );
                if (payload is null)
                    return;

                using var scope = scopeFactory.CreateScope();
                var conferenceRepo =
                    scope.ServiceProvider.GetRequiredService<IConferenceRepository>();
                var conference = conferenceRepo
                    .GetById(new ConferenceId(new GuidV7(payload.ConferenceId)))
                    .Result;
                conference.SubmitSession(new SessionId(new GuidV7(storedEvent.AggregateId)));
                conferenceRepo.Save(conference).Wait();
            }
        );

        return app;
    }

    private record SessionSubmittedPayload(Guid ConferenceId);
}
