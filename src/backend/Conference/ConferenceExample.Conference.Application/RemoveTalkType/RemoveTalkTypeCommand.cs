namespace ConferenceExample.Conference.Application.RemoveTalkType;

public record RemoveTalkTypeCommand(Guid ConferenceId, Guid TalkTypeId);
