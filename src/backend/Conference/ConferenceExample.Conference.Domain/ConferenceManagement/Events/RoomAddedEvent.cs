using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.ConferenceManagement.Events;

public record RoomAddedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid RoomId,
    string RoomName
) : IDomainEvent;
