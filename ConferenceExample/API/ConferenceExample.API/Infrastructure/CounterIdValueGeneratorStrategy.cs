using ConferenceExample.Conference.Application;

namespace ConferenceExample.API.Infrastructure;

public class CounterIdValueGeneratorStrategy : IIdValueGeneratorStrategy, Session.Application.IIdValueGeneratorStrategy
{
    private static long _counter;
    
    long IIdValueGeneratorStrategy.GenerateIdValue()
    {
        return _counter++;
    }

    long Session.Application.IIdValueGeneratorStrategy.GenerateIdValue()
    {
        return _counter++;
    }
}