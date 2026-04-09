namespace ConferenceExample.Conference.Domain.Events;

public record TalkScheduledEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid TalkId,
    DateTimeOffset Start,
    DateTimeOffset End
) : IDomainEvent;
