namespace ConferenceExample.Talk.Application.EditTalk;

public record EditTalkCommand(
    Guid TalkId,
    string Title,
    string Abstract,
    IReadOnlyList<string> Tags
);
