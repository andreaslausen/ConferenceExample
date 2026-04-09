namespace ConferenceExample.Talk.Domain.Events;

public record TalkAbstractEditedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Abstract)
    : IDomainEvent;
