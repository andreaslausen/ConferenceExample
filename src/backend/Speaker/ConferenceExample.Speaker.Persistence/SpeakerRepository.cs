using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Speaker.Domain.SharedKernel;
using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Speaker.Domain.SpeakerManagement;
using ConferenceExample.Speaker.Domain.SpeakerManagement.Events;

namespace ConferenceExample.Speaker.Persistence;

public class SpeakerRepository(ISpeakerEventStore eventStore) : ISpeakerRepository
{
    private static readonly Dictionary<string, Type> EventTypeMap = new()
    {
        [nameof(SpeakerProfileCreatedEvent)] = typeof(SpeakerProfileCreatedEvent),
        [nameof(SpeakerProfileUpdatedEvent)] = typeof(SpeakerProfileUpdatedEvent),
    };

    public async Task<Domain.SpeakerManagement.Speaker> GetById(SpeakerId speakerId)
    {
        var storedEvents = await eventStore.GetEvents(speakerId.Value);

        if (storedEvents.Count == 0)
        {
            throw new NotFoundException($"Speaker with id {speakerId.Value} not found.");
        }

        var domainEvents = storedEvents.Select(Deserialize).ToList();
        return Domain.SpeakerManagement.Speaker.LoadFromHistory(domainEvents);
    }

    public async Task Save(Domain.SpeakerManagement.Speaker speaker)
    {
        var uncommittedEvents = speaker.GetUncommittedEvents();

        if (uncommittedEvents.Count == 0)
        {
            return;
        }

        var storedEvents = uncommittedEvents
            .Select(
                (e, i) =>
                    new StoredEvent(
                        GuidV7.NewGuid().Value,
                        e.AggregateId,
                        e.GetType().Name,
                        JsonSerializer.Serialize(e, e.GetType()),
                        e.OccurredAt,
                        speaker.Version + i + 1
                    )
            )
            .ToList();

        await eventStore.AppendEvents(
            uncommittedEvents[0].AggregateId,
            storedEvents,
            speaker.Version
        );

        speaker.ClearUncommittedEvents();
    }

    private static IDomainEvent Deserialize(StoredEvent storedEvent)
    {
        if (!EventTypeMap.TryGetValue(storedEvent.EventType, out var type))
        {
            throw new InvalidOperationException($"Unknown event type: {storedEvent.EventType}");
        }

        return (IDomainEvent)(
            JsonSerializer.Deserialize(storedEvent.Payload, type)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize event: {storedEvent.EventType}"
            )
        );
    }
}
