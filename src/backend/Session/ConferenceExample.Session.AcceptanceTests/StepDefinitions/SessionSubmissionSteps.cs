using ConferenceExample.Session.Application;
using ConferenceExample.Session.Application.Dtos;
using ConferenceExample.Session.Domain;
using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;
using Reqnroll;
using Xunit;
using SessionEntity = ConferenceExample.Session.Domain.Entities.Session;

namespace ConferenceExample.Session.AcceptanceTests.StepDefinitions;

[Binding]
public class SessionSubmissionSteps(
    ISessionService sessionService,
    ISessionRepository sessionRepository
)
{
    private Guid _conferenceId;
    private IReadOnlyList<SessionEntity> _sessions = [];

    [Given("a conference exists")]
    public void GivenAConferenceExists()
    {
        _conferenceId = Guid.CreateVersion7();
    }

    [When("a speaker submits a session titled {string} with abstract {string}")]
    public async Task WhenASpeakerSubmitsASession(string title, string @abstract)
    {
        await sessionService.SubmitSession(
            new SubmitSessionDto
            {
                Title = title,
                Abstract = @abstract,
                ConferenceId = _conferenceId,
                SpeakerId = Guid.CreateVersion7(),
                Tags = [],
                SessionTypeId = Guid.CreateVersion7(),
            }
        );
        _sessions = await sessionRepository.GetSessions(new ConferenceId(_conferenceId));
    }

    [When(
        "a speaker submits a session titled {string} with abstract {string} tagged {string} and {string}"
    )]
    public async Task WhenASpeakerSubmitsASessionWithTags(
        string title,
        string @abstract,
        string tag1,
        string tag2
    )
    {
        await sessionService.SubmitSession(
            new SubmitSessionDto
            {
                Title = title,
                Abstract = @abstract,
                ConferenceId = _conferenceId,
                SpeakerId = Guid.CreateVersion7(),
                Tags = [tag1, tag2],
                SessionTypeId = Guid.CreateVersion7(),
            }
        );
        _sessions = await sessionRepository.GetSessions(new ConferenceId(_conferenceId));
    }

    [Then("the session is stored with status Submitted")]
    public void ThenTheSessionIsStoredWithStatusSubmitted()
    {
        Assert.Single(_sessions);
        Assert.Equal(SessionStatus.Submitted, _sessions[0].Status);
    }

    [Then("the session has the tag {string}")]
    public void ThenTheSessionHasTheTag(string expectedTag)
    {
        Assert.Contains(_sessions[0].Tags, t => t.Tag == expectedTag);
    }
}
