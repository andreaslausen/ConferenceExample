using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Persistence.EventSubscriptions;

/// <summary>
/// Wires Talk BC read-model handlers to the in-memory event bus.
/// Conference BC status events are also projected into a minimal local
/// ConferenceStatusDocument so the Talk BC can validate conference existence in command handlers.
/// TalkSubmissionRejectedEvent (raised by Conference when it isn't accepting submissions) updates
/// the submitting talk's own read model to Rejected.
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
                    scope.ServiceProvider.GetRequiredService<ConferenceStatusEventHandler>();
                await handler.HandleConferenceCreated(storedEvent);
            }
        );
        eventBus.Subscribe(
            "ConferenceStatusChangedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler =
                    scope.ServiceProvider.GetRequiredService<ConferenceStatusEventHandler>();
                await handler.HandleConferenceStatusChanged(storedEvent);
            }
        );

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
