using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Conference.Persistence.ReadModels;

/// <summary>
/// Speaker profiles replicated from the Speaker BC. Read once, when a submission is registered, to
/// freeze the speaker's details into that submission — never read again afterwards, so a speaker
/// updating their profile does not rewrite conferences' history.
/// </summary>
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
}
