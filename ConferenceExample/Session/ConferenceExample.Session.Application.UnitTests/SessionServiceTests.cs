namespace ConferenceExample.Session.Application.UnitTests;

using ConferenceExample.Session.Application.Dtos;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects.Ids;
using NSubstitute;
using Shouldly;

public class SessionServiceTests
{
    [Fact]
    public async Task SubmitSession_ValidDto_SavesSessionInRepository()
    {
        // Arrange
        var sessionRepositoryMock = Substitute.For<ISessionRepository>();
        var databaseContextStub = Substitute.For<IDatabaseContext>();
        var idGeneratorStub = Substitute.For<IIdGenerator>();
        var sessionId = new SessionId(42);
        idGeneratorStub.New<SessionId>().Returns(sessionId);
        
        var sut = new SessionService(sessionRepositoryMock, databaseContextStub, idGeneratorStub);
        
        var submitSessionDto = new SubmitSessionDto
        {
            Title = "Test Session",
            Abstract = "Test Abstract",
            ConferenceId = 1,
            SpeakerId = 2,
            Tags = new List<string> { "Tag1", "Tag2" },
            SessionTypeId = 3
        };

        // Act
        await sut.SubmitSession(submitSessionDto);

        // Assert
        await sessionRepositoryMock.Received(1).Save(Arg.Any<Domain.Entities.Session>());
    }

    [Fact]
    public async Task SubmitSession_ValidDto_SavesChangesInDatabase()
    {
        // Arrange
        var sessionRepositoryStub = Substitute.For<ISessionRepository>();
        var databaseContextMock = Substitute.For<IDatabaseContext>();
        var idGeneratorStub = Substitute.For<IIdGenerator>();
        var sessionId = new SessionId(42);
        idGeneratorStub.New<SessionId>().Returns(sessionId);
        
        var sut = new SessionService(sessionRepositoryStub, databaseContextMock, idGeneratorStub);
        
        var submitSessionDto = new SubmitSessionDto
        {
            Title = "Test Session",
            Abstract = "Test Abstract",
            ConferenceId = 1,
            SpeakerId = 2,
            Tags = new List<string> { "Tag1", "Tag2" },
            SessionTypeId = 3
        };

        // Act
        await sut.SubmitSession(submitSessionDto);

        // Assert
        await databaseContextMock.Received(1).SaveChanges();
    }

    [Fact]
    public async Task SubmitSession_ValidDto_CreatesSessionWithCorrectValues()
    {
        // Arrange
        var sessionRepositoryStub = Substitute.For<ISessionRepository>();
        var databaseContextStub = Substitute.For<IDatabaseContext>();
        var idGeneratorStub = Substitute.For<IIdGenerator>();
        var sessionId = new SessionId(42);
        idGeneratorStub.New<SessionId>().Returns(sessionId);
        Domain.Entities.Session? capturedSession = null;
        await sessionRepositoryStub.Save(Arg.Do<Domain.Entities.Session>(s => capturedSession = s));
        
        var sut = new SessionService(sessionRepositoryStub, databaseContextStub, idGeneratorStub);
        
        var submitSessionDto = new SubmitSessionDto
        {
            Title = "Test Session",
            Abstract = "Test Abstract",
            ConferenceId = 1,
            SpeakerId = 2,
            Tags = ["Tag1", "Tag2"],
            SessionTypeId = 3
        };

        // Act
        await sut.SubmitSession(submitSessionDto);

        // Assert
        capturedSession.ShouldNotBeNull();
        capturedSession.Id.Value.ShouldBe(sessionId.Value);
        capturedSession.Title.Title.ShouldBe(submitSessionDto.Title);
        capturedSession.Abstract.Content.ShouldBe(submitSessionDto.Abstract);
        capturedSession.ConferenceId.Value.ShouldBe(submitSessionDto.ConferenceId);
        capturedSession.SpeakerId.Value.ShouldBe(submitSessionDto.SpeakerId);
        capturedSession.SessionTypeId.Value.ShouldBe(submitSessionDto.SessionTypeId);
        capturedSession.Tags.Select(t => t.Tag).ShouldBe(submitSessionDto.Tags);
        capturedSession.Status.ShouldBe(Domain.SessionStatus.Submitted);
    }
}