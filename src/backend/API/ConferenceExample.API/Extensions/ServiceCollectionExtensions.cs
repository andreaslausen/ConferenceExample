using ConferenceExample.Conference.Persistence.EventSubscriptions;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Persistence.EventSubscriptions;

namespace ConferenceExample.API.Extensions;

/// <summary>
/// Extension methods for registering event bus subscriptions.
/// Delegates to bounded context-specific subscription classes for better organization.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static WebApplication AddEventBusSubscriptions(this WebApplication app)
    {
        var eventBus = app.Services.GetRequiredService<IEventBus>();
        var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

        // Register Talk BC subscriptions
        TalkEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // Register Conference BC subscriptions
        ConferenceEventSubscriptions.Subscribe(eventBus, scopeFactory);

        // Register cross-BC synchronization subscriptions
        ConferenceTalkSynchronizationSubscriptions.Subscribe(eventBus, scopeFactory);

        return app;
    }
}
