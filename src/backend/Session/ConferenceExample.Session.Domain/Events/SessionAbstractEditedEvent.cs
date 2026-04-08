namespace ConferenceExample.Session.Domain.Events;

public record SessionAbstractEditedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string Abstract
) : IDomainEvent;
