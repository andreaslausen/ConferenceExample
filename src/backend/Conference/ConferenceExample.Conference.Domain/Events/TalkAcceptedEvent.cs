namespace ConferenceExample.Conference.Domain.Events;

public record TalkAcceptedEvent(Guid AggregateId, DateTimeOffset OccurredAt, Guid TalkId)
    : IDomainEvent;
