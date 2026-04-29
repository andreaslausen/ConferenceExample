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
                0, // Version starts at 0
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

        RaiseEvent(
            new ConferenceRenamedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                name.Value,
                ConferenceTime.Start,
                ConferenceTime.End,
                Location.Name.Value,
                Location.Address.Street,
                Location.Address.City,
                Location.Address.State,
                Location.Address.PostalCode,
                Location.Address.Country,
                OrganizerId.Value,
                Status.ToString()
            )
        );
    }

    public void ChangeStatus(ConferenceStatus newStatus)
    {
        if (newStatus == ConferenceStatus.CallForSpeakers && !_talkTypes.Any())
        {
            throw new InvalidOperationException(
                "Conference cannot be changed to 'CallForSpeakers' status without defined talk types. Please define at least one talk type first."
            );
        }

        RaiseEvent(
            new ConferenceStatusChangedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                Name.Value,
                ConferenceTime.Start,
                ConferenceTime.End,
                Location.Name.Value,
                Location.Address.Street,
                Location.Address.City,
                Location.Address.State,
                Location.Address.PostalCode,
                Location.Address.Country,
                OrganizerId.Value,
                newStatus.ToString()
            )
        );
    }

    public void UpdateDetails(Text name, Time conferenceTime, Location location)
    {
        EnsureIsEditable();

        RaiseEvent(
            new ConferenceDetailsUpdatedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                name.Value,
                conferenceTime.Start,
                conferenceTime.End,
                location.Name.Value,
                location.Address.Street,
                location.Address.City,
                location.Address.State,
                location.Address.PostalCode,
                location.Address.Country,
                OrganizerId.Value,
                Status.ToString()
            )
        );
    }

    public void SubmitTalk(TalkId talkId)
    {
        RaiseEvent(
            new TalkSubmittedToConferenceEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                Name.Value,
                ConferenceTime.Start,
                ConferenceTime.End,
                Location.Name.Value,
                Location.Address.Street,
                Location.Address.City,
                Location.Address.State,
                Location.Address.PostalCode,
                Location.Address.Country,
                OrganizerId.Value,
                Status.ToString(),
                talkId.Value
            )
        );
    }

    public void AcceptTalk(TalkId talkId)
    {
        RaiseEvent(
            new TalkAcceptedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                Name.Value,
                ConferenceTime.Start,
                ConferenceTime.End,
                Location.Name.Value,
                Location.Address.Street,
                Location.Address.City,
                Location.Address.State,
                Location.Address.PostalCode,
                Location.Address.Country,
                OrganizerId.Value,
                Status.ToString(),
                talkId.Value
            )
        );
    }

    public void RejectTalk(TalkId talkId)
    {
        RaiseEvent(
            new TalkRejectedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                Name.Value,
                ConferenceTime.Start,
                ConferenceTime.End,
                Location.Name.Value,
                Location.Address.Street,
                Location.Address.City,
                Location.Address.State,
                Location.Address.PostalCode,
                Location.Address.Country,
                OrganizerId.Value,
                Status.ToString(),
                talkId.Value
            )
        );
    }

    public void ScheduleTalk(TalkId talkId, Time slot)
    {
        RaiseEvent(
            new TalkScheduledEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                Name.Value,
                ConferenceTime.Start,
                ConferenceTime.End,
                Location.Name.Value,
                Location.Address.Street,
                Location.Address.City,
                Location.Address.State,
                Location.Address.PostalCode,
                Location.Address.Country,
                OrganizerId.Value,
                Status.ToString(),
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
                Version + 1,
                Name.Value,
                ConferenceTime.Start,
                ConferenceTime.End,
                Location.Name.Value,
                Location.Address.Street,
                Location.Address.City,
                Location.Address.State,
                Location.Address.PostalCode,
                Location.Address.Country,
                OrganizerId.Value,
                Status.ToString(),
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
            new RoomAddedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                Name.Value,
                ConferenceTime.Start,
                ConferenceTime.End,
                Location.Name.Value,
                Location.Address.Street,
                Location.Address.City,
                Location.Address.State,
                Location.Address.PostalCode,
                Location.Address.Country,
                OrganizerId.Value,
                Status.ToString(),
                roomId.Value,
                name.Value
            )
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

        RaiseEvent(
            new RoomRemovedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                Name.Value,
                ConferenceTime.Start,
                ConferenceTime.End,
                Location.Name.Value,
                Location.Address.Street,
                Location.Address.City,
                Location.Address.State,
                Location.Address.PostalCode,
                Location.Address.Country,
                OrganizerId.Value,
                Status.ToString(),
                roomId.Value
            )
        );
    }

    public void DefineTalkType(TalkTypeId talkTypeId, Text name, int durationInMinutes)
    {
        // Check if talk type with same name already exists
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
                Version + 1,
                Name.Value,
                ConferenceTime.Start,
                ConferenceTime.End,
                Location.Name.Value,
                Location.Address.Street,
                Location.Address.City,
                Location.Address.State,
                Location.Address.PostalCode,
                Location.Address.Country,
                OrganizerId.Value,
                Status.ToString(),
                talkTypeId.Value,
                name.Value,
                durationInMinutes
            )
        );
    }

    public void RemoveTalkType(TalkTypeId talkTypeId)
    {
        // Check if talk type exists
        if (!_talkTypes.Any(tt => tt.Id == talkTypeId))
        {
            throw new InvalidOperationException(
                $"Talk type with id '{talkTypeId.Value}' does not exist for this conference."
            );
        }

        RaiseEvent(
            new TalkTypeRemovedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                Name.Value,
                ConferenceTime.Start,
                ConferenceTime.End,
                Location.Name.Value,
                Location.Address.Street,
                Location.Address.City,
                Location.Address.State,
                Location.Address.PostalCode,
                Location.Address.Country,
                OrganizerId.Value,
                Status.ToString(),
                talkTypeId.Value
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
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                break;
            case ConferenceRenamedEvent e:
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                break;
            case ConferenceStatusChangedEvent e:
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                break;
            case ConferenceDetailsUpdatedEvent e:
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                break;
            case TalkSubmittedToConferenceEvent e:
                // Reconstruct full Conference state from fat event
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                // Apply event-specific change
                _talks.Add(new Talk(new TalkId(new GuidV7(e.TalkId))));
                break;
            case TalkAcceptedEvent e:
                // Reconstruct full Conference state from fat event
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                // Apply event-specific change
                FindTalk(e.TalkId).Accept();
                break;
            case TalkRejectedEvent e:
                // Reconstruct full Conference state from fat event
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                // Apply event-specific change
                FindTalk(e.TalkId).Reject();
                break;
            case TalkScheduledEvent e:
                // Reconstruct full Conference state from fat event
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.ConferenceStart, e.ConferenceEnd);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                // Apply event-specific change
                FindTalk(e.TalkId).Schedule(new Time(e.TalkStart, e.TalkEnd));
                break;
            case TalkAssignedToRoomEvent e:
                // Reconstruct full Conference state from fat event
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                // Apply event-specific change
                FindTalk(e.TalkId)
                    .AssignRoom(new RoomId(new GuidV7(e.RoomId)), new Text(e.RoomName));
                break;
            case RoomAddedEvent e:
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                _rooms.Add(new Room(new RoomId(new GuidV7(e.RoomId)), new Text(e.RoomName)));
                break;
            case RoomRemovedEvent e:
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                var roomToRemove = _rooms.First(r => r.Id.Value == (GuidV7)e.RoomId);
                _rooms.Remove(roomToRemove);
                break;
            case TalkTypeDefinedEvent e:
                // Reconstruct full Conference state from fat event
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                // Apply event-specific change
                _talkTypes.Add(
                    new TalkType(
                        new TalkTypeId(new GuidV7(e.TalkTypeId)),
                        new Text(e.TalkTypeName),
                        e.TalkTypeDurationInMinutes
                    )
                );
                break;
            case TalkTypeRemovedEvent e:
                // Reconstruct full Conference state from fat event
                Name = new Text(e.Name);
                ConferenceTime = new Time(e.Start, e.End);
                Location = new Location(
                    new Text(e.LocationName),
                    new Address(e.Street, e.City, e.State, e.PostalCode, e.Country)
                );
                OrganizerId = new OrganizerId(new GuidV7(e.OrganizerId));
                Status = Enum.Parse<ConferenceStatus>(e.Status);
                // Apply event-specific change
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
