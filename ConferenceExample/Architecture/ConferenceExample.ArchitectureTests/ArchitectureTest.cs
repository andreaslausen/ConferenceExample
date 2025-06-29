﻿using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ConferenceExample.API;
using ConferenceExample.Conference.Domain.Entities;
using Assembly = System.Reflection.Assembly;

namespace ConferenceExample.ArchitectureTests;

public abstract class ArchitectureTest
{
    // Conference
    protected static Assembly ConferenceApplication => typeof(Conference.Application.CreateConferenceDto).Assembly;
    protected static Assembly ConferenceDomain => typeof(Room).Assembly;
    protected static Assembly ConferencePersistence => typeof(Conference.Persistence.Class1).Assembly;

    protected static readonly Assembly[] ConferenceAssemblies = [ConferenceApplication, ConferenceDomain, ConferencePersistence];

    // Session
    protected static Assembly SessionApplication => typeof(Session.Application.SessionService).Assembly;
    protected static Assembly SessionDomain => typeof(Session.Domain.ValueObjects.Abstract).Assembly;
    protected static Assembly SessionPersistence => typeof(Session.Persistence.SessionRepository).Assembly;

    protected static readonly Assembly[] SessionAssemblies = [SessionApplication, SessionDomain, SessionPersistence];

    // Persistence
    protected static Assembly Persistence => typeof(Persistence.IDatabaseContext).Assembly;

    protected static readonly Assembly[] PersistenceAssemblies = [Persistence];

    // API
    protected static Assembly Api => typeof(DatabaseContext).Assembly;

    protected static readonly Assembly[] ApiAssemblies = [Api];

    protected static readonly Assembly[] AllAssemblies = [..ConferenceAssemblies, ..SessionAssemblies, ..PersistenceAssemblies, ..ApiAssemblies];

    protected static readonly Architecture Architecture = new ArchLoader().LoadAssembliesIncludingDependencies(
        AllAssemblies)
        .Build();
}