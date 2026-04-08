namespace ConferenceExample.Conference.Domain.Events;

public record SessionRejectedEvent(Guid AggregateId, DateTimeOffset OccurredAt, Guid SessionId)
    : IDomainEvent;
