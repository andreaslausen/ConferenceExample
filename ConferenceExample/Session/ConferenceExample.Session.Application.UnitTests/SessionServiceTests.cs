using NSubstitute;
using ConferenceExample.Session.Application.Dtos;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Application.UnitTests;

public class SessionServiceTests
{
    [Fact]
    public async Task SubmitSession_CallsIdGeneratorNew_MethodCalled()
    {
        // Arrange
        var sessionRepository = Substitute.For<ISessionRepository>();
        var databaseContext = Substitute.For<IDatabaseContext>();
        var idGenerator = Substitute.For<IIdGenerator>();
        var service = new SessionService(sessionRepository, databaseContext, idGenerator);
        var dto = CreateSubmitSessionDto();

        // Act
        await service.SubmitSession(dto);

        // Assert
        idGenerator.Received(1).New<SessionId>();
    }

    [Fact]
    public async Task SubmitSession_CallsSessionRepositorySave_MethodCalled()
    {
        // Arrange
        var sessionRepository = Substitute.For<ISessionRepository>();
        var databaseContext = Substitute.For<IDatabaseContext>();
        var idGenerator = Substitute.For<IIdGenerator>();
        idGenerator.New<SessionId>().Returns(new SessionId(1));
        var service = new SessionService(sessionRepository, databaseContext, idGenerator);
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
        var idGenerator = Substitute.For<IIdGenerator>();
        idGenerator.New<SessionId>().Returns(new SessionId(1));
        var service = new SessionService(sessionRepository, databaseContext, idGenerator);
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
            SpeakerId = 1,
            Tags = new List<string> { "tag1", "tag2" },
            SessionTypeId = 1,
            Abstract = "Test Abstract",
            ConferenceId = 1
        };
    }
}
