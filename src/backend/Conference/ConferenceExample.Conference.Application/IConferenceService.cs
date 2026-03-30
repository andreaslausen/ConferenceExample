namespace ConferenceExample.Conference.Application;

public interface IConferenceService
{
    Task CreateConference(CreateConferenceDto createConferenceDto);
}