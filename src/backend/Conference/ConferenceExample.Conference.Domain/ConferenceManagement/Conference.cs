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
    private readonly List<TalkType> _talkTypes = [];
    private readonly List<Room> _rooms = [];

    public ConferenceId Id { get; private set; } = null!;
    public Text Name { get; private set; } = null!;
    public Time ConferenceTime { get; private set; } = null!;
    public Location Location { get; private set; } = null!;
    public OrganizerId OrganizerId { get; private set; } = null!;
    public ConferenceStatus Status { get; private set; }
    public IReadOnlyList<Talk> Talks => _talks;
    public IReadOnlyList<TalkType> TalkTypes => _talkTypes;
    public IReadOnlyList<Room> Rooms => _rooms;

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
                organizerId.Value,
                ConferenceStatus.Draft.ToString()
            )
        );
        return conference;
    }

    public void Rename(Text name)
    {
        EnsureIsEditable();

        RaiseEvent(new ConferenceRenamedEvent(Id.Value, DateTimeOffset.UtcNow, name.Value));
    }

    public void ChangeStatus(ConferenceStatus newStatus)
    {
        ValidateStatusTransition(Status, newStatus);

        RaiseEvent(
            new ConferenceStatusChangedEvent(Id.Value, DateTimeOffset.UtcNow, newStatus.ToString())
        );
    }

    private void ValidateStatusTransition(
        ConferenceStatus currentStatus,
        ConferenceStatus newStatus
    )
    {
        // No change is always allowed
        if (currentStatus == newStatus)
        {
            return;
        }

        // Forward transitions
        if (newStatus > currentStatus)
        {
            // Draft -> CallForSpeakers requires talk types
            if (
                currentStatus == ConferenceStatus.Draft
                && newStatus == ConferenceStatus.CallForSpeakers
            )
            {
                if (!_talkTypes.Any())
                {
                    throw new InvalidOperationException(
                        "Conference cannot be changed to 'CallForSpeakers' status without defined talk types. Please define at least one talk type first."
                    );
                }
            }

            // Any status -> ProgramPublished requires accepted talks with room and slot
            if (newStatus == ConferenceStatus.ProgramPublished)
            {
                var acceptedTalks = _talks.Where(t => t.Status == TalkStatus.Accepted).ToList();

                if (!acceptedTalks.Any())
                {
                    throw new InvalidOperationException(
                        "Conference cannot be changed to 'ProgramPublished' status without at least one accepted talk."
                    );
                }

                var unscheduledTalks = acceptedTalks
                    .Where(t => t.Slot == null || t.Room == null)
                    .ToList();

                if (unscheduledTalks.Any())
                {
                    throw new InvalidOperationException(
                        $"Conference cannot be changed to 'ProgramPublished' status. All accepted talks must have a room and time slot assigned. {unscheduledTalks.Count} talk(s) are not fully scheduled."
                    );
                }
            }
        }
        // Backward transitions (rollback)
        else
        {
            // ProgramPublished cannot be rolled back
            if (currentStatus == ConferenceStatus.ProgramPublished)
            {
                throw new InvalidOperationException(
                    "Conference status cannot be changed back from 'ProgramPublished'. The program has been published and cannot be unpublished."
                );
            }

            // CallForSpeakers -> Draft only if no talks submitted
            if (
                currentStatus == ConferenceStatus.CallForSpeakers
                && newStatus == ConferenceStatus.Draft
            )
            {
                if (_talks.Any())
                {
                    throw new InvalidOperationException(
                        "Conference status cannot be changed back to 'Draft' when talks have already been submitted. Consider closing the Call for Speakers instead."
                    );
                }
            }
        }
    }

    public void UpdateDetails(Text name, Time conferenceTime, Location location)
    {
        EnsureIsEditable();

        RaiseEvent(
            new ConferenceDetailsUpdatedEvent(
                Id.Value,
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

    public void AddRoom(RoomId roomId, Text name)
    {
        if (_rooms.Any(r => r.Name.Value.Equals(name.Value, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"A room with the name '{name.Value}' already exists for this conference."
            );
        }

        RaiseEvent(
            new RoomAddedEvent(Id.Value, DateTimeOffset.UtcNow, roomId.Value, name.Value)
        );
    }

    public void RemoveRoom(RoomId roomId)
    {
        if (!_rooms.Any(r => r.Id == roomId))
        {
            throw new InvalidOperationException(
                $"Room with id '{roomId.Value}' does not exist for this conference."
            );
        }

        RaiseEvent(new RoomRemovedEvent(Id.Value, DateTimeOffset.UtcNow, roomId.Value));
    }

    public void DefineTalkType(TalkTypeId talkTypeId, Text name, int durationInMinutes)
    {
        EnsureIsEditable();

        if (
            _talkTypes.Any(tt =>
                tt.Name.Value.Equals(name.Value, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            throw new InvalidOperationException(
                $"A talk type with the name '{name.Value}' already exists for this conference."
            );
        }

        RaiseEvent(
            new TalkTypeDefinedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                talkTypeId.Value,
                name.Value,
                durationInMinutes
            )
        );
    }

    public void RemoveTalkType(TalkTypeId talkTypeId)
    {
        EnsureIsEditable();

        if (!_talkTypes.Any(tt => tt.Id == talkTypeId))
        {
            throw new InvalidOperationException(
                $"Talk type with id '{talkTypeId.Value}' does not exist for this conference."
            );
        }

        RaiseEvent(new TalkTypeRemovedEvent(Id.Value, DateTimeOffset.UtcNow, talkTypeId.Value));
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
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                break;
            case ConferenceRenamedEvent e:
                Name = new Text(e.Name);
                break;
            case ConferenceStatusChangedEvent e:
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                break;
            case ConferenceDetailsUpdatedEvent e:
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
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
                FindTalk(e.TalkId).Schedule(new Time(e.TalkStart, e.TalkEnd));
                break;
            case TalkAssignedToRoomEvent e:
                FindTalk(e.TalkId)
                    .AssignRoom(new RoomId(new GuidV7(e.RoomId)), new Text(e.RoomName));
                break;
            case RoomAddedEvent e:
                _rooms.Add(new Room(new RoomId(new GuidV7(e.RoomId)), new Text(e.RoomName)));
                break;
            case RoomRemovedEvent e:
                var roomToRemove = _rooms.First(r => r.Id.Value == (GuidV7)e.RoomId);
                _rooms.Remove(roomToRemove);
                break;
            case TalkTypeDefinedEvent e:
                _talkTypes.Add(
                    new TalkType(
                        new TalkTypeId(new GuidV7(e.TalkTypeId)),
                        new Text(e.TalkTypeName),
                        e.TalkTypeDurationInMinutes
                    )
                );
                break;
            case TalkTypeRemovedEvent e:
                var talkTypeToRemove = FindTalkType(e.TalkTypeId);
                _talkTypes.Remove(talkTypeToRemove);
                break;
        }
    }

    private void EnsureIsEditable()
    {
        if (Status >= ConferenceStatus.CallForSpeakers)
        {
            throw new InvalidOperationException(
                $"Conference cannot be edited when status is '{Status}'. Only conferences in 'Draft' status can be edited."
            );
        }
    }

    private Talk FindTalk(Guid talkId) => _talks.First(s => s.Id.Value == (GuidV7)talkId);

    private TalkType FindTalkType(Guid talkTypeId) =>
        _talkTypes.First(tt => tt.Id.Value == (GuidV7)talkTypeId);
}
