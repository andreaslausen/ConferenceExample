namespace ConferenceExample.Conference.Application.RemoveTalkType;

public interface IRemoveTalkTypeCommandHandler
{
    Task Handle(RemoveTalkTypeCommand command);
}
