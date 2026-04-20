using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Talk.Persistence.ReadModels;

public class TalkDocument
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

    [BsonElement("speakerFirstName")]
    public string SpeakerFirstName { get; set; } = string.Empty;

    [BsonElement("speakerLastName")]
    public string SpeakerLastName { get; set; } = string.Empty;

    [BsonElement("speakerBiography")]
    public string SpeakerBiography { get; set; } = string.Empty;

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
