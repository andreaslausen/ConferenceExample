namespace ConferenceExample.Talk.Application.SubmitTalk;

public interface ISubmitTalkCommandHandler
{
    Task<Guid> Handle(SubmitTalkCommand command);
}
