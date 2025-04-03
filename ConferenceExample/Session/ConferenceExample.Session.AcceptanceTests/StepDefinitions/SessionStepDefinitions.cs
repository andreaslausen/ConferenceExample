using System.Collections.Generic;
using System.Threading.Tasks;
using ConferenceExample.Session.Application;
using ConferenceExample.Session.Application.Dtos;
using ConferenceExample.Session.Domain;
using Reqnroll;
using Xunit;
using IDatabaseContext = ConferenceExample.Persistence.IDatabaseContext;

namespace ConferenceExample.Session.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class SessionStepDefinitions(IDatabaseContext databaseContext, ISessionService sessionService)
{
    private long _conferenceId;
    private long _speakerId;

    [Given(@"a conference with the id (.*)")]
    public void GivenAConferenceWithTheId(long conferenceId)
    {
        _conferenceId = conferenceId;
    }

    [Given(@"a speaker with the id (.*)")]
    public void GivenASpeakerWithTheId(long speakerId)
    {
        _speakerId = speakerId;
    }

    [When(@"he submits a session")]
    public async Task WhenHeSubmitsASession(DataTable table)
    {
        var sessionDataFromTable = table.CreateInstance<SessionDataFromTable>();
        await sessionService.SubmitSession(new SubmitSessionDto
        {
            Title = sessionDataFromTable.Title,
            Abstract = sessionDataFromTable.Abstract,
            Tags = sessionDataFromTable.Tags,
            SessionTypeId = sessionDataFromTable.SessionTypeId,
            ConferenceId = _conferenceId, 
            SpeakerId = _speakerId
        });
    }

    [Then(@"the session should be submitted")]
    public void ThenTheSessionShouldBeSubmitted()
    {
        var sessions = databaseContext.Sessions;
        Assert.Single(sessions);
        Assert.Equal("Besser testen mit Akzeptanztests", sessions[0].Title);
        Assert.Equal("Akzeptanztests in .NET mit Reqnroll", sessions[0].Abstract);
        Assert.Equal(new List<string> { ".NET", "Testing" }, sessions[0].Tags);
        Assert.Equal(1, sessions[0].SessionTypeId);
        Assert.Equal(_conferenceId, sessions[0].ConferenceId);
        Assert.Equal(_speakerId, sessions[0].SpeakerId);
        Assert.Equal((int)SessionStatus.Submitted, sessions[0].SessionStatus);
    }

    private class SessionDataFromTable
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public List<string> Tags { get; set; }
        public long SessionTypeId { get; set; }
    }
}