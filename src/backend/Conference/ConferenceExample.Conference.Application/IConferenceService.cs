using ConferenceExample.Conference.Application.AcceptTalk;
using ConferenceExample.Conference.Application.AddRoom;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.DefineTalkType;
using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Application.GetConferenceProgram;
using ConferenceExample.Conference.Application.GetConferenceRooms;
using ConferenceExample.Conference.Application.GetConferenceSchedule;
using ConferenceExample.Conference.Application.GetConferenceTalks;
using ConferenceExample.Conference.Application.GetConferenceTalkTypes;
using ConferenceExample.Conference.Application.RejectTalk;
using ConferenceExample.Conference.Application.RemoveRoom;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;
using ConferenceExample.Conference.Application.UpdateConferenceDetails;

namespace ConferenceExample.Conference.Application;

public interface IConferenceService
{
    Task<ConferenceCreatedDto> CreateConference(CreateConferenceDto createConferenceDto);
    Task RenameConference(Guid id, RenameConferenceDto dto);
    Task UpdateConferenceDetails(Guid id, UpdateConferenceDetailsDto dto);
    Task ChangeConferenceStatus(Guid id, ChangeConferenceStatusDto dto);
    Task<IReadOnlyList<GetAllConferencesDto>> GetAllConferences();
    Task<GetConferenceByIdDto> GetConferenceById(Guid conferenceId);
    Task<IReadOnlyList<GetConferenceScheduleDto>> GetConferenceSchedule(Guid conferenceId);
    Task<IReadOnlyList<GetConferenceTalksDto>> GetConferenceTalks(Guid conferenceId);
    Task AcceptTalk(Guid conferenceId, Guid talkId);
    Task RejectTalk(Guid conferenceId, Guid talkId);
    Task ScheduleTalk(Guid conferenceId, Guid talkId, ScheduleTalkDto dto);
    Task AssignTalkToRoom(Guid conferenceId, Guid talkId, AssignTalkToRoomDto dto);
    Task<TalkTypeDefinedDto> DefineTalkType(Guid conferenceId, DefineTalkTypeDto dto);
    Task RemoveTalkType(Guid conferenceId, Guid talkTypeId);
    Task<IReadOnlyList<GetConferenceTalkTypesDto>> GetConferenceTalkTypes(Guid conferenceId);
    Task<IReadOnlyList<GetConferenceProgramDto>> GetConferenceProgram(Guid conferenceId);
    Task<RoomAddedDto> AddRoom(Guid conferenceId, AddRoomDto dto);
    Task RemoveRoom(Guid conferenceId, Guid roomId);
    Task<IReadOnlyList<GetConferenceRoomsDto>> GetConferenceRooms(Guid conferenceId);
}
