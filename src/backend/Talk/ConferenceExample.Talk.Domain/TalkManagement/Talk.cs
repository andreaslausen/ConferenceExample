using ConferenceExample.Talk.Domain.SharedKernel;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement.Events;

namespace ConferenceExample.Talk.Domain.TalkManagement;

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
        var tagList = tags.Select(t => t.Tag).ToList();
        talk.RaiseEvent(
            new TalkSubmittedEvent(
                id.Value,
                DateTimeOffset.UtcNow,
                0, // Version starts at 0
                title.Title,
                @abstract.Content,
                speakerId.Value,
                tagList,
                talkTypeId.Value,
                conferenceId.Value,
                TalkStatus.Submitted.ToString()
            )
        );
        return talk;
    }

    public void EditTitle(TalkTitle title)
    {
        RaiseEvent(
            new TalkTitleEditedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                title.Title,
                Abstract.Content,
                SpeakerId.Value,
                Tags.Select(t => t.Tag).ToList(),
                TalkTypeId.Value,
                ConferenceId.Value,
                Status.ToString()
            )
        );
    }

    public void EditAbstract(Abstract @abstract)
    {
        RaiseEvent(
            new TalkAbstractEditedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                Title.Title,
                @abstract.Content,
                SpeakerId.Value,
                Tags.Select(t => t.Tag).ToList(),
                TalkTypeId.Value,
                ConferenceId.Value,
                Status.ToString()
            )
        );
    }

    public void AddTag(TalkTag tag)
    {
        var updatedTags = Tags.Select(t => t.Tag).ToList();
        updatedTags.Add(tag.Tag);

        RaiseEvent(
            new TalkTagAddedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                Title.Title,
                Abstract.Content,
                SpeakerId.Value,
                updatedTags,
                TalkTypeId.Value,
                ConferenceId.Value,
                Status.ToString()
            )
        );
    }

    public void RemoveTag(TalkTag tag)
    {
        var updatedTags = Tags.Where(t => t.Tag != tag.Tag).Select(t => t.Tag).ToList();

        RaiseEvent(
            new TalkTagRemovedEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                Version + 1,
                Title.Title,
                Abstract.Content,
                SpeakerId.Value,
                updatedTags,
                TalkTypeId.Value,
                ConferenceId.Value,
                Status.ToString()
            )
        );
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
                Status = Enum.Parse<TalkStatus>(e.Status);
                _tags.Clear();
                _tags.AddRange(e.Tags.Select(t => new TalkTag(t)));
                break;
            case TalkTitleEditedEvent e:
                Title = new TalkTitle(e.Title);
                SpeakerId = new SpeakerId(new GuidV7(e.SpeakerId));
                TalkTypeId = new TalkTypeId(new GuidV7(e.TalkTypeId));
                Abstract = new Abstract(e.Abstract);
                ConferenceId = new ConferenceId(new GuidV7(e.ConferenceId));
                Status = Enum.Parse<TalkStatus>(e.Status);
                _tags.Clear();
                _tags.AddRange(e.Tags.Select(t => new TalkTag(t)));
                break;
            case TalkAbstractEditedEvent e:
                Title = new TalkTitle(e.Title);
                SpeakerId = new SpeakerId(new GuidV7(e.SpeakerId));
                TalkTypeId = new TalkTypeId(new GuidV7(e.TalkTypeId));
                Abstract = new Abstract(e.Abstract);
                ConferenceId = new ConferenceId(new GuidV7(e.ConferenceId));
                Status = Enum.Parse<TalkStatus>(e.Status);
                _tags.Clear();
                _tags.AddRange(e.Tags.Select(t => new TalkTag(t)));
                break;
            case TalkTagAddedEvent e:
                Title = new TalkTitle(e.Title);
                SpeakerId = new SpeakerId(new GuidV7(e.SpeakerId));
                TalkTypeId = new TalkTypeId(new GuidV7(e.TalkTypeId));
                Abstract = new Abstract(e.Abstract);
                ConferenceId = new ConferenceId(new GuidV7(e.ConferenceId));
                Status = Enum.Parse<TalkStatus>(e.Status);
                _tags.Clear();
                _tags.AddRange(e.Tags.Select(t => new TalkTag(t)));
                break;
            case TalkTagRemovedEvent e:
                Title = new TalkTitle(e.Title);
                SpeakerId = new SpeakerId(new GuidV7(e.SpeakerId));
                TalkTypeId = new TalkTypeId(new GuidV7(e.TalkTypeId));
                Abstract = new Abstract(e.Abstract);
                ConferenceId = new ConferenceId(new GuidV7(e.ConferenceId));
                Status = Enum.Parse<TalkStatus>(e.Status);
                _tags.Clear();
                _tags.AddRange(e.Tags.Select(t => new TalkTag(t)));
                break;
        }
    }
}
