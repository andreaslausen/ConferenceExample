namespace ConferenceExample.Talk.Application.GetTalkById;

public record GetTalkByIdDto(
    Guid Id,
    string Title,
    string Abstract,
    Guid SpeakerId,
    IReadOnlyList<string> Tags,
    int SubmissionCount
);
