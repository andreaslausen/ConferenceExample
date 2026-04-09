namespace ConferenceExample.Talk.Application.Commands;

public record SubmitTalkCommand(
    string Title,
    string Abstract,
    Guid ConferenceId,
    Guid SpeakerId,
    List<string> Tags,
    Guid TalkTypeId
);
