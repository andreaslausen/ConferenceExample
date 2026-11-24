using ConferenceExample.Conference.Domain;

namespace ConferenceExample.Conference.Domain.TalkManagement.Events;

public record TalkAcceptedEvent(Guid AggregateId, DateTimeOffset OccurredAt, Guid TalkId)
    : IDomainEvent;
