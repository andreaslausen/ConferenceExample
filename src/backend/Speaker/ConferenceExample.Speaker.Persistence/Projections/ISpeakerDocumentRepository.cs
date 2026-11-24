namespace ConferenceExample.Speaker.Persistence.Projections;

/// <summary>
/// Raw CRUD on the <see cref="SpeakerDocument"/> projection, keyed by its storage shape rather than
/// domain types. Used exclusively by <c>SpeakerEventHandler</c> to build and update the projection
/// as events arrive — not a query API for application code.
///
/// For reading the projection as a domain-shaped result, use <see cref="ISpeakerReadModelRepository"/>
/// instead.
/// </summary>
public interface ISpeakerDocumentRepository
{
    Task<SpeakerDocument?> GetById(Guid speakerId);

    Task Save(SpeakerDocument document);

    Task Update(SpeakerDocument document);
}
