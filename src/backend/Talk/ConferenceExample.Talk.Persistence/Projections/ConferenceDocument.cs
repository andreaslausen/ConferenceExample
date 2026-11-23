using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Talk.Persistence.ReadModels;

/// <summary>
/// Conference names replicated from the Conference BC, so a speaker's submission list can say
/// where a talk went instead of showing a bare id.
/// </summary>
public class ConferenceDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
}
