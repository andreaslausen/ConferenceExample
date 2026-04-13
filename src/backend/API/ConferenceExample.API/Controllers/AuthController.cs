using ConferenceExample.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceExample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Authentication")]
public class AuthController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<RegisterResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await authenticationService.Register(dto.Email, dto.Password, dto.Role);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponseDto(result.ErrorMessage ?? "Registration failed"));
        }

        return Ok(
            new RegisterResponseDto(
                result.Token ?? throw new InvalidOperationException("Token is null")
            )
        );
    }

    [HttpPost("login")]
    [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await authenticationService.Login(dto.Email, dto.Password);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponseDto(result.ErrorMessage ?? "Login failed"));
        }

        return Ok(
            new LoginResponseDto(
                result.Token ?? throw new InvalidOperationException("Token is null")
            )
        );
    }
}

public record RegisterRequestDto(string Email, string Password, UserRole Role);

public record RegisterResponseDto(string Token);

public record LoginRequestDto(string Email, string Password);

public record LoginResponseDto(string Token);

public record ErrorResponseDto(string Message);
