using ConferenceExample.Conference.Application;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConferencesController(IConferenceService conferenceService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateConference([FromBody] CreateConferenceDto dto)
    {
        await conferenceService.CreateConference(dto);
        return Created();
    }
}
