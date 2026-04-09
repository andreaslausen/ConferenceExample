using ConferenceExample.Conference.Application;
using ConferenceExample.Conference.Application.Dtos;
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
}
