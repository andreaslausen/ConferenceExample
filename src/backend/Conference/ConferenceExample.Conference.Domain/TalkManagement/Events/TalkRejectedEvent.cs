using ConferenceExample.Conference.Domain;

namespace ConferenceExample.Conference.Domain.TalkManagement.Events;

public record TalkRejectedEvent(Guid AggregateId, DateTimeOffset OccurredAt, Guid TalkId)
    : IDomainEvent;
