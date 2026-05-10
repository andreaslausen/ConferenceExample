using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.TalkManagement.Events;

public record TalkAssignedToRoomEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid TalkId,
    Guid RoomId,
    string RoomName
) : IDomainEvent;
