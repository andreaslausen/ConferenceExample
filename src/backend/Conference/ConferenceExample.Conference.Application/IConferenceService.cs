using ConferenceExample.Conference.Application.Dtos;

namespace ConferenceExample.Conference.Application;

public interface IConferenceService
{
    Task<ConferenceCreatedDto> CreateConference(CreateConferenceDto createConferenceDto);
}
