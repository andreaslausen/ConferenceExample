using ConferenceExample.Talk.Domain.SharedKernel;

namespace ConferenceExample.Talk.Domain.TalkManagement.Events;

public record TalkSubmittedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string Title,
    string Abstract,
    Guid SpeakerId,
    string SpeakerFirstName,
    string SpeakerLastName,
    string SpeakerBiography,
    List<string> Tags,
    Guid TalkTypeId,
    Guid ConferenceId,
    string Status
) : IDomainEvent;
