using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using Assembly = System.Reflection.Assembly;

namespace ConferenceExample.ArchitectureTests;

public abstract class ArchitectureTest
{
    // Conference
    protected static Assembly ConferenceApplication => typeof(Conference.Application.Class1).Assembly;
    protected static Assembly ConferenceDomain => typeof(Conference.Domain.ValueObjects.Room).Assembly;
    protected static Assembly ConferencePersistence => typeof(Conference.Persistence.Class1).Assembly;
    
    protected static readonly Assembly[] ConferenceAssemblies = [ConferenceApplication, ConferenceDomain, ConferencePersistence];
    
    // Session
    protected static Assembly SessionApplication => typeof(Session.Application.Class1).Assembly;
    protected static Assembly SessionDomain => typeof(Session.Domain.ValueObjects.Abstract).Assembly;
    protected static Assembly SessionPersistence => typeof(Session.Persistence.Class1).Assembly;
    
    protected static readonly Assembly[] SessionAssemblies = [SessionApplication, SessionDomain, SessionPersistence];
    
    // Persistence
    protected static Assembly Persistence => typeof(Persistence.Class1).Assembly;

    protected static readonly Assembly[] PersistenceAssemblies = [Persistence];    

    // API
    protected static Assembly Api => typeof(ConferenceExample.API.Test).Assembly;
    
    protected static readonly Assembly[] ApiAssemblies = [Api];
    
    protected static readonly Architecture Architecture = new ArchLoader().LoadAssembliesIncludingDependencies(
        [..ConferenceAssemblies, ..SessionAssemblies, ..PersistenceAssemblies, ..ApiAssemblies]).Build();
}