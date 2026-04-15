namespace ConferenceExample.IntegrationTests.Infrastructure;

/// <summary>
/// Defines a test collection to ensure integration tests run serially.
/// This prevents race conditions and shared state issues when tests access the same MongoDB container.
/// </summary>
[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestCollection { }
