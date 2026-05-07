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

        var latestEvent = storedEvents.OrderByDescending(e => e.Version).First();
        var payload = JsonSerializer.Deserialize<ConferenceEventPayload>(latestEvent.Payload);

        if (payload is null)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize Conference event for {conferenceId.Value}"
            );
        }

        return Conference.FromEvents(conferenceId, payload.Status);
    }

    private record ConferenceEventPayload(
        Guid AggregateId,
        DateTimeOffset OccurredAt,
        long Version,
        string Name,
        DateTimeOffset Start,
        DateTimeOffset End,
        string LocationName,
        string Street,
        string City,
        string State,
        string PostalCode,
        string Country,
        Guid OrganizerId,
        string Status
    );
}
