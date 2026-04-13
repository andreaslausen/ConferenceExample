using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Application.SubmitTalk;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Talks")]
public class TalksController(ITalkService talkService) : ControllerBase
{
    [HttpPost]
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
}
