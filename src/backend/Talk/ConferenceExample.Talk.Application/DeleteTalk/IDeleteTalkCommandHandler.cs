namespace ConferenceExample.Talk.Application.DeleteTalk;

public interface IDeleteTalkCommandHandler
{
    Task Handle(DeleteTalkCommand command);
}
