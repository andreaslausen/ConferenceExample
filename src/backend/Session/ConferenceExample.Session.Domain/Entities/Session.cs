using ConferenceExample.Session.Domain.Events;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Domain.Entities;

public class Session : AggregateRoot
{
    private readonly List<SessionTag> _tags = [];

    public SessionId Id { get; private set; } = null!;
    public SessionTitle Title { get; private set; } = null!;
    public SpeakerId SpeakerId { get; private set; } = null!;
    public SessionStatus Status { get; private set; }
    public SessionTypeId SessionTypeId { get; private set; } = null!;
    public Abstract Abstract { get; private set; } = null!;
    public IReadOnlyList<SessionTag> Tags => _tags;
    public ConferenceId ConferenceId { get; private set; } = null!;

    private Session() { }

    public static Session LoadFromHistory(IEnumerable<IDomainEvent> events)
    {
        var session = new Session();
        session.ReplayEvents(events);
        return session;
    }

    public static Session Submit(
        SessionId id,
        SessionTitle title,
        SpeakerId speakerId,
        IEnumerable<SessionTag> tags,
        SessionTypeId sessionTypeId,
        Abstract @abstract,
        ConferenceId conferenceId
    )
    {
        ArgumentNullException.ThrowIfNull(tags);

        var session = new Session();
        session.RaiseEvent(
            new SessionSubmittedEvent(
                id.Value,
                DateTimeOffset.UtcNow,
                title.Title,
                @abstract.Content,
                speakerId.Value,
                tags.Select(t => t.Tag).ToList(),
                sessionTypeId.Value,
                conferenceId.Value
            )
        );
        return session;
    }

    public void EditTitle(SessionTitle title)
    {
        RaiseEvent(new SessionTitleEditedEvent(Id.Value, DateTimeOffset.UtcNow, title.Title));
    }

    public void EditAbstract(Abstract @abstract)
    {
        RaiseEvent(
            new SessionAbstractEditedEvent(Id.Value, DateTimeOffset.UtcNow, @abstract.Content)
        );
    }

    public void AddTag(SessionTag tag)
    {
        RaiseEvent(new SessionTagAddedEvent(Id.Value, DateTimeOffset.UtcNow, tag.Tag));
    }

    public void RemoveTag(SessionTag tag)
    {
        RaiseEvent(new SessionTagRemovedEvent(Id.Value, DateTimeOffset.UtcNow, tag.Tag));
    }

    protected override void ApplyEvent(IDomainEvent @event)
    {
        switch (@event)
        {
            case SessionSubmittedEvent e:
                Id = new SessionId(new GuidV7(e.AggregateId));
                Title = new SessionTitle(e.Title);
                SpeakerId = new SpeakerId(new GuidV7(e.SpeakerId));
                SessionTypeId = new SessionTypeId(new GuidV7(e.SessionTypeId));
                Abstract = new Abstract(e.Abstract);
                ConferenceId = new ConferenceId(new GuidV7(e.ConferenceId));
                Status = SessionStatus.Submitted;
                _tags.AddRange(e.Tags.Select(t => new SessionTag(t)));
                break;
            case SessionTitleEditedEvent e:
                Title = new SessionTitle(e.Title);
                break;
            case SessionAbstractEditedEvent e:
                Abstract = new Abstract(e.Abstract);
                break;
            case SessionTagAddedEvent e:
                _tags.Add(new SessionTag(e.Tag));
                break;
            case SessionTagRemovedEvent e:
                _tags.Remove(new SessionTag(e.Tag));
                break;
        }
    }
}
