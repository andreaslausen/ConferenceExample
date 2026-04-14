namespace ConferenceExample.Conference.Application.AcceptTalk;

public record AcceptTalkCommand(Guid ConferenceId, Guid TalkId);
