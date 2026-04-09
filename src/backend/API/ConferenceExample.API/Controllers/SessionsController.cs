using ConferenceExample.Session.Application;
using ConferenceExample.Session.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController(ISessionService sessionService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitSession([FromBody] SubmitSessionDto dto)
    {
        await sessionService.SubmitSession(dto);
        return Created();
    }
}
