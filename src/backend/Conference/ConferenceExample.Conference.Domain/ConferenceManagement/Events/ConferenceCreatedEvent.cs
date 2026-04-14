using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.ConferenceManagement.Events;

public record ConferenceCreatedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string Name,
    DateTimeOffset Start,
    DateTimeOffset End,
    string LocationName,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    Guid OrganizerId
) : IDomainEvent;
