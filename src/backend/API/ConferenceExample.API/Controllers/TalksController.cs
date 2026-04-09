using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TalksController(ITalkService talkService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitTalk([FromBody] SubmitTalkDto dto)
    {
        await talkService.SubmitTalk(dto);
        return Created();
    }
}
