namespace ConferenceExample.Session.Domain.Events;

public record SessionTagRemovedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Tag)
    : IDomainEvent;
