using ConferenceExample.Conference.Application.AcceptTalk;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.RejectTalk;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;

namespace ConferenceExample.Conference.Application;

public interface IConferenceService
{
    Task<ConferenceCreatedDto> CreateConference(CreateConferenceDto createConferenceDto);
    Task RenameConference(Guid id, RenameConferenceDto dto);
    Task<IReadOnlyList<GetConferenceSessionDto>> GetSessions(Guid conferenceId);
    Task AcceptTalk(Guid conferenceId, Guid talkId);
    Task RejectTalk(Guid conferenceId, Guid talkId);
    Task ScheduleTalk(Guid conferenceId, Guid talkId, ScheduleTalkDto dto);
    Task AssignTalkToRoom(Guid conferenceId, Guid talkId, AssignTalkToRoomDto dto);
}
