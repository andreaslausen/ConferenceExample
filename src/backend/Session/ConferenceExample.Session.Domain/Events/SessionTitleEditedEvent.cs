namespace ConferenceExample.Session.Domain.Events;

public record SessionTitleEditedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Title)
    : IDomainEvent;
