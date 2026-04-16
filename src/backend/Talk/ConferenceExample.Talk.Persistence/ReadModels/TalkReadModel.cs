using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Talk.Persistence.ReadModels;

/// <summary>
/// Read Model for Talk queries in the Talk bounded context.
/// Denormalized data stored in MongoDB for efficient querying.
/// Updated via event handlers when Talk domain events occur.
/// </summary>
public class TalkReadModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

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

    [BsonElement("conferenceId")]
    [BsonRepresentation(BsonType.String)]
    public string ConferenceId { get; set; } = string.Empty;

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public string Status { get; set; } = "Submitted";

    [BsonElement("submittedAt")]
    public DateTimeOffset SubmittedAt { get; set; }

    [BsonElement("lastModifiedAt")]
    public DateTimeOffset LastModifiedAt { get; set; }

    [BsonElement("version")]
    public long Version { get; set; } = -1;
}
