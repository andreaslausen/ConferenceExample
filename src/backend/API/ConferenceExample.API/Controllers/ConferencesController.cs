using ConferenceExample.Conference.Application;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.GetConferenceSessions;
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
    [HttpPost]
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

    [HttpPut("{id:guid}/name")]
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

    [HttpGet("{id:guid}/sessions")]
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

    [HttpPut("{conferenceId:guid}/talks/{talkId:guid}/accept")]
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

    [HttpPut("{conferenceId:guid}/talks/{talkId:guid}/reject")]
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

    [HttpPut("{conferenceId:guid}/talks/{talkId:guid}/schedule")]
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

    [HttpPut("{conferenceId:guid}/talks/{talkId:guid}/room")]
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
}
