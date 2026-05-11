using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Persistence;

public class ConferenceRepository(ITalkEventStore eventStore) : IConferenceRepository
{
    public async Task<Conference> GetById(ConferenceId conferenceId)
    {
        var aggregateId = conferenceId.Value.Value;
        var storedEvents = await eventStore.GetEvents(aggregateId);

        if (storedEvents.Count == 0)
        {
            throw new InvalidOperationException(
                $"Conference with id {conferenceId.Value} does not exist."
            );
        }

        // Slim events: only ConferenceCreatedEvent and ConferenceStatusChangedEvent carry Status.
        // Pick the latest of those by version to get the current Status.
        var latestStatusEvent = storedEvents
            .Where(e =>
                e.EventType == "ConferenceCreatedEvent"
                || e.EventType == "ConferenceStatusChangedEvent"
            )
            .OrderByDescending(e => e.Version)
            .FirstOrDefault();

        if (latestStatusEvent is null)
        {
            throw new InvalidOperationException(
                $"No status-bearing event found for Conference {conferenceId.Value}."
            );
        }

        var payload = JsonSerializer.Deserialize<StatusPayload>(latestStatusEvent.Payload);
        if (payload is null)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize Conference event for {conferenceId.Value}."
            );
        }

        return Conference.FromEvents(conferenceId, payload.Status);
    }

    private record StatusPayload(string Status);
}
