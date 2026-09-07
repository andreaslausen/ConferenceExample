using ConferenceExample.EventStore;
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
    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        // Talk's own events
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            "TalkCreatedEvent",
            (h, e) => h.HandleTalkCreated(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            "TalkTitleEditedEvent",
            (h, e) => h.HandleTalkTitleEdited(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            "TalkAbstractEditedEvent",
            (h, e) => h.HandleTalkAbstractEdited(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            "TalkTagAddedEvent",
            (h, e) => h.HandleTalkTagAdded(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            "TalkTagRemovedEvent",
            (h, e) => h.HandleTalkTagRemoved(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            "TalkDeletedEvent",
            (h, e) => h.HandleTalkDeleted(e)
        );
        SubscribeHandler<TalkEventHandler>(
            eventBus,
            scopeFactory,
            "TalkSubmittedToConferenceEvent",
            (h, e) => h.HandleTalkSubmittedToConference(e)
        );

        // Conference BC: what became of a submission
        SubscribeHandler<TalkSubmissionEventHandler>(
            eventBus,
            scopeFactory,
            "TalkSubmissionRegisteredEvent",
            (h, e) => h.HandleSubmissionRegistered(e)
        );
        SubscribeHandler<TalkSubmissionEventHandler>(
            eventBus,
            scopeFactory,
            "TalkSubmissionRejectedEvent",
            (h, e) => h.HandleSubmissionRejected(e)
        );
        SubscribeHandler<TalkSubmissionEventHandler>(
            eventBus,
            scopeFactory,
            "TalkAcceptedEvent",
            (h, e) => h.HandleTalkAccepted(e)
        );
        SubscribeHandler<TalkSubmissionEventHandler>(
            eventBus,
            scopeFactory,
            "TalkRejectedEvent",
            (h, e) => h.HandleTalkRejected(e)
        );

        // Speaker BC: which profile belongs to which account
        SubscribeHandler<SpeakerDirectoryEventHandler>(
            eventBus,
            scopeFactory,
            "SpeakerProfileCreatedEvent",
            (h, e) => h.HandleSpeakerProfileCreated(e)
        );

        // Conference BC: names for the speaker's submission list
        SubscribeHandler<ConferenceDirectoryEventHandler>(
            eventBus,
            scopeFactory,
            "ConferenceCreatedEvent",
            (h, e) => h.HandleConferenceCreated(e)
        );
        SubscribeHandler<ConferenceDirectoryEventHandler>(
            eventBus,
            scopeFactory,
            "ConferenceRenamedEvent",
            (h, e) => h.HandleConferenceRenamed(e)
        );
        SubscribeHandler<ConferenceDirectoryEventHandler>(
            eventBus,
            scopeFactory,
            "ConferenceDetailsUpdatedEvent",
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
