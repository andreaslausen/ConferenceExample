using ConferenceExample.Talk.Domain.Events;
using ConferenceExample.Talk.Domain.ValueObjects;
using ConferenceExample.Talk.Domain.ValueObjects.Ids;

namespace ConferenceExample.Talk.Domain.Entities;

public class Talk : AggregateRoot
{
    private readonly List<TalkTag> _tags = [];

    public TalkId Id { get; private set; } = null!;
    public TalkTitle Title { get; private set; } = null!;
    public SpeakerId SpeakerId { get; private set; } = null!;
    public TalkStatus Status { get; private set; }
    public TalkTypeId TalkTypeId { get; private set; } = null!;
    public Abstract Abstract { get; private set; } = null!;
    public IReadOnlyList<TalkTag> Tags => _tags;
    public ConferenceId ConferenceId { get; private set; } = null!;

    private Talk() { }

    public static Talk LoadFromHistory(IEnumerable<IDomainEvent> events)
    {
        var talk = new Talk();
        talk.ReplayEvents(events);
        return talk;
    }

    public static Talk Submit(
        TalkId id,
        TalkTitle title,
        SpeakerId speakerId,
        IEnumerable<TalkTag> tags,
        TalkTypeId talkTypeId,
        Abstract @abstract,
        ConferenceId conferenceId
    )
    {
        ArgumentNullException.ThrowIfNull(tags);

        var talk = new Talk();
        talk.RaiseEvent(
            new TalkSubmittedEvent(
                id.Value,
                DateTimeOffset.UtcNow,
                title.Title,
                @abstract.Content,
                speakerId.Value,
                tags.Select(t => t.Tag).ToList(),
                talkTypeId.Value,
                conferenceId.Value
            )
        );
        return talk;
    }

    public void EditTitle(TalkTitle title)
    {
        RaiseEvent(new TalkTitleEditedEvent(Id.Value, DateTimeOffset.UtcNow, title.Title));
    }

    public void EditAbstract(Abstract @abstract)
    {
        RaiseEvent(new TalkAbstractEditedEvent(Id.Value, DateTimeOffset.UtcNow, @abstract.Content));
    }

    public void AddTag(TalkTag tag)
    {
        RaiseEvent(new TalkTagAddedEvent(Id.Value, DateTimeOffset.UtcNow, tag.Tag));
    }

    public void RemoveTag(TalkTag tag)
    {
        RaiseEvent(new TalkTagRemovedEvent(Id.Value, DateTimeOffset.UtcNow, tag.Tag));
    }

    protected override void ApplyEvent(IDomainEvent @event)
    {
        switch (@event)
        {
            case TalkSubmittedEvent e:
                Id = new TalkId(new GuidV7(e.AggregateId));
                Title = new TalkTitle(e.Title);
                SpeakerId = new SpeakerId(new GuidV7(e.SpeakerId));
                TalkTypeId = new TalkTypeId(new GuidV7(e.TalkTypeId));
                Abstract = new Abstract(e.Abstract);
                ConferenceId = new ConferenceId(new GuidV7(e.ConferenceId));
                Status = TalkStatus.Submitted;
                _tags.AddRange(e.Tags.Select(t => new TalkTag(t)));
                break;
            case TalkTitleEditedEvent e:
                Title = new TalkTitle(e.Title);
                break;
            case TalkAbstractEditedEvent e:
                Abstract = new Abstract(e.Abstract);
                break;
            case TalkTagAddedEvent e:
                _tags.Add(new TalkTag(e.Tag));
                break;
            case TalkTagRemovedEvent e:
                _tags.Remove(new TalkTag(e.Tag));
                break;
        }
    }
}
