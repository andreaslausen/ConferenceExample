using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.EventStore;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Conference.Persistence.EventSubscriptions;

/// <summary>
/// Wires Conference BC read-model handlers to the in-memory event bus.
/// Each slim event is dispatched to its dedicated handler method.
/// </summary>
public static class ConferenceEventSubscriptions
{
    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            "ConferenceCreatedEvent",
            (h, e) => h.HandleConferenceCreated(e)
        );
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            "ConferenceRenamedEvent",
            (h, e) => h.HandleConferenceRenamed(e)
        );
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            "ConferenceStatusChangedEvent",
            (h, e) => h.HandleConferenceStatusChanged(e)
        );
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            "ConferenceDetailsUpdatedEvent",
            (h, e) => h.HandleConferenceDetailsUpdated(e)
        );
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            "TalkTypeDefinedEvent",
            (h, e) => h.HandleTalkTypeDefined(e)
        );
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            "TalkTypeRemovedEvent",
            (h, e) => h.HandleTalkTypeRemoved(e)
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
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            "TalkAcceptedEvent",
            (h, e) => h.HandleTalkAccepted(e)
        );
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            "TalkRejectedEvent",
            (h, e) => h.HandleTalkRejected(e)
        );
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            "TalkScheduledEvent",
            (h, e) => h.HandleTalkScheduled(e)
        );
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            "TalkAssignedToRoomEvent",
            (h, e) => h.HandleTalkAssignedToRoom(e)
        );
    }

    private static void SubscribeConferenceHandler(
        IEventBus eventBus,
        IServiceScopeFactory scopeFactory,
        string eventType,
        Func<ConferenceEventHandler, StoredEvent, Task> dispatch
    )
    {
        eventBus.Subscribe(
            eventType,
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler =
                    scope.ServiceProvider.GetRequiredService<ConferenceEventHandler>();
                await dispatch(handler, storedEvent);
            }
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
}
