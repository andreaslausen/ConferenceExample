namespace ConferenceExample.Conference.Domain.Events;

public record TalkRejectedEvent(Guid AggregateId, DateTimeOffset OccurredAt, Guid TalkId)
    : IDomainEvent;
