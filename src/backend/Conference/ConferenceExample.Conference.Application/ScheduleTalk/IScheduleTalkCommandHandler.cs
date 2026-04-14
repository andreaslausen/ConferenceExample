namespace ConferenceExample.Conference.Application.ScheduleTalk;

public interface IScheduleTalkCommandHandler
{
    Task Handle(ScheduleTalkCommand command);
}
