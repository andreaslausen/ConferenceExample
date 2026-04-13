namespace ConferenceExample.Talk.Application.SubmitTalk;

public interface ISubmitTalkCommandHandler
{
    Task Handle(SubmitTalkCommand command);
}
