namespace ConferenceExample.Conference.Domain.Events;

public record SessionAssignedToRoomEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid SessionId,
    Guid RoomId,
    string RoomName
) : IDomainEvent;
