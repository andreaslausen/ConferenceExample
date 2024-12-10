using FluentAssertions;
using Reqnroll;

namespace ConferenceExample.Session.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class SessionStepDefinitions
{
    [Given(@"a conference with the id (.*)")]
    public void GivenAConferenceWithTheId(long conferenceId)
    {
        
    }

    [Given(@"a speaker with the id (.*)")]
    public void GivenASpeakerWithTheId(long speakerId)
    {
        
    }

    [When(@"he submits a session")]
    public void WhenHeSubmitsASession(DataTable table)
    {
        var test = table.CreateInstance<Test>();
    }

    class Test
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    [Then(@"the session should be submitted")]
    public void ThenTheSessionShouldBeSubmitted()
    {
        
    }
}