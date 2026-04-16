using ConferenceExample.Talk.Domain.SharedKernel;

namespace ConferenceExample.Talk.Domain.TalkManagement.Events;

/// <summary>
/// Domain event that contains the complete state of a Talk aggregate when submitted.
/// Used both for domain logic and for updating Read Models across bounded contexts.
/// </summary>
public record TalkSubmittedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    long Version,
    string Title,
    string Abstract,
    Guid SpeakerId,
    List<string> Tags,
    Guid TalkTypeId,
    Guid ConferenceId,
    string Status
) : IDomainEvent;
