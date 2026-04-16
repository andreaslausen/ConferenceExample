using ConferenceExample.Conference.Application;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.DefineTalkType;
using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.GetConferenceTalks;
using ConferenceExample.Conference.Application.GetConferenceTalkTypes;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;
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
    [ProducesResponseType<IReadOnlyList<GetAllConferencesDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GetAllConferencesDto>>> GetAll()
    {
        var conferences = await conferenceService.GetAllConferences();
        return Ok(conferences);
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

    [HttpGet("{id:guid}/sessions", Name = "GetSessions")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType<IReadOnlyList<GetConferenceSessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<GetConferenceSessionDto>>> GetSessions(Guid id)
    {
        var sessions = await conferenceService.GetSessions(id);
        return Ok(sessions);
    }

    [HttpGet("{id:guid}/talks", Name = "GetConferenceTalks")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType<IReadOnlyList<GetConferenceTalksDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<GetConferenceTalksDto>>> GetConferenceTalks(
        Guid id
    )
    {
        var talks = await conferenceService.GetConferenceTalks(id);
        return Ok(talks);
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
