namespace ConferenceExample.Conference.Domain.Events;

public record SessionSubmittedToConferenceEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid SessionId
) : IDomainEvent;
