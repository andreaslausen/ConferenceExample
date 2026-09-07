using ConferenceExample.Speaker.Domain.SharedKernel;

namespace ConferenceExample.Speaker.Domain.SpeakerManagement.Events;

/// <summary>
/// Carries the owning <c>UserId</c> so other bounded contexts can resolve the current account to
/// its speaker profile without the Speaker BC exposing a synchronous lookup across the boundary.
/// </summary>
public record SpeakerProfileCreatedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    Guid UserId,
    string FirstName,
    string LastName,
    string Biography
) : IDomainEvent;
