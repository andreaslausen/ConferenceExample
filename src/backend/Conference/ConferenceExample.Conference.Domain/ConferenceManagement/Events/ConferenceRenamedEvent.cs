using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.ConferenceManagement.Events;

/// <summary>
/// Domain event that contains the complete state of a Conference aggregate when renamed.
/// Used both for domain logic and for updating Read Models across bounded contexts.
/// </summary>
public record ConferenceRenamedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    long Version,
    string Name,
    DateTimeOffset Start,
    DateTimeOffset End,
    string LocationName,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    Guid OrganizerId,
    string Status
) : IDomainEvent;
