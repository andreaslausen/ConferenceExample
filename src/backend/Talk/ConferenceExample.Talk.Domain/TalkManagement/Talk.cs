using ConferenceExample.Talk.Domain.ConferenceManagement;
using ConferenceExample.Talk.Domain.SharedKernel;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement.Events;

namespace ConferenceExample.Talk.Domain.TalkManagement;

/// <summary>
/// A talk owned by one speaker, independent of any conference. It is created, edited and deleted
/// on its own, and can be submitted to any number of conferences; each submission freezes the
/// talk's content at that moment.
/// </summary>
public class Talk : AggregateRoot
{
    private readonly List<TalkTag> _tags = [];
    private readonly List<TalkSubmission> _submissions = [];

    public TalkId Id { get; private set; } = null!;
    public TalkTitle Title { get; private set; } = null!;
    public Abstract Abstract { get; private set; } = null!;
    public SpeakerId SpeakerId { get; private set; } = null!;
    public bool IsDeleted { get; private set; }
    public IReadOnlyList<TalkTag> Tags => _tags;
    public IReadOnlyList<TalkSubmission> Submissions => _submissions;

    private Talk() { }

    public static Talk LoadFromHistory(IEnumerable<IDomainEvent> events)
    {
        var talk = new Talk();
        talk.ReplayEvents(events);
        return talk;
    }

    public static Talk Create(
        TalkId id,
        TalkTitle title,
        Abstract @abstract,
        IEnumerable<TalkTag> tags,
        SpeakerId speakerId
    )
    {
        ArgumentNullException.ThrowIfNull(tags);

        var talk = new Talk();
        talk.RaiseEvent(
            new TalkCreatedEvent(
                id.Value,
                DateTimeOffset.UtcNow,
                title.Title,
                @abstract.Content,
                speakerId.Value,
                tags.Select(t => t.Tag).ToList()
            )
        );
        return talk;
    }

    public void EditTitle(TalkTitle title)
    {
        EnsureNotDeleted();

        RaiseEvent(new TalkTitleEditedEvent(Id.Value, DateTimeOffset.UtcNow, title.Title));
    }

    public void EditAbstract(Abstract @abstract)
    {
        EnsureNotDeleted();

        RaiseEvent(new TalkAbstractEditedEvent(Id.Value, DateTimeOffset.UtcNow, @abstract.Content));
    }

    public void AddTag(TalkTag tag)
    {
        EnsureNotDeleted();

        RaiseEvent(new TalkTagAddedEvent(Id.Value, DateTimeOffset.UtcNow, tag.Tag));
    }

    public void RemoveTag(TalkTag tag)
    {
        EnsureNotDeleted();

        RaiseEvent(new TalkTagRemovedEvent(Id.Value, DateTimeOffset.UtcNow, tag.Tag));
    }

    /// <summary>
    /// Deleting a talk withdraws it from the speaker's own list only. Conferences keep the
    /// submissions they already registered, because those are snapshots they own — a published
    /// program must not fall apart because a speaker tidied up their talk list.
    /// </summary>
    public void Delete()
    {
        EnsureNotDeleted();

        RaiseEvent(new TalkDeletedEvent(Id.Value, DateTimeOffset.UtcNow));
    }

    public void SubmitToConference(ConferenceId conferenceId, TalkTypeId talkTypeId)
    {
        EnsureNotDeleted();

        if (_submissions.Any(s => s.ConferenceId == conferenceId))
        {
            throw new DomainException(
                $"Talk '{Id.Value}' has already been submitted to conference '{conferenceId.Value}'."
            );
        }

        RaiseEvent(
            new TalkSubmittedToConferenceEvent(
                Id.Value,
                DateTimeOffset.UtcNow,
                conferenceId.Value,
                talkTypeId.Value,
                SpeakerId.Value,
                Title.Title,
                Abstract.Content,
                _tags.Select(t => t.Tag).ToList()
            )
        );
    }

    protected override void ApplyEvent(IDomainEvent @event)
    {
        switch (@event)
        {
            case TalkCreatedEvent e:
                Id = new TalkId(new GuidV7(e.AggregateId));
                Title = new TalkTitle(e.Title);
                Abstract = new Abstract(e.Abstract);
                SpeakerId = new SpeakerId(new GuidV7(e.SpeakerId));
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
                _tags.RemoveAll(t => t.Tag == e.Tag);
                break;
            case TalkDeletedEvent:
                IsDeleted = true;
                break;
            case TalkSubmittedToConferenceEvent e:
                _submissions.Add(
                    new TalkSubmission(
                        new ConferenceId(new GuidV7(e.ConferenceId)),
                        new TalkTypeId(new GuidV7(e.TalkTypeId)),
                        e.OccurredAt,
                        new TalkTitle(e.Title),
                        new Abstract(e.Abstract),
                        e.Tags.Select(t => new TalkTag(t)).ToList()
                    )
                );
                break;
        }
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new DomainException($"Talk '{Id.Value}' has been deleted and cannot be changed.");
        }
    }
}
