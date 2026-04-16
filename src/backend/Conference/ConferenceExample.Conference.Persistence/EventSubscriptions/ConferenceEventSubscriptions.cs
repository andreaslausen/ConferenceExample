using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.EventStore;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Conference.Persistence.EventSubscriptions;

/// <summary>
/// Registers event subscriptions for the Conference bounded context.
/// Updates Conference Read Models when domain events occur.
/// All domain events now contain the complete aggregate state.
/// </summary>
public static class ConferenceEventSubscriptions
{
    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        // Subscribe to all Conference domain events - they all contain complete aggregate state
        var conferenceDomainEvents = new[]
        {
            "ConferenceCreatedEvent",
            "ConferenceRenamedEvent",
            "ConferenceStatusChangedEvent",
        };

        foreach (var eventType in conferenceDomainEvents)
        {
            eventBus.Subscribe(
                eventType,
                async storedEvent =>
                {
                    using var scope = scopeFactory.CreateScope();
                    var handler =
                        scope.ServiceProvider.GetRequiredService<ConferenceEventHandler>();
                    await handler.HandleConferenceDomainEvent(storedEvent);
                }
            );
        }

        // Subscribe to Talk domain events (for cross-BC synchronization)
        var talkDomainEvents = new[]
        {
            "TalkSubmittedEvent",
            "TalkTitleEditedEvent",
            "TalkAbstractEditedEvent",
            "TalkTagAddedEvent",
            "TalkTagRemovedEvent",
        };

        foreach (var eventType in talkDomainEvents)
        {
            eventBus.Subscribe(
                eventType,
                async storedEvent =>
                {
                    using var scope = scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                    await handler.HandleTalkDomainEvent(storedEvent);
                }
            );
        }

        // Conference BC specific events (managed by Conference aggregate)
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
