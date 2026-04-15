using ConferenceExample.Conference.Domain.ConferenceManagement.Events;
using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceExample.Conference.Domain.TalkManagement.Events;

namespace ConferenceExample.Conference.Domain.ConferenceManagement;

public class Conference : AggregateRoot
{
    private readonly List<Talk> _talks = [];

    public ConferenceId Id { get; private set; } = null!;
    public Text Name { get; private set; } = null!;
    public Time ConferenceTime { get; private set; } = null!;
    public Location Location { get; private set; } = null!;
    public OrganizerId OrganizerId { get; private set; } = null!;
    public ConferenceStatus Status { get; private set; }
    public IReadOnlyList<Talk> Talks => _talks;

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
        Location location,
        OrganizerId organizerId
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
                location.Address.Country,
                organizerId.Value
            )
        );
        return conference;
    }

    public void Rename(Text name)
    {
        RaiseEvent(new ConferenceRenamedEvent(Id.Value, DateTimeOffset.UtcNow, name.Value));
    }

    public void ChangeStatus(ConferenceStatus newStatus)
    {
        RaiseEvent(new ConferenceStatusChangedEvent(Id.Value, DateTimeOffset.UtcNow, newStatus));
    }

    public void SubmitTalk(TalkId talkId)
    {
        RaiseEvent(
            new TalkSubmittedToConferenceEvent(Id.Value, DateTimeOffset.UtcNow, talkId.Value)
        );
    }

    public void AcceptTalk(TalkId talkId)
    {
        RaiseEvent(new TalkAcceptedEvent(Id.Value, DateTimeOffset.UtcNow, talkId.Value));
    }

    public void RejectTalk(TalkId talkId)
    {
        RaiseEvent(new TalkRejectedEvent(Id.Value, DateTimeOffset.UtcNow, talkId.Value));
    }

    public void ScheduleTalk(TalkId talkId, Time slot)
    {
        RaiseEvent(
            new TalkScheduledEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                talkId.Value,
                slot.Start,
                slot.End
            )
        );
    }

    public void AssignTalkToRoom(TalkId talkId, Room room)
    {
        RaiseEvent(
            new TalkAssignedToRoomEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                talkId.Value,
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
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = ConferenceStatus.Draft;
                break;
            case ConferenceRenamedEvent e:
                Name = new Text(e.Name);
                break;
            case ConferenceStatusChangedEvent e:
                Status = e.NewStatus;
                break;
            case TalkSubmittedToConferenceEvent e:
                _talks.Add(new Talk(new TalkId(new GuidV7(e.TalkId))));
                break;
            case TalkAcceptedEvent e:
                FindTalk(e.TalkId).Accept();
                break;
            case TalkRejectedEvent e:
                FindTalk(e.TalkId).Reject();
                break;
            case TalkScheduledEvent e:
                FindTalk(e.TalkId).Schedule(new Time(e.Start, e.End));
                break;
            case TalkAssignedToRoomEvent e:
                FindTalk(e.TalkId)
                    .AssignRoom(new RoomId(new GuidV7(e.RoomId)), new Text(e.RoomName));
                break;
        }
    }

    private Talk FindTalk(Guid talkId) => _talks.First(s => s.Id.Value == (GuidV7)talkId);
}
