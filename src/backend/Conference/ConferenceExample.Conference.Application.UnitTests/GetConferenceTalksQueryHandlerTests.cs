using ConferenceExample.Authentication;
using ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Application.GetConferenceTalks;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using AuthGuidV7 = ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids.GuidV7;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;
using ConferenceGuidV7 = ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids.GuidV7;

namespace ConferenceExample.Conference.Application.UnitTests;

public class GetConferenceTalksQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidQuery_ReturnsTalks()
    {
        // Arrange
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var talkSummaryRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        var conference = CreateValidConference();
        var currentUserService = CreateMockCurrentUserService(conference.OrganizerId);

        var talkId = Guid.NewGuid();
        var speakerId = Guid.NewGuid();
        var talkTypeId = Guid.NewGuid();
        var talks = new List<ConferenceTalkReadModel>
        {
            new(
                talkId,
                "Introduction to DDD",
                "Learn about Domain-Driven Design",
                speakerId,
                "Jane",
                "Doe",
                "Speaker bio",
                "Submitted",
                new List<string> { "DDD", "Architecture" },
                talkTypeId,
                null,
                null,
                null,
                null
            ),
        };

        conferenceRepository.GetById(Arg.Any<ConferenceId>()).Returns(conference);
        talkSummaryRepository.GetByConferenceId(Arg.Any<ConferenceId>()).Returns(talks);

        var handler = new GetConferenceTalksQueryHandler(
            talkSummaryRepository,
            conferenceRepository,
            currentUserService
        );
        var query = new GetConferenceTalksQuery(conference.Id.Value);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(talkId, result[0].Id);
        Assert.Equal("Introduction to DDD", result[0].Title);
        Assert.Equal("Learn about Domain-Driven Design", result[0].Abstract);
        Assert.Equal(speakerId, result[0].SpeakerId);
        Assert.Equal("Submitted", result[0].Status);
        Assert.Equal(2, result[0].Tags.Count);
        Assert.Contains("DDD", result[0].Tags);
        Assert.Contains("Architecture", result[0].Tags);
    }

    [Fact]
    public async Task Handle_ConferenceWithNoTalks_ReturnsEmptyList()
    {
        // Arrange
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var talkSummaryRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        var conference = CreateValidConference();
        var currentUserService = CreateMockCurrentUserService(conference.OrganizerId);

        conferenceRepository.GetById(Arg.Any<ConferenceId>()).Returns(conference);
        talkSummaryRepository
            .GetByConferenceId(Arg.Any<ConferenceId>())
            .Returns(new List<ConferenceTalkReadModel>());

        var handler = new GetConferenceTalksQueryHandler(
            talkSummaryRepository,
            conferenceRepository,
            currentUserService
        );
        var query = new GetConferenceTalksQuery(conference.Id.Value);

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
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var talkSummaryRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        var currentUserService = CreateMockCurrentUserService();
        var handler = new GetConferenceTalksQueryHandler(
            talkSummaryRepository,
            conferenceRepository,
            currentUserService
        );
        var query = new GetConferenceTalksQuery(ConferenceGuidV7.NewGuid());

        conferenceRepository
            .GetById(Arg.Any<ConferenceId>())
            .Throws(new InvalidOperationException("Conference not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query));
    }

    [Fact]
    public async Task Handle_InvalidGuidV7_ThrowsArgumentException()
    {
        // Arrange
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var talkSummaryRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        var currentUserService = CreateMockCurrentUserService();
        var handler = new GetConferenceTalksQueryHandler(
            talkSummaryRepository,
            conferenceRepository,
            currentUserService
        );
        var invalidGuid = Guid.NewGuid();
        var query = new GetConferenceTalksQuery(invalidGuid);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(query));
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var talkSummaryRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        var conference = CreateValidConference();
        var differentUserId = ConferenceGuidV7.NewGuid();
        var currentUserService = CreateMockCurrentUserService(new OrganizerId(differentUserId));

        conferenceRepository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var handler = new GetConferenceTalksQueryHandler(
            talkSummaryRepository,
            conferenceRepository,
            currentUserService
        );
        var query = new GetConferenceTalksQuery(conference.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(query));
    }

    [Fact]
    public async Task Handle_TalkRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var conferenceRepository = Substitute.For<IConferenceRepository>();
        var talkSummaryRepository = Substitute.For<IConferenceTalkReadModelRepository>();
        var conference = CreateValidConference();
        var currentUserService = CreateMockCurrentUserService(conference.OrganizerId);

        conferenceRepository.GetById(Arg.Any<ConferenceId>()).Returns(conference);
        talkSummaryRepository
            .GetByConferenceId(Arg.Any<ConferenceId>())
            .Throws(new InvalidOperationException("Database error"));

        var handler = new GetConferenceTalksQueryHandler(
            talkSummaryRepository,
            conferenceRepository,
            currentUserService
        );
        var query = new GetConferenceTalksQuery(conference.Id.Value);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query));
    }

    private static ICurrentUserService CreateMockCurrentUserService(OrganizerId? organizerId = null)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = organizerId?.Value.Value ?? ConferenceGuidV7.NewGuid();
        currentUserService.GetCurrentUserId().Returns(new UserId(new AuthGuidV7(userId)));
        return currentUserService;
    }

    private static ConferenceAggregate CreateValidConference()
    {
        var id = new ConferenceId(ConferenceGuidV7.NewGuid());
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
            new OrganizerId(ConferenceGuidV7.NewGuid())
        );
    }
}
