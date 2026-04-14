namespace ConferenceExample.Conference.Application.RejectTalk;

public interface IRejectTalkCommandHandler
{
    Task Handle(RejectTalkCommand command);
}
