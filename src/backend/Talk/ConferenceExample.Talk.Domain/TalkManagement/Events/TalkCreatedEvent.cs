using ConferenceExample.Talk.Domain;

namespace ConferenceExample.Talk.Domain.TalkManagement.Events;

public record TalkCreatedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string Title,
    string Abstract,
    Guid SpeakerId,
    List<string> Tags
) : IDomainEvent;
