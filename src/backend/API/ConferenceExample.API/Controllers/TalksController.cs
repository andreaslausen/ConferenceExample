using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Application.CreateTalk;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.GetTalkById;
using ConferenceExample.Talk.Application.GetTalkSubmissions;
using ConferenceExample.Talk.Application.SubmitTalkToConference;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Controllers;

/// <summary>
/// A speaker's own talks. Talks live independently of any conference here; submitting one to a
/// conference is a separate action on an existing talk.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Talks")]
public class TalksController(ITalkService talkService) : ControllerBase
{
    [HttpPost(Name = "CreateTalk")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTalk([FromBody] CreateTalkDto dto)
    {
        var talkId = await talkService.CreateTalk(dto);
        return Created($"/api/talks/{talkId}", null);
    }

    [HttpGet("{id:guid}", Name = "GetTalkById")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType<GetTalkByIdDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetTalkByIdDto>> GetTalkById(Guid id)
    {
        var talk = await talkService.GetTalkById(id);
        if (talk is null)
            return NotFound();
        return Ok(talk);
    }

    [HttpGet("my-talks", Name = "GetMyTalks")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType<PagedResult<GetMyTalksDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<GetMyTalksDto>>> GetMyTalks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var (items, totalCount) = await talkService.GetMyTalks(page, pageSize);
        return Ok(new PagedResult<GetMyTalksDto>(items, totalCount, page, pageSize));
    }

    [HttpPut("{id:guid}", Name = "EditTalk")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditTalk(Guid id, [FromBody] EditTalkDto dto)
    {
        await talkService.EditTalk(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}", Name = "DeleteTalk")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTalk(Guid id)
    {
        await talkService.DeleteTalk(id);
        return NoContent();
    }

    /// <summary>
    /// Submits an existing talk to a conference. The conference decides asynchronously whether it
    /// takes the submission into review — poll the submission list for the outcome.
    /// </summary>
    [HttpPost("{id:guid}/submissions", Name = "SubmitTalkToConference")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitTalkToConference(
        Guid id,
        [FromBody] SubmitTalkToConferenceDto dto
    )
    {
        await talkService.SubmitTalkToConference(id, dto);
        return Accepted($"/api/talks/{id}/submissions");
    }

    [HttpGet("{id:guid}/submissions", Name = "GetTalkSubmissions")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType<IReadOnlyList<GetTalkSubmissionsDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<GetTalkSubmissionsDto>>> GetTalkSubmissions(
        Guid id
    )
    {
        var submissions = await talkService.GetTalkSubmissions(id);
        if (submissions is null)
            return NotFound();
        return Ok(submissions);
    }
}
