namespace ConferenceExample.Conference.Application.ChangeConferenceStatus;

public interface IChangeConferenceStatusCommandHandler
{
    Task Handle(ChangeConferenceStatusCommand command);
}
