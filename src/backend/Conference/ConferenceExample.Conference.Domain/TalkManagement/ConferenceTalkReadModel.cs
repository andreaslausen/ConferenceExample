namespace ConferenceExample.Conference.Domain.TalkManagement;

public record ConferenceTalkReadModel(
    Guid Id,
    string Title,
    string Abstract,
    Guid SpeakerId,
    string Status,
    IReadOnlyList<string> Tags,
    Guid TalkTypeId
);
