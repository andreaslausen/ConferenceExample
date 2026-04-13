using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ConferenceExample.API;
using ConferenceExample.Conference.Domain.RoomManagement;
using Assembly = System.Reflection.Assembly;

namespace ConferenceExample.ArchitectureTests;

public abstract class ArchitectureTest
{
    // Conference
    protected static Assembly ConferenceApplication =>
        typeof(Conference.Application.CreateConference.CreateConferenceDto).Assembly;
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
    protected static Assembly TalkDomain => typeof(Talk.Domain.TalkManagement.Abstract).Assembly;
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

    // Authentication
    protected static Assembly Authentication =>
        typeof(ConferenceExample.Authentication.IAuthenticationService).Assembly;

    protected static readonly Assembly[] AuthenticationAssemblies = [Authentication];

    // API
    protected static Assembly Api =>
        typeof(ConferenceExample.API.Extensions.ServiceCollectionExtensions).Assembly;

    protected static readonly Assembly[] ApiAssemblies = [Api];

    protected static readonly Assembly[] AllAssemblies =
    [
        .. ConferenceAssemblies,
        .. TalkAssemblies,
        .. EventStoreAssemblies,
        .. AuthenticationAssemblies,
        .. ApiAssemblies,
    ];

    protected static readonly Architecture Architecture = new ArchLoader()
        .LoadAssembliesIncludingDependencies(AllAssemblies)
        .Build();

    // Conference Test Assemblies
    protected static Assembly ConferenceDomainUnitTests =>
        typeof(Conference.Domain.UnitTests.ConferenceTests).Assembly;
    protected static Assembly ConferenceApplicationUnitTests =>
        typeof(Conference.Application.UnitTests.ConferenceServiceTests).Assembly;
    protected static Assembly ConferencePersistenceUnitTests =>
        typeof(Conference.Persistence.UnitTests.ConferenceRepositoryTests).Assembly;
    protected static readonly Assembly[] ConferenceTestAssemblies =
    [
        ConferenceDomainUnitTests,
        ConferenceApplicationUnitTests,
        ConferencePersistenceUnitTests,
    ];

    // Talk Test Assemblies
    protected static Assembly TalkDomainUnitTests =>
        typeof(Talk.Domain.UnitTests.AbstractTests).Assembly;
    protected static Assembly TalkApplicationUnitTests =>
        typeof(Talk.Application.UnitTests.TalkServiceTests).Assembly;
    protected static Assembly TalkPersistenceUnitTests =>
        typeof(Talk.Persistence.UnitTests.TalkRepositoryTests).Assembly;
    protected static Assembly TalkAcceptanceTests =>
        typeof(Talk.AcceptanceTests.SetupTestDependencies).Assembly;

    protected static readonly Assembly[] TalkTestAssemblies =
    [
        TalkDomainUnitTests,
        TalkApplicationUnitTests,
        TalkPersistenceUnitTests,
        TalkAcceptanceTests,
    ];

    // EventStore Test Assemblies
    protected static Assembly EventStoreUnitTests =>
        typeof(EventStore.UnitTests.InMemoryEventBusTests).Assembly;

    protected static readonly Assembly[] EventStoreTestAssemblies = [EventStoreUnitTests];

    protected static readonly Assembly[] AllTestAssemblies =
    [
        .. ConferenceTestAssemblies,
        .. TalkTestAssemblies,
        .. EventStoreTestAssemblies,
    ];

    // Separate architecture for test assembly dependency checks.
    // Kept separate from Architecture to prevent test code from affecting production rules (e.g. ClassRules).
    protected static readonly Architecture TestArchitecture = new ArchLoader()
        .LoadAssembliesIncludingDependencies([.. AllAssemblies, .. AllTestAssemblies])
        .Build();
}
