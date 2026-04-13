namespace ConferenceExample.Conference.Application.RenameConference;

public interface IRenameConferenceCommandHandler
{
    Task Handle(RenameConferenceCommand command);
}
