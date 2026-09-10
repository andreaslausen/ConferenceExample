using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Conference.Persistence.ReadModels;

/// <summary>
/// One talk's submission to this conference. Keyed by conference and talk together, because the
/// same talk can be submitted to several conferences, each with its own review outcome and
/// schedule. Title, abstract, tags and the speaker's details are the snapshot taken when the
/// submission was registered — later edits by the speaker do not reach this document.
/// </summary>
public class ConferenceTalkDocument
{
    public static string BuildId(Guid conferenceId, Guid talkId) => $"{conferenceId}_{talkId}";

    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("talkId")]
    [BsonRepresentation(BsonType.String)]
    public string TalkId { get; set; } = string.Empty;

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

    [BsonElement("speakerFirstName")]
    public string SpeakerFirstName { get; set; } = string.Empty;

    [BsonElement("speakerLastName")]
    public string SpeakerLastName { get; set; } = string.Empty;

    [BsonElement("speakerBiography")]
    public string SpeakerBiography { get; set; } = string.Empty;

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
