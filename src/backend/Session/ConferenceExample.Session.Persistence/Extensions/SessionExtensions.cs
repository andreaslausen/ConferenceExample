using ConferenceExample.Session.Domain;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Persistence.Extensions;

public static class SessionExtensions
{
    public static Domain.Entities.Session ToDomain(this ConferenceExample.Persistence.Model.Session session)
    {
        return new Domain.Entities.Session(
            new SessionId(session.Id),
            new SessionTitle(session.Title),
            new SpeakerId(session.SpeakerId),
            session.Tags.Select(t => new SessionTag(t)).ToList(),
            new SessionTypeId(session.SessionTypeId),
            new Abstract(session.Abstract),
            new ConferenceId(session.ConferenceId),
            (SessionStatus)session.SessionStatus
        );
    }
    
    public static ConferenceExample.Persistence.Model.Session ToPersistence(this Domain.Entities.Session session)
    {
        return new ConferenceExample.Persistence.Model.Session
        {
            Id = session.Id.Value,
            Title = session.Title.Title,
            SpeakerId = session.SpeakerId.Value,
            Tags = session.Tags.Select(t => t.Tag).ToList(),
            SessionTypeId = session.SessionTypeId.Value,
            Abstract = session.Abstract.Content,
            ConferenceId = session.ConferenceId.Value,
            SessionStatus = (int)session.Status
        };
    }
}