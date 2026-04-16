using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceExample.Talk.Persistence.EventSubscriptions;

/// <summary>
/// Registers event subscriptions for the Talk bounded context.
/// Updates Talk Read Models when Talk domain events occur.
/// </summary>
public static class TalkEventSubscriptions
{
    public static void Subscribe(IEventBus eventBus, IServiceScopeFactory scopeFactory)
    {
        eventBus.Subscribe(
            "TalkSubmittedEvent",
            async storedEvent =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TalkEventHandler>();
                await handler.HandleTalkSubmitted(storedEvent);
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
}
