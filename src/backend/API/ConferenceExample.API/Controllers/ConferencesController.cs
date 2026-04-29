using ConferenceExample.Conference.Application;
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
using ConferenceExample.Conference.Application.GetMyConferences;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;
using ConferenceExample.Conference.Application.UpdateConferenceDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Conferences")]
public class ConferencesController(IConferenceService conferenceService) : ControllerBase
{
    [HttpGet(Name = "GetAllConferences")]
    [AllowAnonymous]
    [ProducesResponseType<PagedResult<GetAllConferencesDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<GetAllConferencesDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var conferences = await conferenceService.GetAllConferences();
        var items = conferences.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new PagedResult<GetAllConferencesDto>(items, conferences.Count, page, pageSize));
    }

    [HttpGet("my", Name = "GetMyConferences")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType<PagedResult<GetMyConferencesDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<GetMyConferencesDto>>> GetMyConferences(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var conferences = await conferenceService.GetMyConferences();
        var items = conferences.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new PagedResult<GetMyConferencesDto>(items, conferences.Count, page, pageSize));
    }

    [HttpGet("{id:guid}", Name = "GetConferenceById")]
    [AllowAnonymous]
    [ProducesResponseType<GetConferenceByIdDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetConferenceByIdDto>> GetById(Guid id)
    {
        var conference = await conferenceService.GetConferenceById(id);
        return Ok(conference);
    }

    [HttpPost(Name = "CreateConference")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType<ConferenceCreatedDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ConferenceCreatedDto>> CreateConference(
        [FromBody] CreateConferenceDto dto
    )
    {
        var result = await conferenceService.CreateConference(dto);
        return Created($"/api/conferences/{result.Id}", result);
    }

    [HttpPut("{id:guid}/details", Name = "UpdateConferenceDetails")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateConferenceDetails(
        Guid id,
        [FromBody] UpdateConferenceDetailsDto dto
    )
    {
        await conferenceService.UpdateConferenceDetails(id, dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/name", Name = "RenameConference")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RenameConference(Guid id, [FromBody] RenameConferenceDto dto)
    {
        await conferenceService.RenameConference(id, dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/status", Name = "ChangeConferenceStatus")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeConferenceStatus(
        Guid id,
        [FromBody] ChangeConferenceStatusDto dto
    )
    {
        await conferenceService.ChangeConferenceStatus(id, dto);
        return NoContent();
    }

    [HttpGet("{id:guid}/schedule", Name = "GetConferenceSchedule")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType<IReadOnlyList<GetConferenceScheduleDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<GetConferenceScheduleDto>>> GetConferenceSchedule(
        Guid id
    )
    {
        var schedule = await conferenceService.GetConferenceSchedule(id);
        return Ok(schedule);
    }

    [HttpGet("{id:guid}/talks", Name = "GetConferenceTalks")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType<PagedResult<GetConferenceTalksDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<GetConferenceTalksDto>>> GetConferenceTalks(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var talks = await conferenceService.GetConferenceTalks(id);
        var items = talks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new PagedResult<GetConferenceTalksDto>(items, talks.Count, page, pageSize));
    }

    [HttpPut("{conferenceId:guid}/talks/{talkId:guid}/accept", Name = "AcceptTalk")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AcceptTalk(Guid conferenceId, Guid talkId)
    {
        await conferenceService.AcceptTalk(conferenceId, talkId);
        return NoContent();
    }

    [HttpPut("{conferenceId:guid}/talks/{talkId:guid}/reject", Name = "RejectTalk")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RejectTalk(Guid conferenceId, Guid talkId)
    {
        await conferenceService.RejectTalk(conferenceId, talkId);
        return NoContent();
    }

    [HttpPut("{conferenceId:guid}/talks/{talkId:guid}/schedule", Name = "ScheduleTalk")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ScheduleTalk(
        Guid conferenceId,
        Guid talkId,
        [FromBody] ScheduleTalkDto dto
    )
    {
        await conferenceService.ScheduleTalk(conferenceId, talkId, dto);
        return NoContent();
    }

    [HttpPut("{conferenceId:guid}/talks/{talkId:guid}/room", Name = "AssignTalkToRoom")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignTalkToRoom(
        Guid conferenceId,
        Guid talkId,
        [FromBody] AssignTalkToRoomDto dto
    )
    {
        await conferenceService.AssignTalkToRoom(conferenceId, talkId, dto);
        return NoContent();
    }

    [HttpGet("{id:guid}/talk-types", Name = "GetConferenceTalkTypes")]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<GetConferenceTalkTypesDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<IReadOnlyList<GetConferenceTalkTypesDto>>
    > GetConferenceTalkTypes(Guid id)
    {
        var talkTypes = await conferenceService.GetConferenceTalkTypes(id);
        return Ok(talkTypes);
    }

    [HttpPost("{id:guid}/talk-types", Name = "DefineTalkType")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType<TalkTypeDefinedDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TalkTypeDefinedDto>> DefineTalkType(
        Guid id,
        [FromBody] DefineTalkTypeDto dto
    )
    {
        var result = await conferenceService.DefineTalkType(id, dto);
        return Created($"/api/conferences/{id}/talk-types/{result.TalkTypeId}", result);
    }

    [HttpGet("{id:guid}/program", Name = "GetConferenceProgram")]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<GetConferenceProgramDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<GetConferenceProgramDto>>> GetConferenceProgram(
        Guid id
    )
    {
        var program = await conferenceService.GetConferenceProgram(id);
        return Ok(program);
    }

    [HttpGet("{id:guid}/rooms", Name = "GetConferenceRooms")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType<IReadOnlyList<GetConferenceRoomsDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<GetConferenceRoomsDto>>> GetConferenceRooms(
        Guid id
    )
    {
        var rooms = await conferenceService.GetConferenceRooms(id);
        return Ok(rooms);
    }

    [HttpPost("{id:guid}/rooms", Name = "AddRoom")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType<RoomAddedDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RoomAddedDto>> AddRoom(Guid id, [FromBody] AddRoomDto dto)
    {
        var result = await conferenceService.AddRoom(id, dto);
        return Created($"/api/conferences/{id}/rooms/{result.RoomId}", result);
    }

    [HttpDelete("{conferenceId:guid}/rooms/{roomId:guid}", Name = "RemoveRoom")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveRoom(Guid conferenceId, Guid roomId)
    {
        await conferenceService.RemoveRoom(conferenceId, roomId);
        return NoContent();
    }

    [HttpDelete("{conferenceId:guid}/talk-types/{talkTypeId:guid}", Name = "RemoveTalkType")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveTalkType(Guid conferenceId, Guid talkTypeId)
    {
        await conferenceService.RemoveTalkType(conferenceId, talkTypeId);
        return NoContent();
    }
}
