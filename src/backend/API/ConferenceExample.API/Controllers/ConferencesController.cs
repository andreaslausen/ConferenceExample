using ConferenceExample.Conference.Application;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.RenameConference;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Conferences")]
public class ConferencesController(IConferenceService conferenceService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ConferenceCreatedDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConferenceCreatedDto>> CreateConference(
        [FromBody] CreateConferenceDto dto
    )
    {
        var result = await conferenceService.CreateConference(dto);
        return Created($"/api/conferences/{result.Id}", result);
    }

    [HttpPut("{id:guid}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RenameConference(Guid id, [FromBody] RenameConferenceDto dto)
    {
        await conferenceService.RenameConference(id, dto);
        return NoContent();
    }

    [HttpGet("{id:guid}/sessions")]
    [ProducesResponseType<IReadOnlyList<GetConferenceSessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<GetConferenceSessionDto>>> GetSessions(Guid id)
    {
        var sessions = await conferenceService.GetSessions(id);
        return Ok(sessions);
    }
}
