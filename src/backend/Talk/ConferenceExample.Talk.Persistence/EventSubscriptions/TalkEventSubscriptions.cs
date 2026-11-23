using ConferenceExample.EventStore;
using ConferenceExample.Talk.Domain.TalkManagement.Events;
using ConferenceExample.Talk.Persistence.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Persistence.EventSubscriptions;

/// <summary>
/// Wires Talk BC read-model handlers to the in-memory event bus.
///
/// Besides its own events, the Talk BC listens to two other contexts:
/// the Speaker BC, to learn which speaker profile belongs to which account, and the Conference BC,
/// to learn what became of a submission (registered, accepted, rejected, or refused outright).
/// It keeps no other copy of their state.
/// </summary>
public static class TalkEventSubscriptions
{
    // Conference BC's and Speaker BC's event classes live in Conference.Domain/Speaker.Domain,
    // which Talk.Persistence must not reference (bounded context isolation, enforced by
    // TalkDependencyRules). These constants mirror the external event class names so the
    // subscriptions below are at least named constants rather than inline magic strings.
    private const string TalkSubmissionRegisteredEventType = "TalkSubmissionRegisteredEvent";
    private const string TalkSubmissionRejectedEventType = "TalkSubmissionRejectedEvent";
    private const string TalkAcceptedEventType = "TalkAcceptedEvent";
    private const string TalkRejectedEventType = "TalkRejectedEvent";
    private const string SpeakerProfileCreatedEventType = "SpeakerProfileCreatedEvent";
    private const string ConferenceCreatedEventType = "ConferenceCreatedEvent";
    private const string ConferenceRenamedEventType = "ConferenceRenamedEvent";
    private const string ConferenceDetailsUpdatedEventType = "ConferenceDetailsUpdatedEvent";

    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        // Talk's own events
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            nameof(TalkCreatedEvent),
            (h, e) => h.HandleTalkCreated(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            nameof(TalkTitleEditedEvent),
            (h, e) => h.HandleTalkTitleEdited(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            nameof(TalkAbstractEditedEvent),
            (h, e) => h.HandleTalkAbstractEdited(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            nameof(TalkTagAddedEvent),
            (h, e) => h.HandleTalkTagAdded(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            nameof(TalkTagRemovedEvent),
            (h, e) => h.HandleTalkTagRemoved(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            nameof(TalkDeletedEvent),
            (h, e) => h.HandleTalkDeleted(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            nameof(TalkSubmittedToConferenceEvent),
            (h, e) => h.HandleTalkSubmittedToConference(e)
        );

        // Conference BC: what became of a submission
        SubscribeHandler<TalkSubmissionEventHandler>(
            eventBus,
            scopeFactory,
            TalkSubmissionRegisteredEventType,
            (h, e) => h.HandleSubmissionRegistered(e)
        );
        SubscribeHandler<TalkSubmissionEventHandler>(
            eventBus,
            scopeFactory,
            TalkSubmissionRejectedEventType,
            (h, e) => h.HandleSubmissionRejected(e)
        );
        SubscribeHandler<TalkSubmissionEventHandler>(
            eventBus,
            scopeFactory,
            TalkAcceptedEventType,
            (h, e) => h.HandleTalkAccepted(e)
        );
        SubscribeHandler<TalkSubmissionEventHandler>(
            eventBus,
            scopeFactory,
            TalkRejectedEventType,
            (h, e) => h.HandleTalkRejected(e)
        );

        // Speaker BC: which profile belongs to which account
        SubscribeHandler<SpeakerReplicaEventHandler>(
            eventBus,
            scopeFactory,
            SpeakerProfileCreatedEventType,
            (h, e) => h.HandleSpeakerProfileCreated(e)
        );

        // Conference BC: names for the speaker's submission list
        SubscribeHandler<ConferenceReplicaEventHandler>(
            eventBus,
            scopeFactory,
            ConferenceCreatedEventType,
            (h, e) => h.HandleConferenceCreated(e)
        );
        SubscribeHandler<ConferenceReplicaEventHandler>(
            eventBus,
            scopeFactory,
            ConferenceRenamedEventType,
            (h, e) => h.HandleConferenceRenamed(e)
        );
        SubscribeHandler<ConferenceReplicaEventHandler>(
            eventBus,
            scopeFactory,
            ConferenceDetailsUpdatedEventType,
            (h, e) => h.HandleConferenceRenamed(e)
        );
    }

    private static void SubscribeHandler<THandler>(
        IEventBus eventBus,
        IServiceScopeFactory scopeFactory,
        string eventType,
        Func<THandler, StoredEvent, Task> dispatch
    )
        where THandler : notnull
    {
        eventBus.Subscribe(
            eventType,
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                await dispatch(handler, storedEvent);
            }
        );
    }
}
