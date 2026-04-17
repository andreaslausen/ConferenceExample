using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ConferenceExample.ArchitectureTests;

/// <summary>
/// Architecture rules that enforce Event Sourcing and CQRS principles.
/// These rules ensure proper separation between Commands (write side) and Queries (read side).
/// </summary>
public class EventSourcingRules : ArchitectureTest
{
    /// <summary>
    /// Enforces that CommandHandlers never use ReadModel repositories.
    /// Commands must reconstruct state from the EventStore (Single Source of Truth).
    /// ReadModels are ONLY for Queries, never for Commands.
    /// </summary>
    [Fact]
    public void CommandHandlers_ShouldNotUseReadModelRepositories()
    {
        // Define what a CommandHandler is
        var commandHandlers = Types()
            .That()
            .ResideInAssembly(ConferenceApplication, TalkApplication)
            .And()
            .HaveNameEndingWith("CommandHandler");

        // Define what ReadModel repositories are
        // These are repositories that return DTOs/ReadModels instead of domain aggregates
        var readModelRepositories = Types()
            .That()
            .ResideInAssembly(ConferencePersistence, TalkPersistence)
            .And()
            .HaveNameEndingWith("ReadModelRepository");

        // Rule: CommandHandlers should never depend on ReadModel repositories
        var rule = commandHandlers
            .Should()
            .NotDependOnAny(readModelRepositories)
            .Because(
                "Commands must use the EventStore as the Single Source of Truth. "
                    + "ReadModels are projections for Queries only and should never be used for write operations."
            );

        rule.Check(Architecture);
    }

    /// <summary>
    /// Enforces that CommandHandlers never call methods on ReadModel repositories.
    /// This is a stricter check than dependency checking.
    /// </summary>
    [Fact]
    public void CommandHandlers_ShouldNotCallReadModelRepositories()
    {
        // Define what a CommandHandler is
        var commandHandlers = Types()
            .That()
            .ResideInAssembly(ConferenceApplication, TalkApplication)
            .And()
            .HaveNameEndingWith("CommandHandler");

        // Define ReadModel repository interfaces
        var readModelRepositoryInterfaces = Interfaces()
            .That()
            .ResideInAssembly(ConferenceDomain, TalkDomain)
            .And()
            .HaveNameEndingWith("ReadModelRepository");

        // Rule: CommandHandlers should never call ReadModel repository methods
        var rule = MethodMembers()
            .That()
            .AreDeclaredIn(commandHandlers)
            .Should()
            .NotCallAny(
                MethodMembers()
                    .That()
                    .ArePublic()
                    .And()
                    .AreDeclaredIn(readModelRepositoryInterfaces)
            )
            .Because(
                "Commands must reconstruct state from EventStore, not from ReadModels. "
                    + "ReadModels are for Queries only."
            )
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    /// <summary>
    /// Enforces that classes in Application layer that handle Commands
    /// do not depend on types that are clearly ReadModel DTOs.
    /// </summary>
    [Fact]
    public void CommandHandlers_ShouldNotDependOnReadModelDtos()
    {
        // Define what a CommandHandler is
        var commandHandlers = Types()
            .That()
            .ResideInAssembly(ConferenceApplication, TalkApplication)
            .And()
            .HaveNameEndingWith("CommandHandler");

        // Define ReadModel DTOs - these typically have "ReadModel" in their name
        var readModelDtos = Types()
            .That()
            .ResideInAssembly(ConferencePersistence, TalkPersistence)
            .And()
            .HaveNameContaining("ReadModel");

        // Rule: CommandHandlers should not depend on ReadModel DTOs
        var rule = commandHandlers
            .Should()
            .NotDependOnAny(readModelDtos)
            .Because(
                "Commands should work with domain aggregates from EventStore, not ReadModel DTOs."
            );

        rule.Check(Architecture);
    }
}
