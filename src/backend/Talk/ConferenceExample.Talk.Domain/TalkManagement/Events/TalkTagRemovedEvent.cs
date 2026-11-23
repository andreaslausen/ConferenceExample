using ConferenceExample.Talk.Domain;

namespace ConferenceExample.Talk.Domain.TalkManagement.Events;

public record TalkTagRemovedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Tag)
    : IDomainEvent;
