using ConferenceExample.Conference.Persistence.EventHandlers;
using ConferenceExample.EventStore;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Conference.Persistence.EventSubscriptions;

/// <summary>
/// Registers event subscriptions for the Conference bounded context.
/// Updates Conference Read Models when Conference domain events occur.
/// </summary>
public static class ConferenceEventSubscriptions
{
    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        eventBus.Subscribe(
            "ConferenceCreatedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<ConferenceEventHandler>();
                await handler.HandleConferenceCreated(storedEvent);
            }
        );

        eventBus.Subscribe(
            "ConferenceRenamedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<ConferenceEventHandler>();
                await handler.HandleConferenceRenamed(storedEvent);
            }
        );

        eventBus.Subscribe(
            "ConferenceStatusChangedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<ConferenceEventHandler>();
                await handler.HandleConferenceStatusChanged(storedEvent);
            }
        );
    }
}
