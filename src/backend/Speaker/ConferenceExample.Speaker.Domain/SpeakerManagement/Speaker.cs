using ConferenceExample.Speaker.Domain.SharedKernel;
using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Speaker.Domain.SpeakerManagement.Events;

namespace ConferenceExample.Speaker.Domain.SpeakerManagement;

public class Speaker : AggregateRoot
{
    public SpeakerId Id { get; private set; } = null!;

    /// <summary>The account this profile belongs to. Set at creation and never changed.</summary>
    public UserId UserId { get; private set; } = null!;
    public Name Name { get; private set; } = null!;
    public SpeakerBiography Biography { get; private set; } = null!;

    private Speaker() { }

    public static Speaker LoadFromHistory(IEnumerable<IDomainEvent> events)
    {
        var speaker = new Speaker();
        speaker.ReplayEvents(events);
        return speaker;
    }

    public static Speaker Create(SpeakerId id, UserId userId, Name name, SpeakerBiography biography)
    {
        var speaker = new Speaker();
        speaker.RaiseEvent(
            new SpeakerProfileCreatedEvent(
                id.Value,
                DateTimeOffset.UtcNow,
                userId.Value,
                name.FirstName,
                name.LastName,
                biography.Content
            )
        );
        return speaker;
    }

    public void UpdateProfile(Name name, SpeakerBiography biography)
    {
        RaiseEvent(
            new SpeakerProfileUpdatedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                name.FirstName,
                name.LastName,
                biography.Content
            )
        );
    }

    protected override void ApplyEvent(IDomainEvent @event)
    {
        switch (@event)
        {
            case SpeakerProfileCreatedEvent e:
                Id = new SpeakerId(new GuidV7(e.AggregateId));
                UserId = new UserId(new GuidV7(e.UserId));
                Name = new Name(e.FirstName, e.LastName);
                Biography = new SpeakerBiography(e.Biography);
                break;
            case SpeakerProfileUpdatedEvent e:
                Name = new Name(e.FirstName, e.LastName);
                Biography = new SpeakerBiography(e.Biography);
                break;
        }
    }
}
