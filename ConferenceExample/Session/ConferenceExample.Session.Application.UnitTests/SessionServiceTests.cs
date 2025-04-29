using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConferenceExample.Session.Application;
using ConferenceExample.Session.Application.Dtos;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;
using NSubstitute;
using Xunit;

namespace ConferenceExample.Session.Application.UnitTests;

public class SessionServiceTests
{
    [Fact]
    public async Task SubmitSession_ValidInput_CallsIdGenerator()
    {
        // Arrange
        var sessionRepository = Substitute.For<ISessionRepository>();
        var databaseContext = Substitute.For<IDatabaseContext>();
        var idGenerator = Substitute.For<IIdGenerator>();
        var sessionService = new SessionService(sessionRepository, databaseContext, idGenerator);
        var submitSessionDto = new SubmitSessionDto
        {
            Title = "Test Title",
            Abstract = "Test Abstract",
            ConferenceId = 1,
            SpeakerId = 2,
            Tags = new List<string> { "Tag1", "Tag2" },
            SessionTypeId = 3
        };

        // Act
        await sessionService.SubmitSession(submitSessionDto);

        // Assert
        idGenerator.Received(1).New<SessionId>();
    }

    [Fact]
    public async Task SubmitSession_ValidInput_CallsSessionRepositorySave()
    {
        // Arrange
        var sessionRepository = Substitute.For<ISessionRepository>();
        var databaseContext = Substitute.For<IDatabaseContext>();
        var idGenerator = Substitute.For<IIdGenerator>();
        idGenerator.New<SessionId>().Returns(new SessionId(1));
        var sessionService = new SessionService(sessionRepository, databaseContext, idGenerator);
        var submitSessionDto = new SubmitSessionDto
        {
            Title = "Test Title",
            Abstract = "Test Abstract",
            ConferenceId = 1,
            SpeakerId = 2,
            Tags = new List<string> { "Tag1", "Tag2" },
            SessionTypeId = 3
        };

        // Act
        await sessionService.SubmitSession(submitSessionDto);

        // Assert
        await sessionRepository.Received(1).Save(Arg.Any<Domain.Entities.Session>());
    }

    [Fact]
    public async Task SubmitSession_ValidInput_CallsDatabaseContextSaveChanges()
    {
        // Arrange
        var sessionRepository = Substitute.For<ISessionRepository>();
        var databaseContext = Substitute.For<IDatabaseContext>();
        var idGenerator = Substitute.For<IIdGenerator>();
        idGenerator.New<SessionId>().Returns(new SessionId(1));
        var sessionService = new SessionService(sessionRepository, databaseContext, idGenerator);
        var submitSessionDto = new SubmitSessionDto
        {
            Title = "Test Title",
            Abstract = "Test Abstract",
            ConferenceId = 1,
            SpeakerId = 2,
            Tags = new List<string> { "Tag1", "Tag2" },
            SessionTypeId = 3
        };

        // Act
        await sessionService.SubmitSession(submitSessionDto);

        // Assert
        await databaseContext.Received(1).SaveChanges();
    }
}