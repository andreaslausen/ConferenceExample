using ConferenceExample.Conference.Domain;

namespace ConferenceExample.Conference.Domain.ConferenceManagement.Events;

public record ConferenceRenamedEvent(Guid AggregateId, DateTimeOffset OccurredAt, string Name)
    : IDomainEvent;
