using System.Text.Json;
using ConferenceExample.EventStore;
using ConferenceExample.Session.Domain;
using ConferenceExample.Session.Domain.Events;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Persistence;

public class SessionRepository(IEventStore eventStore, IEventBus eventBus) : ISessionRepository
{
    private static readonly Dictionary<string, Type> EventTypeMap = new()
    {
        [nameof(SessionSubmittedEvent)] = typeof(SessionSubmittedEvent),
        [nameof(SessionTitleEditedEvent)] = typeof(SessionTitleEditedEvent),
        [nameof(SessionAbstractEditedEvent)] = typeof(SessionAbstractEditedEvent),
        [nameof(SessionTagAddedEvent)] = typeof(SessionTagAddedEvent),
        [nameof(SessionTagRemovedEvent)] = typeof(SessionTagRemovedEvent),
    };

    public async Task<Domain.Entities.Session> GetById(SessionId id)
    {
        var storedEvents = await eventStore.GetEvents(id.Value);

        if (storedEvents.Count == 0)
        {
            throw new InvalidOperationException($"Session with id {id.Value} not found.");
        }

        var domainEvents = storedEvents.Select(Deserialize).ToList();
        return Domain.Entities.Session.LoadFromHistory(domainEvents);
    }

    public async Task<IReadOnlyList<Domain.Entities.Session>> GetSessions(ConferenceId conferenceId)
    {
        var allEvents = await eventStore.GetAllEvents();

        var sessionEvents = allEvents
            .Where(e => EventTypeMap.ContainsKey(e.EventType))
            .GroupBy(e => e.AggregateId);

        var sessions = new List<Domain.Entities.Session>();

        foreach (var group in sessionEvents)
        {
            var domainEvents = group.OrderBy(e => e.Version).Select(Deserialize).ToList();
            var session = Domain.Entities.Session.LoadFromHistory(domainEvents);

            if (session.ConferenceId == conferenceId)
            {
                sessions.Add(session);
            }
        }

        return sessions;
    }

    public async Task Save(Domain.Entities.Session session)
    {
        var uncommittedEvents = session.GetUncommittedEvents();

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
                        session.Version + i + 1
                    )
            )
            .ToList();

        await eventStore.AppendEvents(
            uncommittedEvents[0].AggregateId,
            storedEvents,
            session.Version
        );

        await eventBus.Publish(storedEvents);

        session.ClearUncommittedEvents();
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
