using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Persistence.EventSubscriptions;

/// <summary>
/// Registers event subscriptions for the Talk bounded context.
/// Updates Talk Read Models when any Talk domain event occurs.
/// All domain events now contain the complete aggregate state.
///
/// Note: Conference events are NOT subscribed here anymore.
/// Commands that need Conference state load it directly from the EventStore via IConferenceRepository.
/// This ensures the EventStore is the Single Source of Truth for write operations.
/// </summary>
public static class TalkEventSubscriptions
{
    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        // Subscribe to all Talk domain events - they all contain complete aggregate state
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
    }
}
