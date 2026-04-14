namespace ConferenceExample.Talk.Application.EditTalk;

public interface IEditTalkCommandHandler
{
    Task Handle(EditTalkCommand command);
}
