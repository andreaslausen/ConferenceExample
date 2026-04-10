namespace ConferenceExample.Conference.Application.Commands;

public interface IRenameConferenceCommandHandler
{
    Task Handle(RenameConferenceCommand command);
}
