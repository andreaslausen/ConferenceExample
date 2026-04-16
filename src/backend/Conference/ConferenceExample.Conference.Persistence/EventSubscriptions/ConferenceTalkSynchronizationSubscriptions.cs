using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.EventStore;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Conference.Persistence.EventSubscriptions;

/// <summary>
/// Registers event subscriptions for cross-BC synchronization from Talk BC to Conference BC.
/// Updates Conference BC's Talk Read Models when Talk events occur.
/// This enables eventual consistency between the two bounded contexts.
/// </summary>
public static class ConferenceTalkSynchronizationSubscriptions
{
    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        // Talk lifecycle events from Talk BC -> replicate to Conference BC
        SubscribeToTalkLifecycleEvents(eventBus, scopeFactory);

        // Conference-specific talk management events (Accepted, Rejected, Scheduled, etc.)
        SubscribeToConferenceTalkManagementEvents(eventBus, scopeFactory);
    }

    private static void SubscribeToTalkLifecycleEvents(
        IEventBus eventBus,
        IServiceScopeFactory scopeFactory
    )
    {
        eventBus.Subscribe(
            "TalkSubmittedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();

                // Update Conference BC Talk Read Model
                var talkHandler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await talkHandler.HandleTalkSubmitted(storedEvent);
            }
        );

        eventBus.Subscribe(
            "TalkTitleEditedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await handler.HandleTalkTitleEdited(storedEvent);
            }
        );

        eventBus.Subscribe(
            "TalkAbstractEditedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await handler.HandleTalkAbstractEdited(storedEvent);
            }
        );

        eventBus.Subscribe(
            "TalkTagAddedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await handler.HandleTalkTagAdded(storedEvent);
            }
        );

        eventBus.Subscribe(
            "TalkTagRemovedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await handler.HandleTalkTagRemoved(storedEvent);
            }
        );
    }

    private static void SubscribeToConferenceTalkManagementEvents(
        IEventBus eventBus,
        IServiceScopeFactory scopeFactory
    )
    {
        eventBus.Subscribe(
            "TalkAcceptedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await handler.HandleTalkAccepted(storedEvent);
            }
        );

        eventBus.Subscribe(
            "TalkRejectedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await handler.HandleTalkRejected(storedEvent);
            }
        );

        eventBus.Subscribe(
            "TalkScheduledEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await handler.HandleTalkScheduled(storedEvent);
            }
        );

        eventBus.Subscribe(
            "TalkAssignedToRoomEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await handler.HandleTalkAssignedToRoom(storedEvent);
            }
        );
    }
}
