using ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Authentication;

/// <summary>
/// MongoDB-persisted user document
/// </summary>
public class StoredUser
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("role")]
    [BsonRepresentation(BsonType.String)]
    public string Role { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    public User ToUser()
    {
        if (!Enum.TryParse<UserRole>(Role, out var userRole))
        {
            throw new InvalidOperationException($"Invalid user role: {Role}");
        }

        // Parse the GuidV7 from string stored in MongoDB
        return new User
        {
            Id = new UserId(GuidV7.Parse(Id)),
            Email = Email,
            PasswordHash = PasswordHash,
            Role = userRole,
            CreatedAt = CreatedAt,
        };
    }

    public static StoredUser FromUser(User user)
    {
        return new StoredUser
        {
            Id = user.Id.Value.ToString(),
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt,
        };
    }
}
