namespace ConferenceExample.Conference.Application.UpdateConferenceDetails;

public interface IUpdateConferenceDetailsCommandHandler
{
    Task Handle(UpdateConferenceDetailsCommand command);
}
