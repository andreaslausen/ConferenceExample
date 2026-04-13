namespace ConferenceExample.Conference.Application.CreateConference;

public interface ICreateConferenceCommandHandler
{
    Task<ConferenceCreatedDto> Handle(CreateConferenceCommand command);
}
