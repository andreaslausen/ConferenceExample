namespace ConferenceExample.Session.Application.Commands;

public record SubmitSessionCommand(
    string Title,
    string Abstract,
    Guid ConferenceId,
    Guid SpeakerId,
    List<string> Tags,
    Guid SessionTypeId
);
