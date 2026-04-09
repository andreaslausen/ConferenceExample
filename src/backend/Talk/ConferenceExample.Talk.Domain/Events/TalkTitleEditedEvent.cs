namespace ConferenceExample.Talk.Domain.Events;

public record TalkTitleEditedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Title)
    : IDomainEvent;
