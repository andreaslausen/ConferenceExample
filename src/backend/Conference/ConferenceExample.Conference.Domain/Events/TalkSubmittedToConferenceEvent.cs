namespace ConferenceExample.Conference.Domain.Events;

public record TalkSubmittedToConferenceEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid TalkId
) : IDomainEvent;
