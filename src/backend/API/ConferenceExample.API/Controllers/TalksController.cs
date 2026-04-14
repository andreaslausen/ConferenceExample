using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.SubmitTalk;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Talks")]
public class TalksController(ITalkService talkService) : ControllerBase
{
    [HttpPost(Name = "SubmitTalk")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitTalk([FromBody] SubmitTalkDto dto)
    {
        await talkService.SubmitTalk(dto);
        return Created();
    }

    [HttpGet("my-talks", Name = "GetMyTalks")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType<IReadOnlyList<GetMyTalksDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<GetMyTalksDto>>> GetMyTalks()
    {
        var talks = await talkService.GetMyTalks();
        return Ok(talks);
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
}
