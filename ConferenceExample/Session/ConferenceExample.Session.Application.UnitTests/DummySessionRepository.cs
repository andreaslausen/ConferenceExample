using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;
using ConferenceExample.Session.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConferenceExample.Session.Application.UnitTests;

public class DummySessionRepository : ISessionRepository
{
    private readonly List<Domain.Entities.Session> _sessions = new();

    public DummySessionRepository()
    {
        for (int i = 1; i <= 100; i++)
        {
            _sessions.Add(new Domain.Entities.Session(
                new SessionId(i),
                new SessionTitle($"Demo Session {i}"),
                new SpeakerId(i),
                new List<SessionTag> { new SessionTag($"tag{i}"), new SessionTag($"tag{i + 1}") },
                new SessionTypeId(i % 5 + 1),
                new Abstract($"This is demo session {i}."),
                new ConferenceId(i % 3 + 1)
            ));
        }
    }

    public Task<IReadOnlyList<Domain.Entities.Session>> GetSessions(ConferenceId conferenceId)
    {
        var sessions = _sessions.Where(s => s.ConferenceId == conferenceId).ToList();
        return Task.FromResult((IReadOnlyList<Domain.Entities.Session>)sessions);
    }

    public Task Save(Domain.Entities.Session session)
    {
        _sessions.Add(session);
        return Task.CompletedTask;
    }
}