namespace ConferenceExample.Conference.Application.RejectTalk;

public record RejectTalkCommand(Guid ConferenceId, Guid TalkId);
