namespace ConferenceExample.Conference.Application.ScheduleTalk;

public record ScheduleTalkCommand(
    Guid ConferenceId,
    Guid TalkId,
    DateTimeOffset Start,
    DateTimeOffset End
);
