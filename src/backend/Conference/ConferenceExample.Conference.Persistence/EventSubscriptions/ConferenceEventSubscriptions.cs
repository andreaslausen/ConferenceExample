using ConferenceExample.Conference.Domain.ConferenceManagement.Events;
using ConferenceExample.Conference.Domain.TalkManagement.Events;
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
    // Talk BC's and Speaker BC's event classes live in Talk.Domain/Speaker.Domain, which
    // Conference.Persistence must not reference (bounded context isolation, enforced by
    // ConferenceDependencyRules). These constants mirror the external event class names so the
    // subscription below is at least a named constant rather than an inline magic string.
    private const string TalkSubmittedToConferenceEventType = "TalkSubmittedToConferenceEvent";
    private const string SpeakerProfileCreatedEventType = "SpeakerProfileCreatedEvent";
    private const string SpeakerProfileUpdatedEventType = "SpeakerProfileUpdatedEvent";

    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            nameof(ConferenceCreatedEvent),
            (h, e) => h.HandleConferenceCreated(e)
        );
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            nameof(ConferenceRenamedEvent),
            (h, e) => h.HandleConferenceRenamed(e)
        );
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            nameof(ConferenceStatusChangedEvent),
            (h, e) => h.HandleConferenceStatusChanged(e)
        );
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            nameof(ConferenceDetailsUpdatedEvent),
            (h, e) => h.HandleConferenceDetailsUpdated(e)
        );
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            nameof(TalkTypeDefinedEvent),
            (h, e) => h.HandleTalkTypeDefined(e)
        );
        SubscribeConferenceHandler(
            eventBus,
            scopeFactory,
            nameof(TalkTypeRemovedEvent),
            (h, e) => h.HandleTalkTypeRemoved(e)
        );

        // Talk BC: a speaker submits one of their talks here. Their later edits are deliberately
        // not subscribed to — a registered submission keeps the content it was submitted with.
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            TalkSubmittedToConferenceEventType,
            (h, e) => h.HandleTalkSubmittedToConference(e)
        );

        // Speaker BC: profile details, snapshotted into a submission when it is registered
        SubscribeSpeakerHandler(
            eventBus,
            scopeFactory,
            SpeakerProfileCreatedEventType,
            (h, e) => h.HandleSpeakerProfileChanged(e)
        );
        SubscribeSpeakerHandler(
            eventBus,
            scopeFactory,
            SpeakerProfileUpdatedEventType,
            (h, e) => h.HandleSpeakerProfileChanged(e)
        );

        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            nameof(TalkAcceptedEvent),
            (h, e) => h.HandleTalkAccepted(e)
        );
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            nameof(TalkRejectedEvent),
            (h, e) => h.HandleTalkRejected(e)
        );
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            nameof(TalkScheduledEvent),
            (h, e) => h.HandleTalkScheduled(e)
        );
        SubscribeTalkHandler(
            eventBus,
            scopeFactory,
            nameof(TalkAssignedToRoomEvent),
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
                var handler = scope.ServiceProvider.GetRequiredService<ConferenceEventHandler>();
                await dispatch(handler, storedEvent);
            }
        );
    }

    private static void SubscribeSpeakerHandler(
        IEventBus eventBus,
        IServiceScopeFactory scopeFactory,
        string eventType,
        Func<SpeakerReplicaEventHandler, StoredEvent, Task> dispatch
    )
    {
        eventBus.Subscribe(
            eventType,
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler =
                    scope.ServiceProvider.GetRequiredService<SpeakerReplicaEventHandler>();
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
