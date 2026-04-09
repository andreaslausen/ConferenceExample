using ConferenceExample.Conference.Domain.Entities;
using ConferenceExample.Conference.Domain.Events;
using ConferenceExample.Conference.Domain.ValueObjects;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Domain;

public class Conference : AggregateRoot
{
    private readonly List<Session> _sessions = [];

    public ConferenceId Id { get; private set; } = null!;
    public Text Name { get; private set; } = null!;
    public Time ConferenceTime { get; private set; } = null!;
    public Location Location { get; private set; } = null!;
    public IReadOnlyList<Session> Sessions => _sessions;

    private Conference() { }

    public static Conference LoadFromHistory(IEnumerable<IDomainEvent> events)
    {
        var conference = new Conference();
        conference.ReplayEvents(events);
        return conference;
    }

    public static Conference Create(
        ConferenceId id,
        Text name,
        Time conferenceTime,
        Location location
    )
    {
        var conference = new Conference();
        conference.RaiseEvent(
            new ConferenceCreatedEvent(
                id.Value,
                DateTimeOffset.UtcNow,
                name.Value,
                conferenceTime.Start,
                conferenceTime.End,
                location.Name.Value,
                location.Address.Street,
                location.Address.City,
                location.Address.State,
                location.Address.PostalCode,
                location.Address.Country
            )
        );
        return conference;
    }

    public void Rename(Text name)
    {
        RaiseEvent(new ConferenceRenamedEvent(Id.Value, DateTimeOffset.UtcNow, name.Value));
    }

    public void SubmitSession(SessionId sessionId)
    {
        RaiseEvent(
            new SessionSubmittedToConferenceEvent(Id.Value, DateTimeOffset.UtcNow, sessionId.Value)
        );
    }

    public void AcceptSession(SessionId sessionId)
    {
        RaiseEvent(new SessionAcceptedEvent(Id.Value, DateTimeOffset.UtcNow, sessionId.Value));
    }

    public void RejectSession(SessionId sessionId)
    {
        RaiseEvent(new SessionRejectedEvent(Id.Value, DateTimeOffset.UtcNow, sessionId.Value));
    }

    public void ScheduleSession(SessionId sessionId, Time slot)
    {
        RaiseEvent(
            new SessionScheduledEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                sessionId.Value,
                slot.Start,
                slot.End
            )
        );
    }

    public void AssignSessionToRoom(SessionId sessionId, Room room)
    {
        RaiseEvent(
            new SessionAssignedToRoomEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                sessionId.Value,
                room.Id.Value,
                room.Name.Value
            )
        );
    }

    protected override void ApplyEvent(IDomainEvent @event)
    {
        switch (@event)
        {
            case ConferenceCreatedEvent e:
                Id = new ConferenceId(new GuidV7(e.AggregateId));
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                break;
            case ConferenceRenamedEvent e:
                Name = new Text(e.Name);
                break;
            case SessionSubmittedToConferenceEvent e:
                _sessions.Add(new Session(new SessionId(new GuidV7(e.SessionId))));
                break;
            case SessionAcceptedEvent e:
                FindSession(e.SessionId).Accept();
                break;
            case SessionRejectedEvent e:
                FindSession(e.SessionId).Reject();
                break;
            case SessionScheduledEvent e:
                FindSession(e.SessionId).Schedule(new Time(e.Start, e.End));
                break;
            case SessionAssignedToRoomEvent e:
                FindSession(e.SessionId)
                    .AssignRoom(new RoomId(new GuidV7(e.RoomId)), new Text(e.RoomName));
                break;
        }
    }

    private Session FindSession(Guid sessionId) =>
        _sessions.First(s => s.Id.Value == (GuidV7)sessionId);
}
