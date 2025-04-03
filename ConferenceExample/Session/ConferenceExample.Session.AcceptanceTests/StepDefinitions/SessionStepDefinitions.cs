using System.Collections.Generic;
using System.Threading.Tasks;
using ConferenceExample.Session.Application;
using ConferenceExample.Session.Application.Dtos;
using ConferenceExample.Session.Domain;
using Shouldly;
using Reqnroll;
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
        sessions.Count.ShouldBe(1);
        sessions[0].Title.ShouldBe("Besser testen mit Akzeptanztests");
        sessions[0].Abstract.ShouldBe("Akzeptanztests in .NET mit Reqnroll");
        sessions[0].Tags.ShouldBe(new List<string> { ".NET", "Testing" });
        sessions[0].SessionTypeId.ShouldBe(1);
        sessions[0].ConferenceId.ShouldBe(_conferenceId);
        sessions[0].SpeakerId.ShouldBe(_speakerId);
        sessions[0].SessionStatus.ShouldBe((int)SessionStatus.Submitted);
    }

    private class SessionDataFromTable
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public List<string> Tags { get; set; }
        public long SessionTypeId { get; set; }
    }
}