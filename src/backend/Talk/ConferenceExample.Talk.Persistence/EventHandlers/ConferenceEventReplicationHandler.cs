using ConferenceExample.EventStore;

namespace ConferenceExample.Talk.Persistence.EventHandlers;

/// <summary>
/// Replicates Conference BC events into the Talk BC's event store so the Talk BC has a local
/// copy of cross-context events (used to derive minimal Conference state for talk-submission
/// validation). With the in-memory bus each event is published exactly once, so no extra
/// idempotency handling is needed here.
/// </summary>
public class ConferenceEventReplicationHandler(ITalkEventStore talkEventStore)
{
    public async Task ReplicateEvent(StoredEvent storedEvent)
    {
        await talkEventStore.AppendEvents(
            storedEvent.AggregateId,
            [storedEvent],
            storedEvent.Version - 1
        );
    }
}
