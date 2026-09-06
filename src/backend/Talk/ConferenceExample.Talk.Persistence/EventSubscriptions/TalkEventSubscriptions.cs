using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Persistence.EventSubscriptions;

/// <summary>
/// Wires Talk BC read-model handlers to the in-memory event bus.
/// TalkSubmissionRejectedEvent (raised by Conference — either because it isn't accepting
/// submissions, or because it doesn't exist at all) updates the submitting talk's own read model
/// to Rejected. Talk keeps no other local knowledge of Conference state.
/// </summary>
public static class TalkEventSubscriptions
{
    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        eventBus.Subscribe(
            "ConferenceCreatedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler =
                    scope.ServiceProvider.GetRequiredService<ConferenceOrganizerEventHandler>();
                await handler.HandleConferenceCreated(storedEvent);
            }
        );

        eventBus.Subscribe(
            "TalkSubmissionRejectedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await handler.HandleTalkSubmissionRejected(storedEvent);
            }
        );

        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            "TalkSubmittedEvent",
            (h, e) => h.HandleTalkSubmitted(e)
        );
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            "TalkTitleEditedEvent",
            (h, e) => h.HandleTalkTitleEdited(e)
        );
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            "TalkAbstractEditedEvent",
            (h, e) => h.HandleTalkAbstractEdited(e)
        );
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            "TalkTagAddedEvent",
            (h, e) => h.HandleTalkTagAdded(e)
        );
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            "TalkTagRemovedEvent",
            (h, e) => h.HandleTalkTagRemoved(e)
        );

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

    private static void SubscribeTalkHandler(
        IEventBus eventBus,
        IServiceScopeFactory scopeFactory,
        string eventType,
        Func<TalkEventHandler, StoredEvent, Task> dispatch
    )
    {
        eventBus.Subscribe(
            eventType,
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await dispatch(handler, storedEvent);
            }
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
