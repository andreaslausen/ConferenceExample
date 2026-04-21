using ConferenceExample.Authentication;
using ConferenceExample.Conference.Application.GetConferenceSchedule;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;

namespace ConferenceExample.Conference.Application.UnitTests;

public class GetConferenceScheduleQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidQuery_ReturnsSchedule()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var conference = CreateConferenceWithTalks();
        var organizerId = conference.OrganizerId.Value.Value;

        currentUserService
            .GetCurrentUserId()
            .Returns(
                new UserId(new Authentication.SharedKernel.ValueObjects.Ids.GuidV7(organizerId))
            );
        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var talkReadModels = new List<ConferenceTalkReadModel>
        {
            new(
                Guid.NewGuid(),
                "Test Talk",
                "Abstract",
                Guid.NewGuid(),
                "Jane",
                "Doe",
                "Bio",
                "Submitted",
                new List<string>(),
                Guid.NewGuid(),
                null,
                null,
                null,
                null
            ),
        };
        var talkReadModelRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        talkReadModelRepository.GetByConferenceId(Arg.Any<ConferenceId>()).Returns(talkReadModels);
        var handler = new GetConferenceScheduleQueryHandler(
            repository,
            talkReadModelRepository,
            currentUserService
        );
        var query = new GetConferenceScheduleQuery(conference.Id.Value);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Submitted", result[0].Status);
    }

    [Fact]
    public async Task Handle_ConferenceWithNoTalks_ReturnsEmptyList()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var conference = CreateValidConference();
        var organizerId = conference.OrganizerId.Value.Value;

        currentUserService
            .GetCurrentUserId()
            .Returns(
                new UserId(new Authentication.SharedKernel.ValueObjects.Ids.GuidV7(organizerId))
            );
        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var talkReadModelRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        talkReadModelRepository
            .GetByConferenceId(Arg.Any<ConferenceId>())
            .Returns(new List<ConferenceTalkReadModel>());
        var handler = new GetConferenceScheduleQueryHandler(
            repository,
            talkReadModelRepository,
            currentUserService
        );
        var query = new GetConferenceScheduleQuery(conference.Id.Value);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ConferenceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var talkReadModelRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        talkReadModelRepository
            .GetByConferenceId(Arg.Any<ConferenceId>())
            .Returns(new List<ConferenceTalkReadModel>());
        var handler = new GetConferenceScheduleQueryHandler(
            repository,
            talkReadModelRepository,
            currentUserService
        );
        var query = new GetConferenceScheduleQuery(GuidV7.NewGuid());

        repository
            .GetById(Arg.Any<ConferenceId>())
            .Throws(new InvalidOperationException("Conference not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query));
    }

    [Fact]
    public async Task Handle_InvalidGuidV7_ThrowsArgumentException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var talkReadModelRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        talkReadModelRepository
            .GetByConferenceId(Arg.Any<ConferenceId>())
            .Returns(new List<ConferenceTalkReadModel>());
        var handler = new GetConferenceScheduleQueryHandler(
            repository,
            talkReadModelRepository,
            currentUserService
        );
        var invalidGuid = Guid.NewGuid(); // Not a GuidV7
        var query = new GetConferenceScheduleQuery(invalidGuid);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(query));
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var talkReadModelRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        talkReadModelRepository
            .GetByConferenceId(Arg.Any<ConferenceId>())
            .Returns(new List<ConferenceTalkReadModel>());
        var handler = new GetConferenceScheduleQueryHandler(
            repository,
            talkReadModelRepository,
            currentUserService
        );
        var query = new GetConferenceScheduleQuery(GuidV7.NewGuid());

        repository
            .GetById(Arg.Any<ConferenceId>())
            .Throws(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query));
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var conference = CreateValidConference();
        var differentUserId = GuidV7.NewGuid();

        currentUserService
            .GetCurrentUserId()
            .Returns(
                new UserId(new Authentication.SharedKernel.ValueObjects.Ids.GuidV7(differentUserId))
            );
        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var talkReadModelRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        talkReadModelRepository
            .GetByConferenceId(Arg.Any<ConferenceId>())
            .Returns(new List<ConferenceTalkReadModel>());
        var handler = new GetConferenceScheduleQueryHandler(
            repository,
            talkReadModelRepository,
            currentUserService
        );
        var query = new GetConferenceScheduleQuery(conference.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(query));
    }

    private static ConferenceAggregate CreateValidConference()
    {
        var id = new ConferenceId(GuidV7.NewGuid());
        var name = new Text("Test Conference");
        var time = new Time(DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(32));
        var location = new Location(
            new Text("Test Venue"),
            new Address("123 Main St", "Springfield", "IL", "62701", "US")
        );

        return ConferenceAggregate.Create(
            id,
            name,
            time,
            location,
            new OrganizerId(GuidV7.NewGuid())
        );
    }

    private static ConferenceAggregate CreateConferenceWithTalks()
    {
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        return conference;
    }
}
