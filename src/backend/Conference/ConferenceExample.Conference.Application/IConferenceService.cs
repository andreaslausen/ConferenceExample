using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.RenameConference;

namespace ConferenceExample.Conference.Application;

public interface IConferenceService
{
    Task<ConferenceCreatedDto> CreateConference(CreateConferenceDto createConferenceDto);
    Task RenameConference(Guid id, RenameConferenceDto dto);
    Task<IReadOnlyList<SessionDto>> GetSessions(Guid conferenceId);
}
