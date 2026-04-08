namespace ConferenceExample.EventStore;

public class InMemoryEventStore : IEventStore
{
    private readonly Lock _lock = new();
    private readonly List<StoredEvent> _events = [];

    public Task AppendEvents(
        Guid aggregateId,
        IEnumerable<StoredEvent> events,
        long expectedVersion
    )
    {
        lock (_lock)
        {
            var currentVersion = _events.Where(e => e.AggregateId == aggregateId).Count() - 1;

            if (currentVersion != expectedVersion)
            {
                throw new ConcurrencyException(
                    $"Expected version {expectedVersion} but found {currentVersion} for aggregate {aggregateId}."
                );
            }

            _events.AddRange(events);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StoredEvent>> GetEvents(Guid aggregateId)
    {
        lock (_lock)
        {
            var events = _events
                .Where(e => e.AggregateId == aggregateId)
                .OrderBy(e => e.Version)
                .ToList();

            return Task.FromResult<IReadOnlyList<StoredEvent>>(events);
        }
    }

    public Task<IReadOnlyList<StoredEvent>> GetAllEvents()
    {
        lock (_lock)
        {
            var events = _events.OrderBy(e => e.Version).ToList();
            return Task.FromResult<IReadOnlyList<StoredEvent>>(events);
        }
    }
}
