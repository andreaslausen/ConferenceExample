using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Domain;

public class Session(
    SessionId id,
    SessionTitle title,
    SpeakerId speakerId,
    SessionStatus status,
    IEnumerable<SessionTag> tags,
    SessionType type,
    Abstract @abstract,
    ConferenceId conferenceId)
{
    private readonly List<SessionTag> _tags = [..tags];
    public SessionId Id { get; } = id;
    public SessionTitle Title { get; private set; } = title;
    public SpeakerId SpeakerId { get; } = speakerId;
    public SessionStatus Status { get; private set; } = status;
    public SessionType Type { get; } = type;
    public Abstract Abstract { get; private set; } = @abstract;
    public IReadOnlyList<SessionTag> Tags => _tags;
    public ConferenceId ConferenceId { get; } = conferenceId;

    // TODO: Maybe this has to go to a domain service. What happens if the cfp is closed?
    public void Submit()
    {
        Status = SessionStatus.Submitted;
    }

    public void Accept()
    {
        Status = SessionStatus.Accepted;
    }

    public void Reject()
    {
        Status = SessionStatus.Rejected;
    }

    public void EditTitle(SessionTitle title)
    {
        Title = title;
    }

    public void EditAbstract(Abstract @abstract)
    {
        Abstract = @abstract;
    }

    public void AddTag(SessionTag tag)
    {
        _tags.Add(tag);
    }

    public void RemoveTag(SessionTag tag)
    {
        _tags.Remove(tag);
    }
}