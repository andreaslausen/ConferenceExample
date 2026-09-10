using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Talk.Persistence.ReadModels;

/// <summary>
/// UserId -> SpeakerId, replicated from the Speaker BC's SpeakerProfileCreatedEvent. Keyed by the
/// speaker id, because that is what the Talk BC stores on a talk.
/// </summary>
public class SpeakerDirectoryDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public string UserId { get; set; } = string.Empty;
}
