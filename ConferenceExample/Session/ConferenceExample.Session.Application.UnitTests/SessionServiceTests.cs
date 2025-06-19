using NSubstitute;
using ConferenceExample.Session.Application.Dtos;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Application.UnitTests;

public class SessionServiceTests
{
    [Fact]
    public async Task SubmitSession_CallsSessionRepositorySave_MethodCalled()
    {
        // Arrange
        var sessionRepository = Substitute.For<ISessionRepository>();
        var databaseContext = Substitute.For<IDatabaseContext>();
        var service = new SessionService(sessionRepository, databaseContext);
        var dto = CreateSubmitSessionDto();

        // Act
        await service.SubmitSession(dto);

        // Assert
        await sessionRepository.Received(1).Save(Arg.Any<Domain.Entities.Session>());
    }

    [Fact]
    public async Task SubmitSession_CallsDatabaseContextSaveChanges_MethodCalled()
    {
        // Arrange
        var sessionRepository = Substitute.For<ISessionRepository>();
        var databaseContext = Substitute.For<IDatabaseContext>();
        var service = new SessionService(sessionRepository, databaseContext);
        var dto = CreateSubmitSessionDto();

        // Act
        await service.SubmitSession(dto);

        // Assert
        await databaseContext.Received(1).SaveChanges();
    }

    private SubmitSessionDto CreateSubmitSessionDto()
    {
        return new SubmitSessionDto
        {
            Title = "Test Title",
            SpeakerId = GuidV7.NewGuid(),
            Tags = new List<string> { "tag1", "tag2" },
            SessionTypeId = GuidV7.NewGuid(),
            Abstract = "Test Abstract",
            ConferenceId = GuidV7.NewGuid()
        };
    }
}
