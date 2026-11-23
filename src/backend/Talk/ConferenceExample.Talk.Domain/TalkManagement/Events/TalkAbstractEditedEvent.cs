using ConferenceExample.Talk.Domain;

namespace ConferenceExample.Talk.Domain.TalkManagement.Events;

public record TalkAbstractEditedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Abstract)
    : IDomainEvent;
