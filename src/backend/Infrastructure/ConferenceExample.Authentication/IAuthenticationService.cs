namespace ConferenceExample.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResult> Register(string email, string password, UserRole role);
    Task<AuthenticationResult> Login(string email, string password);
}

public record AuthenticationResult(bool Success, string? Token, string? ErrorMessage);
