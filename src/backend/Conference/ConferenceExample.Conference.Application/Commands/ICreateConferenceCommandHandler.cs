using ConferenceExample.Conference.Application.Dtos;

namespace ConferenceExample.Conference.Application.Commands;

public interface ICreateConferenceCommandHandler
{
    Task<ConferenceCreatedDto> Handle(CreateConferenceCommand command);
}
