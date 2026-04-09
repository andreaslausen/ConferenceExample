namespace ConferenceExample.EventStore;

public interface IEventStore
{
    Task AppendEvents(Guid aggregateId, IEnumerable<StoredEvent> events, long expectedVersion);

    Task<IReadOnlyList<StoredEvent>> GetEvents(Guid aggregateId);
    Task<IReadOnlyList<StoredEvent>> GetAllEvents();
}
