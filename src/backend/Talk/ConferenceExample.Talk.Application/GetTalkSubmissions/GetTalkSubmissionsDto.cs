namespace ConferenceExample.Talk.Application.GetTalkSubmissions;

public record GetTalkSubmissionsDto(
    Guid ConferenceId,
    string ConferenceName,
    Guid TalkTypeId,
    string Status,
    string? Reason,
    DateTimeOffset SubmittedAt,
    string Title,
    string Abstract,
    IReadOnlyList<string> Tags
);
