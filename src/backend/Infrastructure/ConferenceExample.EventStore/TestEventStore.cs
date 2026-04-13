namespace ConferenceExample.EventStore;

/// <summary>
/// Simple in-memory event store for testing purposes.
/// This is NOT intended for production use - use MongoDbEventStore instead.
/// </summary>
public class TestEventStore : IEventStore
{
    private readonly Dictionary<Guid, List<StoredEvent>> _events = [];
    private readonly Lock _lock = new();

    public Task AppendEvents(
        Guid aggregateId,
        IEnumerable<StoredEvent> events,
        long expectedVersion
    )
    {
        lock (_lock)
        {
            var currentVersion = GetCurrentVersion(aggregateId);

            if (currentVersion != expectedVersion)
            {
                throw new ConcurrencyException(
                    $"Expected version {expectedVersion} but found {currentVersion} for aggregate {aggregateId}."
                );
            }

            if (!_events.ContainsKey(aggregateId))
            {
                _events[aggregateId] = [];
            }

            _events[aggregateId].AddRange(events);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StoredEvent>> GetEvents(Guid aggregateId)
    {
        lock (_lock)
        {
            if (!_events.TryGetValue(aggregateId, out var events))
            {
                return Task.FromResult<IReadOnlyList<StoredEvent>>([]);
            }

            return Task.FromResult<IReadOnlyList<StoredEvent>>(
                events.OrderBy(e => e.Version).ToList()
            );
        }
    }

    public Task<IReadOnlyList<StoredEvent>> GetAllEvents()
    {
        lock (_lock)
        {
            var allEvents = _events.Values.SelectMany(e => e).OrderBy(e => e.Version).ToList();
            return Task.FromResult<IReadOnlyList<StoredEvent>>(allEvents);
        }
    }

    private long GetCurrentVersion(Guid aggregateId)
    {
        if (!_events.TryGetValue(aggregateId, out var events) || events.Count == 0)
        {
            return -1;
        }

        return events.Max(e => e.Version);
    }
}
