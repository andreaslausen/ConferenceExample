namespace ConferenceExample.Conference.Application.AcceptTalk;

public interface IAcceptTalkCommandHandler
{
    Task Handle(AcceptTalkCommand command);
}
