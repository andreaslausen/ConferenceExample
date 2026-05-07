using ConferenceExample.EventStore;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

public class ConferenceEventReplicationHandler(ITalkEventStore talkEventStore)
{
    public async Task ReplicateEvent(StoredEvent storedEvent)
    {
        try
        {
            await talkEventStore.AppendEvents(
                storedEvent.AggregateId,
                [storedEvent],
                storedEvent.Version - 1
            );
        }
        catch (ConcurrencyException)
        {
            // Event already replicated — idempotent, nothing to do.
        }
    }
}
