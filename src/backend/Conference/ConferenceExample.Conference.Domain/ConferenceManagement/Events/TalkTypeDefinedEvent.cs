using ConferenceExample.Conference.Domain.SharedKernel;

namespace ConferenceExample.Conference.Domain.ConferenceManagement.Events;

/// <summary>
/// Domain event that represents a TalkType being defined for a Conference.
/// Contains the complete Conference state (fat event) to ensure read models can be updated.
/// </summary>
public record TalkTypeDefinedEvent(
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
    string Status,
    Guid TalkTypeId,
    string TalkTypeName
) : IDomainEvent;
