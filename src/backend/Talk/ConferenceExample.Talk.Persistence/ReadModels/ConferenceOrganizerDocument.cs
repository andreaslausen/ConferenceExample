using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Talk.Persistence.ReadModels;

public class ConferenceOrganizerDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("organizerId")]
    [BsonRepresentation(BsonType.String)]
    public string OrganizerId { get; set; } = string.Empty;
}
