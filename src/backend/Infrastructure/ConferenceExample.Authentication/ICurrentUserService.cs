namespace ConferenceExample.Authentication;

public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current authenticated user's ID.
    /// </summary>
    /// <returns>The current user's ID.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authenticated.</exception>
    UserId GetCurrentUserId();

    /// <summary>
    /// Gets the current authenticated user's role.
    /// </summary>
    /// <returns>The current user's role.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authenticated.</exception>
    UserRole GetCurrentUserRole();

    /// <summary>
    /// Gets the current authenticated user's email.
    /// </summary>
    /// <returns>The current user's email.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authenticated.</exception>
    string GetCurrentUserEmail();

    /// <summary>
    /// Checks if a user is currently authenticated.
    /// </summary>
    /// <returns>True if a user is authenticated; otherwise, false.</returns>
    bool IsAuthenticated();
}
