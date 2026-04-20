using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Talk.Persistence.ReadModels;

public class SpeakerDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("lastName")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("biography")]
    public string Biography { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [BsonElement("lastModifiedAt")]
    public DateTimeOffset LastModifiedAt { get; set; }

    [BsonElement("version")]
    public long Version { get; set; } = -1;
}
