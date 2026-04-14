using System.Text.Json;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceExample.EventStore;

namespace ConferenceExample.API.Extensions;

public static class ServiceCollectionExtensions
{
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
