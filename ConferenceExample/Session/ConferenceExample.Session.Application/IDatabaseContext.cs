namespace ConferenceExample.Session.Application;

public interface IDatabaseContext
{
    Task SaveChanges();
}