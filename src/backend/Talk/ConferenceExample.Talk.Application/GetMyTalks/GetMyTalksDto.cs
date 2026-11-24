namespace ConferenceExample.Talk.Application.GetMyTalks;

public record GetMyTalksDto(
    Guid Id,
    string Title,
    string Abstract,
    IReadOnlyList<string> Tags,
    int SubmissionCount
);
