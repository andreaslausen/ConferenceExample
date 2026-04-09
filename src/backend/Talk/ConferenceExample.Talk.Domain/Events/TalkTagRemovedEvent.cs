namespace ConferenceExample.Talk.Domain.Events;

public record TalkTagRemovedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Tag)
    : IDomainEvent;
