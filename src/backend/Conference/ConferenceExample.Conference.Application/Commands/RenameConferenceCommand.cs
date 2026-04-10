namespace ConferenceExample.Conference.Application.Commands;

public record RenameConferenceCommand(Guid Id, string Name);
