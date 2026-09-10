using ConferenceExample.EventStore;
using ConferenceExample.Speaker.Persistence.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Speaker.Persistence.EventSubscriptions;

/// <summary>
/// Wires Speaker BC read-model handlers to the in-memory event bus. The Speaker BC is a source
/// context: it publishes profile events for Talk and Conference to project, and consumes none.
/// </summary>
public static class SpeakerEventSubscriptions
{
    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        SubscribeSpeakerHandler(
            eventBus,
            scopeFactory,
            "SpeakerProfileCreatedEvent",
            (h, e) => h.HandleSpeakerProfileCreated(e)
        );
        SubscribeSpeakerHandler(
            eventBus,
            scopeFactory,
            "SpeakerProfileUpdatedEvent",
            (h, e) => h.HandleSpeakerProfileUpdated(e)
        );
    }

    private static void SubscribeSpeakerHandler(
        IEventBus eventBus,
        IServiceScopeFactory scopeFactory,
        string eventType,
        Func<SpeakerEventHandler, StoredEvent, Task> dispatch
    )
    {
        eventBus.Subscribe(
            eventType,
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<SpeakerEventHandler>();
                await dispatch(handler, storedEvent);
            }
        );
    }
}
