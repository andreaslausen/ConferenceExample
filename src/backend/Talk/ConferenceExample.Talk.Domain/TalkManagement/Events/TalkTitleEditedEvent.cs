using ConferenceExample.Talk.Domain;

namespace ConferenceExample.Talk.Domain.TalkManagement.Events;

public record TalkTitleEditedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Title)
    : IDomainEvent;
