using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.ConferenceManagement.Events;

public record ConferenceStatusChangedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string Status
) : IDomainEvent;
