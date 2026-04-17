using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferenceExample.Talk.Persistence.ReadModels;

/// <summary>
/// MongoDB-persisted Conference Read Model for the Talk bounded context.
/// Stores denormalized conference data replicated from the Conference BC.
/// Updated via event handlers when Conference domain events occur.
/// Converts to/from domain ConferenceInfo value object.
/// </summary>
public class ConferenceReadModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public string Status { get; set; } = "Draft";

    [BsonElement("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [BsonElement("lastModifiedAt")]
    public DateTimeOffset LastModifiedAt { get; set; }

    [BsonElement("version")]
    public long Version { get; set; } = -1;

    /// <summary>
    /// Converts this MongoDB document to a domain ConferenceInfo value object.
    /// </summary>
    public ConferenceInfo ToConferenceInfo()
    {
        return new ConferenceInfo(new ConferenceId(GuidV7.Parse(Id)), Name, Status);
    }

    /// <summary>
    /// Creates a ConferenceReadModel from domain data for initial creation.
    /// </summary>
    public static ConferenceReadModel FromDomainEvent(
        Guid aggregateId,
        string name,
        string status,
        DateTimeOffset occurredAt,
        long version
    )
    {
        return new ConferenceReadModel
        {
            Id = aggregateId.ToString(),
            Name = name,
            Status = status,
            CreatedAt = occurredAt,
            LastModifiedAt = occurredAt,
            Version = version,
        };
    }
}
