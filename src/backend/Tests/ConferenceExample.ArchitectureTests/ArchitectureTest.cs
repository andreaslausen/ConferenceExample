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
    protected static readonly Assembly[] ConferenceTestAssemblies = [ConferenceDomainUnitTests];

    // Talk Test Assemblies
    protected static Assembly TalkDomainUnitTests =>
        typeof(Talk.Domain.UnitTests.AbstractTests).Assembly;
    protected static readonly Assembly[] TalkTestAssemblies = [TalkDomainUnitTests];

    // Acceptance Tests (one suite for the whole API — bounded contexts aren't visible at the
    // HTTP level, so this isn't split per context like the Domain unit tests above)
    protected static Assembly AcceptanceTests =>
        typeof(ConferenceExample.AcceptanceTests.SetupTestDependencies).Assembly;
    protected static readonly Assembly[] AcceptanceTestAssemblies = [AcceptanceTests];

    protected static readonly Assembly[] AllTestAssemblies =
    [
        .. ConferenceTestAssemblies,
        .. TalkTestAssemblies,
        .. AcceptanceTestAssemblies,
    ];

    // Separate architecture for test assembly dependency checks.
    // Kept separate from Architecture to prevent test code from affecting production rules (e.g. ClassRules).
    protected static readonly Architecture TestArchitecture = new ArchLoader()
        .LoadAssembliesIncludingDependencies([.. AllAssemblies, .. AllTestAssemblies])
        .Build();
}
