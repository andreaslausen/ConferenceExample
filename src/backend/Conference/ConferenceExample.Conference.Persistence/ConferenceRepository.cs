using System.Text.Json;
using ConferenceExample.Conference.Domain;
using ConferenceExample.Conference.Domain.Events;
using ConferenceExample.Conference.Domain.Repositories;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;
using ConferenceExample.EventStore;
using GuidV7 = ConferenceExample.Conference.Domain.ValueObjects.Ids.GuidV7;

namespace ConferenceExample.Conference.Persistence;

public class ConferenceRepository(IEventStore eventStore, IEventBus eventBus)
    : IConferenceRepository
{
    private static readonly Dictionary<string, Type> EventTypeMap = new()
    {
        [nameof(ConferenceCreatedEvent)] = typeof(ConferenceCreatedEvent),
        [nameof(ConferenceRenamedEvent)] = typeof(ConferenceRenamedEvent),
        [nameof(SessionSubmittedToConferenceEvent)] = typeof(SessionSubmittedToConferenceEvent),
        [nameof(SessionAcceptedEvent)] = typeof(SessionAcceptedEvent),
        [nameof(SessionRejectedEvent)] = typeof(SessionRejectedEvent),
        [nameof(SessionScheduledEvent)] = typeof(SessionScheduledEvent),
        [nameof(SessionAssignedToRoomEvent)] = typeof(SessionAssignedToRoomEvent),
    };

    public async Task<Domain.Conference> GetById(ConferenceId id)
    {
        var storedEvents = await eventStore.GetEvents(id.Value);

        if (storedEvents.Count == 0)
        {
            throw new InvalidOperationException($"Conference with id {id.Value} not found.");
        }

        var domainEvents = storedEvents.Select(Deserialize).ToList();
        return Domain.Conference.LoadFromHistory(domainEvents);
    }

    public async Task Save(Domain.Conference conference)
    {
        var uncommittedEvents = conference.GetUncommittedEvents();

        if (uncommittedEvents.Count == 0)
        {
            return;
        }

        var storedEvents = uncommittedEvents
            .Select(
                (e, i) =>
                    new StoredEvent(
                        GuidV7.NewGuid(),
                        e.AggregateId,
                        e.GetType().Name,
                        JsonSerializer.Serialize(e, e.GetType()),
                        e.OccurredAt,
                        conference.Version + i + 1
                    )
            )
            .ToList();

        await eventStore.AppendEvents(
            uncommittedEvents[0].AggregateId,
            storedEvents,
            conference.Version
        );

        await eventBus.Publish(storedEvents);

        conference.ClearUncommittedEvents();
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
