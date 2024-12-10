Feature: Session

As a speaker I want to submit a session so that I can speak at the conference

@mytag
Scenario: Submit session
	Given a conference with the id 27352713173
	And a speaker with the id 1284537291
	When he submits a session
| Title	  | Description         |
| My session | My session description |
	Then the session should be submitted