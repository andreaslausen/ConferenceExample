namespace ConferenceExample.Conference.Domain.Events;

public record ConferenceRenamedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Name)
    : IDomainEvent;
