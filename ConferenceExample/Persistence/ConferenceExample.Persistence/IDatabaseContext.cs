using ConferenceExample.Persistence.Model;

namespace ConferenceExample.Persistence;

public interface IDatabaseContext : IDisposable
{
    public List<Session> Sessions { get; }
    public List<Speaker> Speakers { get; }
    public List<Conference> Conferences { get; }
    
    Task SaveChanges();
}