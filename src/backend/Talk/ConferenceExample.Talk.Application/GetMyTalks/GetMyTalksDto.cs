namespace ConferenceExample.Talk.Application.GetMyTalks;

public record GetMyTalksDto(
    Guid Id,
    string Title,
    string Abstract,
    Guid ConferenceId,
    string Status,
    IReadOnlyList<string> Tags
);
