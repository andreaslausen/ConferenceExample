using ConferenceExample.Talk.Domain.SharedKernel;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement.Events;

namespace ConferenceExample.Talk.Domain.SpeakerManagement;

public class Speaker : AggregateRoot
{
    public SpeakerId Id { get; private set; } = null!;
    public Name Name { get; private set; } = null!;
    public SpeakerBiography Biography { get; private set; } = null!;

    private Speaker() { }

    public static Speaker LoadFromHistory(IEnumerable<IDomainEvent> events)
    {
        var speaker = new Speaker();
        speaker.ReplayEvents(events);
        return speaker;
    }

    public static Speaker Create(SpeakerId id, Name name, SpeakerBiography biography)
    {
        var speaker = new Speaker();
        speaker.RaiseEvent(
            new SpeakerProfileCreatedEvent(
                id.Value,
                DateTimeOffset.UtcNow,
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
