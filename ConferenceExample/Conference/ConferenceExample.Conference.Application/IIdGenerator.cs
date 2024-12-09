using ConferenceExample.Conference.Domain.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application;

public interface IIdGenerator
{
    T New<T>() where T : IId;
}