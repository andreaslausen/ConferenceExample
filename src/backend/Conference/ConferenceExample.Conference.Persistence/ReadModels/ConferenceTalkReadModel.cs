using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Conference.Persistence.ReadModels;

/// <summary>
/// Read Model for Talk data replicated to the Conference bounded context.
/// This is denormalized talk data synchronized from the Talk BC via events.
/// Stored separately from the Conference aggregate to enable efficient queries.
/// </summary>
public class ConferenceTalkReadModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("conferenceId")]
    [BsonRepresentation(BsonType.String)]
    public string ConferenceId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("abstract")]
    public string Abstract { get; set; } = string.Empty;

    [BsonElement("speakerId")]
    [BsonRepresentation(BsonType.String)]
    public string SpeakerId { get; set; } = string.Empty;

    [BsonElement("talkTypeId")]
    [BsonRepresentation(BsonType.String)]
    public string TalkTypeId { get; set; } = string.Empty;

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    // Conference BC specific fields (managed by Conference aggregate)
    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public string Status { get; set; } = "Submitted";

    [BsonElement("slotStart")]
    [BsonRepresentation(BsonType.String)]
    public DateTimeOffset? SlotStart { get; set; }

    [BsonElement("slotEnd")]
    [BsonRepresentation(BsonType.String)]
    public DateTimeOffset? SlotEnd { get; set; }

    [BsonElement("roomId")]
    [BsonRepresentation(BsonType.String)]
    public string? RoomId { get; set; }

    [BsonElement("roomName")]
    public string? RoomName { get; set; }

    [BsonElement("submittedAt")]
    public DateTimeOffset SubmittedAt { get; set; }

    [BsonElement("lastModifiedAt")]
    public DateTimeOffset LastModifiedAt { get; set; }

    [BsonElement("version")]
    public long Version { get; set; } = -1;
}
