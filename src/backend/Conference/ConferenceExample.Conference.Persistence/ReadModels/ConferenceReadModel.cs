using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Conference.Persistence.ReadModels;

/// <summary>
/// Read Model for Conference queries in the Conference bounded context.
/// Denormalized data stored in MongoDB for efficient querying.
/// Updated via event handlers when Conference domain events occur.
/// </summary>
public class ConferenceReadModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("start")]
    public DateTimeOffset Start { get; set; }

    [BsonElement("end")]
    public DateTimeOffset End { get; set; }

    [BsonElement("locationName")]
    public string LocationName { get; set; } = string.Empty;

    [BsonElement("street")]
    public string Street { get; set; } = string.Empty;

    [BsonElement("city")]
    public string City { get; set; } = string.Empty;

    [BsonElement("state")]
    public string State { get; set; } = string.Empty;

    [BsonElement("postalCode")]
    public string PostalCode { get; set; } = string.Empty;

    [BsonElement("country")]
    public string Country { get; set; } = string.Empty;

    [BsonElement("organizerId")]
    [BsonRepresentation(BsonType.String)]
    public string OrganizerId { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public string Status { get; set; } = "Draft";

    [BsonElement("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [BsonElement("lastModifiedAt")]
    public DateTimeOffset LastModifiedAt { get; set; }

    [BsonElement("version")]
    public long Version { get; set; } = -1;
}
