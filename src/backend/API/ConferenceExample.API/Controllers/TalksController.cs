using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Application.EditTalk;
using ConferenceExample.Talk.Application.GetMyTalks;
using ConferenceExample.Talk.Application.GetTalkById;
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
        var talkId = await talkService.SubmitTalk(dto);
        return Created($"/api/talks/{talkId}", null);
    }

    [HttpGet("{id:guid}", Name = "GetTalkById")]
    [AllowAnonymous]
    [ProducesResponseType<GetTalkByIdDto>(StatusCodes.Status200OK)]
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
        var talks = await talkService.GetMyTalks();
        var items = talks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new PagedResult<GetMyTalksDto>(items, talks.Count, page, pageSize));
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
