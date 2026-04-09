using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ConferenceExample.API;
using ConferenceExample.Conference.Domain.Entities;
using Assembly = System.Reflection.Assembly;

namespace ConferenceExample.ArchitectureTests;

public abstract class ArchitectureTest
{
    // Conference
    protected static Assembly ConferenceApplication =>
        typeof(Conference.Application.CreateConferenceDto).Assembly;
    protected static Assembly ConferenceDomain => typeof(Room).Assembly;
    protected static Assembly ConferencePersistence =>
        typeof(Conference.Persistence.ConferenceRepository).Assembly;

    protected static readonly Assembly[] ConferenceAssemblies =
    [
        ConferenceApplication,
        ConferenceDomain,
        ConferencePersistence,
    ];

    // Talk
    protected static Assembly TalkApplication => typeof(Talk.Application.TalkService).Assembly;
    protected static Assembly TalkDomain => typeof(Talk.Domain.ValueObjects.Abstract).Assembly;
    protected static Assembly TalkPersistence => typeof(Talk.Persistence.TalkRepository).Assembly;

    protected static readonly Assembly[] TalkAssemblies =
    [
        TalkApplication,
        TalkDomain,
        TalkPersistence,
    ];

    // EventStore
    protected static Assembly EventStore =>
        typeof(ConferenceExample.EventStore.IEventStore).Assembly;

    protected static readonly Assembly[] EventStoreAssemblies = [EventStore];

    // API
    protected static Assembly Api =>
        typeof(ConferenceExample.API.Extensions.ServiceCollectionExtensions).Assembly;

    protected static readonly Assembly[] ApiAssemblies = [Api];

    protected static readonly Assembly[] AllAssemblies =
    [
        .. ConferenceAssemblies,
        .. TalkAssemblies,
        .. EventStoreAssemblies,
        .. ApiAssemblies,
    ];

    protected static readonly Architecture Architecture = new ArchLoader()
        .LoadAssembliesIncludingDependencies(AllAssemblies)
        .Build();
}
