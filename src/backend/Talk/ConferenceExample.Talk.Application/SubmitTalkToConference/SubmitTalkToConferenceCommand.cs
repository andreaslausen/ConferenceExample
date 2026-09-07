namespace ConferenceExample.Talk.Application.SubmitTalkToConference;

public record SubmitTalkToConferenceCommand(Guid TalkId, Guid ConferenceId, Guid TalkTypeId);
