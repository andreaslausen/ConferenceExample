using System.Text.Json;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.ConferenceManagement.Events;
using ConferenceExample.Conference.Domain.SharedKernel;
using ConferenceExample.Conference.Domain.TalkManagement.Events;
using ConferenceExample.EventStore;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;
using GuidV7 = ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids.GuidV7;

namespace ConferenceExample.Conference.Persistence;

public class ConferenceRepository(IEventStore eventStore) : IConferenceRepository
{
    private static readonly Dictionary<string, Type> EventTypeMap = new()
    {
        [nameof(ConferenceCreatedEvent)] = typeof(ConferenceCreatedEvent),
        [nameof(ConferenceRenamedEvent)] = typeof(ConferenceRenamedEvent),
        [nameof(TalkSubmittedToConferenceEvent)] = typeof(TalkSubmittedToConferenceEvent),
        [nameof(TalkAcceptedEvent)] = typeof(TalkAcceptedEvent),
        [nameof(TalkRejectedEvent)] = typeof(TalkRejectedEvent),
        [nameof(TalkScheduledEvent)] = typeof(TalkScheduledEvent),
        [nameof(TalkAssignedToRoomEvent)] = typeof(TalkAssignedToRoomEvent),
    };

    public async Task<ConferenceAggregate> GetById(ConferenceId id)
    {
        var storedEvents = await eventStore.GetEvents(id.Value);

        if (storedEvents.Count == 0)
        {
            throw new InvalidOperationException($"Conference with id {id.Value} not found.");
        }

        var domainEvents = storedEvents.Select(Deserialize).ToList();
        return ConferenceAggregate.LoadFromHistory(domainEvents);
    }

    public async Task<IReadOnlyList<ConferenceAggregate>> GetAll()
    {
        var allEvents = await eventStore.GetAllEvents();

        // Group events by aggregate ID
        var eventsByAggregate = allEvents.GroupBy(e => e.AggregateId).ToList();

        var conferences = new List<ConferenceAggregate>();

        foreach (var aggregateEvents in eventsByAggregate)
        {
            // Only process events that belong to Conference aggregates (check if first event is ConferenceCreatedEvent)
            var firstEvent = aggregateEvents.OrderBy(e => e.Version).FirstOrDefault();
            if (firstEvent?.EventType == nameof(ConferenceCreatedEvent))
            {
                var domainEvents = aggregateEvents
                    .OrderBy(e => e.Version)
                    .Select(Deserialize)
                    .ToList();

                conferences.Add(ConferenceAggregate.LoadFromHistory(domainEvents));
            }
        }

        return conferences;
    }

    public async Task Save(ConferenceAggregate conference)
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
