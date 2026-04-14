namespace ConferenceExample.Talk.Application.SubmitTalk;

public record SubmitTalkCommand(
    string Title,
    string Abstract,
    Guid ConferenceId,
    List<string> Tags,
    Guid TalkTypeId
);
