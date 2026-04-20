namespace ConferenceExample.Conference.Domain.TalkManagement;

public record ConferenceTalkReadModel(
    Guid Id,
    string Title,
    string Abstract,
    Guid SpeakerId,
    string SpeakerFirstName,
    string SpeakerLastName,
    string SpeakerBiography,
    string Status,
    IReadOnlyList<string> Tags,
    Guid TalkTypeId
);
