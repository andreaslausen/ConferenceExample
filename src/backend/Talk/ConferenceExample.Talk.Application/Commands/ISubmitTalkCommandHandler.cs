namespace ConferenceExample.Talk.Application.Commands;

public interface ISubmitTalkCommandHandler
{
    Task Handle(SubmitTalkCommand command);
}
