using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Talk.Domain;
using ConferenceExample.Talk.Domain.Events;
using ConferenceExample.Talk.Domain.Repositories;
using ConferenceExample.Talk.Domain.ValueObjects;
using ConferenceExample.Talk.Domain.ValueObjects.Ids;

namespace ConferenceExample.Talk.Persistence;

public class TalkRepository(IEventStore eventStore, IEventBus eventBus) : ITalkRepository
{
    private static readonly Dictionary<string, Type> EventTypeMap = new()
    {
        [nameof(TalkSubmittedEvent)] = typeof(TalkSubmittedEvent),
        [nameof(TalkTitleEditedEvent)] = typeof(TalkTitleEditedEvent),
        [nameof(TalkAbstractEditedEvent)] = typeof(TalkAbstractEditedEvent),
        [nameof(TalkTagAddedEvent)] = typeof(TalkTagAddedEvent),
        [nameof(TalkTagRemovedEvent)] = typeof(TalkTagRemovedEvent),
    };

    public async Task<Domain.Entities.Talk> GetById(TalkId id)
    {
        var storedEvents = await eventStore.GetEvents(id.Value);

        if (storedEvents.Count == 0)
        {
            throw new InvalidOperationException($"Talk with id {id.Value} not found.");
        }

        var domainEvents = storedEvents.Select(Deserialize).ToList();
        return Domain.Entities.Talk.LoadFromHistory(domainEvents);
    }

    public async Task<IReadOnlyList<Domain.Entities.Talk>> GetTalks(ConferenceId conferenceId)
    {
        var allEvents = await eventStore.GetAllEvents();

        var sessionEvents = allEvents
            .Where(e => EventTypeMap.ContainsKey(e.EventType))
            .GroupBy(e => e.AggregateId);

        var sessions = new List<Domain.Entities.Talk>();

        foreach (var group in sessionEvents)
        {
            var domainEvents = group.OrderBy(e => e.Version).Select(Deserialize).ToList();
            var talk = Domain.Entities.Talk.LoadFromHistory(domainEvents);

            if (talk.ConferenceId == conferenceId)
            {
                sessions.Add(talk);
            }
        }

        return sessions;
    }

    public async Task Save(Domain.Entities.Talk talk)
    {
        var uncommittedEvents = talk.GetUncommittedEvents();

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
                        talk.Version + i + 1
                    )
            )
            .ToList();

        await eventStore.AppendEvents(
            uncommittedEvents[0].AggregateId,
            storedEvents,
            talk.Version
        );

        await eventBus.Publish(storedEvents);

        talk.ClearUncommittedEvents();
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
