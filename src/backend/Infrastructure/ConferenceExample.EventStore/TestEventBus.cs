namespace ConferenceExample.EventStore;

/// <summary>
/// Simple in-memory event bus for testing purposes.
/// This is NOT intended for production use - use MongoDbEventBus instead.
/// </summary>
public class TestEventBus : IEventBus
{
    private readonly Dictionary<string, List<Action<StoredEvent>>> _subscriptions = [];
    private readonly Lock _lock = new();

    public void Subscribe(string eventType, Action<StoredEvent> handler)
    {
        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(eventType, out var handlers))
            {
                handlers = [];
                _subscriptions[eventType] = handlers;
            }

            handlers.Add(handler);
        }
    }

    public Task Publish(IEnumerable<StoredEvent> events)
    {
        foreach (var storedEvent in events)
        {
            List<Action<StoredEvent>> handlers;

            lock (_lock)
            {
                if (!_subscriptions.TryGetValue(storedEvent.EventType, out var registered))
                {
                    continue;
                }

                handlers = [.. registered];
            }

            foreach (var handler in handlers)
            {
                handler(storedEvent);
            }
        }

        return Task.CompletedTask;
    }
}
