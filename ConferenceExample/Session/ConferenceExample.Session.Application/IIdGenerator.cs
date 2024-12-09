using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Application;

public interface IIdGenerator
{
    T New<T>() where T : IId;
}