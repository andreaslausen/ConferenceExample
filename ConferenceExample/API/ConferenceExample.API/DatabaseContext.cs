using ConferenceExample.Persistence;
using ConferenceExample.Persistence.Model;

namespace ConferenceExample.API;

public class DatabaseContext : IDatabaseContext, Session.Application.IDatabaseContext
{
    public List<Persistence.Model.Session> Sessions { get; } = [];
    public List<Speaker> Speakers { get; } = [];
    public List<Persistence.Model.Conference> Conferences { get; } = [];

    public Task SaveChanges()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}