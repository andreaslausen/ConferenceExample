using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.ConferenceManagement.Events;

public record RoomRemovedEvent(Guid AggregateId, DateTimeOffset OccurredAt, Guid RoomId)
    : IDomainEvent;
