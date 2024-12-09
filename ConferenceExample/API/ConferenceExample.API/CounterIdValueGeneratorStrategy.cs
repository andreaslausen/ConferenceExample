using ConferenceExample.Conference.Application;

namespace ConferenceExample.API;

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