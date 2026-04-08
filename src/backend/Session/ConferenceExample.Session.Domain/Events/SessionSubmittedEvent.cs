namespace ConferenceExample.Session.Domain.Events;

public record SessionSubmittedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string Title,
    string Abstract,
    Guid SpeakerId,
    List<string> Tags,
    Guid SessionTypeId,
    Guid ConferenceId
) : IDomainEvent;
