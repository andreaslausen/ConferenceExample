namespace ConferenceExample.Talk.Application.CreateTalk;

public interface ICreateTalkCommandHandler
{
    Task<Guid> Handle(CreateTalkCommand command);
}
