﻿using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Application;

public class IdGenerator(IIdValueGeneratorStrategy idValueGeneratorStrategy) : IIdGenerator
{
    public T New<T>() where T : IId
    {
        return (T)_idGeneratorsFunctions[typeof(T)](idValueGeneratorStrategy.GenerateIdValue());
    }

    private readonly Dictionary<Type, Func<long, IId>> _idGeneratorsFunctions = new()
    {
        { typeof(ConferenceId), id => new ConferenceId(id) },
        { typeof(SessionId), id => new SessionId(id) },
        { typeof(SpeakerId), id => new SpeakerId(id) },
        { typeof(SessionTypeId), id => new SessionTypeId(id) }
    };
}