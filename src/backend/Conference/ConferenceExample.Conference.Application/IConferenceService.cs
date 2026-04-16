using ConferenceExample.Conference.Application.AcceptTalk;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.DefineTalkType;
using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.GetConferenceTalks;
using ConferenceExample.Conference.Application.GetConferenceTalkTypes;
using ConferenceExample.Conference.Application.RejectTalk;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;

namespace ConferenceExample.Conference.Application;

public interface IConferenceService
{
    Task<ConferenceCreatedDto> CreateConference(CreateConferenceDto createConferenceDto);
    Task RenameConference(Guid id, RenameConferenceDto dto);
    Task ChangeConferenceStatus(Guid id, ChangeConferenceStatusDto dto);
    Task<IReadOnlyList<GetAllConferencesDto>> GetAllConferences();
    Task<GetConferenceByIdDto> GetConferenceById(Guid conferenceId);
    Task<IReadOnlyList<GetConferenceSessionDto>> GetSessions(Guid conferenceId);
    Task<IReadOnlyList<GetConferenceTalksDto>> GetConferenceTalks(Guid conferenceId);
    Task AcceptTalk(Guid conferenceId, Guid talkId);
    Task RejectTalk(Guid conferenceId, Guid talkId);
    Task ScheduleTalk(Guid conferenceId, Guid talkId, ScheduleTalkDto dto);
    Task AssignTalkToRoom(Guid conferenceId, Guid talkId, AssignTalkToRoomDto dto);
    Task<TalkTypeDefinedDto> DefineTalkType(Guid conferenceId, DefineTalkTypeDto dto);
    Task RemoveTalkType(Guid conferenceId, Guid talkTypeId);
    Task<IReadOnlyList<GetConferenceTalkTypesDto>> GetConferenceTalkTypes(Guid conferenceId);
}
