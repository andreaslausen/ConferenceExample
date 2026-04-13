namespace ConferenceExample.Authentication;

public class User
{
    public UserId Id { get; init; } = new(SharedKernel.ValueObjects.Ids.GuidV7.NewGuid());
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
