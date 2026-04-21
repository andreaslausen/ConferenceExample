namespace ConferenceExample.Conference.Application.GetConferenceTalks;

public record GetConferenceTalksDto(
    Guid Id,
    string Title,
    string Abstract,
    Guid SpeakerId,
    string SpeakerName,
    string Status,
    IReadOnlyList<string> Tags,
    Guid TalkTypeId
);
