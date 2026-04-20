using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Application.CreateSpeakerProfile;
using ConferenceExample.Talk.Application.GetMyProfile;
using ConferenceExample.Talk.Application.GetSpeakerById;
using ConferenceExample.Talk.Application.UpdateSpeakerProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Speakers")]
public class SpeakersController(ISpeakerService speakerService) : ControllerBase
{
    [HttpPost("profile", Name = "CreateSpeakerProfile")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateSpeakerProfileDto dto)
    {
        await speakerService.CreateProfile(dto);
        return Created("/api/speakers/profile", null);
    }

    [HttpPut("profile", Name = "UpdateSpeakerProfile")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateSpeakerProfileDto dto)
    {
        await speakerService.UpdateProfile(dto);
        return NoContent();
    }

    [HttpGet("profile", Name = "GetMyProfile")]
    [Authorize(Roles = "Speaker")]
    [ProducesResponseType<GetMyProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetMyProfileDto>> GetMyProfile()
    {
        var profile = await speakerService.GetMyProfile();

        if (profile is null)
            return NotFound();

        return Ok(profile);
    }

    [HttpGet("{id:guid}", Name = "GetSpeakerById")]
    [Authorize]
    [ProducesResponseType<GetSpeakerByIdDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetSpeakerByIdDto>> GetSpeakerById(Guid id)
    {
        var speaker = await speakerService.GetSpeakerById(id);

        if (speaker is null)
            return NotFound();

        return Ok(speaker);
    }
}
