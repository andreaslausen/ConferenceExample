namespace ConferenceExample.Session.Domain.Events;

public record SessionTagAddedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Tag)
    : IDomainEvent;
