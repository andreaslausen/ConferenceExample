namespace ConferenceExample.Conference.Domain.Events;

public record SessionScheduledEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid SessionId,
    DateTimeOffset Start,
    DateTimeOffset End
) : IDomainEvent;
