namespace ConferenceExample.Talk.Application.SubmitTalkToConference;

public interface ISubmitTalkToConferenceCommandHandler
{
    Task Handle(SubmitTalkToConferenceCommand command);
}
