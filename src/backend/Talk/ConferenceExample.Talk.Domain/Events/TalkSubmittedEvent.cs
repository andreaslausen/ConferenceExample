namespace ConferenceExample.Talk.Domain.Events;

public record TalkSubmittedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string Title,
    string Abstract,
    Guid SpeakerId,
    List<string> Tags,
    Guid TalkTypeId,
    Guid ConferenceId
) : IDomainEvent;
