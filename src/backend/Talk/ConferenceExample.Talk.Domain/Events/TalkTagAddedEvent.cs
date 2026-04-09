namespace ConferenceExample.Talk.Domain.Events;

public record TalkTagAddedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Tag)
    : IDomainEvent;
