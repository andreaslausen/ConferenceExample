using ConferenceExample.Session.Application.Dtos;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects.Ids;
using FluentAssertions;
using Xunit;

namespace ConferenceExample.Session.Application.UnitTests;

public class SessionServiceTests
{
    private class FakeSessionRepository : ISessionRepository
    {
        public Domain.Entities.Session SavedSession { get; private set; }

        public Task Save(Domain.Entities.Session session)
        {
            SavedSession = session;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Domain.Entities.Session>> GetSessions(ConferenceId conferenceId)
        {
            throw new NotImplementedException();
        }
    }

    private class FakeDatabaseContext : IDatabaseContext
    {
        public bool SaveChangesCalled { get; private set; }

        public Task SaveChanges()
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }
    }

    private class FakeIdGenerator : IIdGenerator
    {
        private readonly SessionId _sessionId;

        public FakeIdGenerator(SessionId sessionId)
        {
            _sessionId = sessionId;
        }

        public T New<T>() where T : IId
        {
            return (T)(object)_sessionId;
        }
    }

    [Fact]
    public async Task SubmitSession_ShouldCallSave()
    {
        // Arrange
        var sessionRepository = new FakeSessionRepository();
        var databaseContext = new FakeDatabaseContext();
        var sessionId = new SessionId(5);
        var idGenerator = new FakeIdGenerator(sessionId);
        var sessionService = new SessionService(sessionRepository, databaseContext, idGenerator);

        var submitSessionDto = new SubmitSessionDto
        {
            Title = "Test Session",
            SpeakerId = 2,
            Tags = new List<string> { "tag1", "tag2" },
            SessionTypeId = 6,
            Abstract = "Test Abstract",
            ConferenceId = 9
        };

        // Act
        await sessionService.SubmitSession(submitSessionDto);

        // Assert
        sessionRepository.SavedSession.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitSession_ShouldCallSaveChanges()
    {
        // Arrange
        var sessionRepository = new FakeSessionRepository();
        var databaseContext = new FakeDatabaseContext();
        var sessionId = new SessionId(5);
        var idGenerator = new FakeIdGenerator(sessionId);
        var sessionService = new SessionService(sessionRepository, databaseContext, idGenerator);

        var submitSessionDto = new SubmitSessionDto
        {
            Title = "Test Session",
            SpeakerId = 2,
            Tags = new List<string> { "tag1", "tag2" },
            SessionTypeId = 6,
            Abstract = "Test Abstract",
            ConferenceId = 9
        };

        // Act
        await sessionService.SubmitSession(submitSessionDto);

        // Assert
        databaseContext.SaveChangesCalled.Should().BeTrue();
    }
}