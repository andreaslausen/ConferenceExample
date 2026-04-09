namespace ConferenceExample.Conference.Domain.Events;

public record TalkAssignedToRoomEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid TalkId,
    Guid RoomId,
    string RoomName
) : IDomainEvent;
