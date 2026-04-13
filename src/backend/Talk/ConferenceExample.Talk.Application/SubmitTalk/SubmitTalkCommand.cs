namespace ConferenceExample.Talk.Application.SubmitTalk;

public record SubmitTalkCommand(
    string Title,
    string Abstract,
    Guid ConferenceId,
    Guid SpeakerId,
    List<string> Tags,
    Guid TalkTypeId
);
