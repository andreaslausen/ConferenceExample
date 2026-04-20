namespace ConferenceExample.Talk.Application.GetTalkById;

public record GetTalkByIdDto(
    Guid Id,
    string Title,
    string Abstract,
    Guid ConferenceId,
    string Status,
    IReadOnlyList<string> Tags
);
