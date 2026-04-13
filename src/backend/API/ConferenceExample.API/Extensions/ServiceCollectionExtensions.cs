using System.Text.Json;
using ConferenceExample.Conference.Application;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Domain.Repositories;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;
using ConferenceExample.Conference.Persistence;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.Repositories;
using ConferenceExample.Talk.Persistence;

namespace ConferenceExample.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        services.AddScoped<IConferenceRepository, ConferenceRepository>();
        services.AddScoped<ITalkRepository, TalkRepository>();

        services.AddScoped<ICreateConferenceCommandHandler, CreateConferenceCommandHandler>();
        services.AddScoped<IRenameConferenceCommandHandler, RenameConferenceCommandHandler>();
        services.AddScoped<ISubmitTalkCommandHandler, SubmitTalkCommandHandler>();
        services.AddScoped<IGetConferenceSessionsQueryHandler, GetConferenceSessionsQueryHandler>();

        services.AddScoped<IConferenceService, ConferenceService>();
        services.AddScoped<ITalkService, TalkService>();

        return services;
    }

    public static WebApplication AddEventBusSubscriptions(this WebApplication app)
    {
        var eventBus = app.Services.GetRequiredService<IEventBus>();
        var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

        eventBus.Subscribe(
            "TalkSubmittedEvent",
            storedEvent =>
            {
                var payload = JsonSerializer.Deserialize<TalkSubmittedPayload>(storedEvent.Payload);
                if (payload is null)
                    return;

                using var scope = scopeFactory.CreateScope();
                var conferenceRepo =
                    scope.ServiceProvider.GetRequiredService<IConferenceRepository>();
                var conference = conferenceRepo
                    .GetById(new ConferenceId(new GuidV7(payload.ConferenceId)))
                    .Result;
                conference.SubmitTalk(new TalkId(new GuidV7(storedEvent.AggregateId)));
                conferenceRepo.Save(conference).Wait();
            }
        );

        return app;
    }

    private record TalkSubmittedPayload(Guid ConferenceId);
}
