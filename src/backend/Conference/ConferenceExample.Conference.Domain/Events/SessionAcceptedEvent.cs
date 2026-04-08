namespace ConferenceExample.Conference.Domain.Events;

public record SessionAcceptedEvent(Guid AggregateId, DateTimeOffset OccurredAt, Guid SessionId)
    : IDomainEvent;
