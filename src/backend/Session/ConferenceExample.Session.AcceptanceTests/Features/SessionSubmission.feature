Feature: Session Submission

  Scenario: Submit a session proposal
    Given a conference exists
    When a speaker submits a session titled "Introduction to DDD" with abstract "An overview of Domain-Driven Design"
    Then the session is stored with status Submitted

  Scenario: Submit a session with tags
    Given a conference exists
    When a speaker submits a session titled "Event Sourcing in Practice" with abstract "Learn about event sourcing" tagged "Architecture" and "CQRS"
    Then the session is stored with status Submitted
    And the session has the tag "Architecture"
    And the session has the tag "CQRS"
