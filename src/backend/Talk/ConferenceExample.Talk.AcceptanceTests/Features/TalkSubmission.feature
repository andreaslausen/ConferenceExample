Feature: Talk Submission

  Scenario: Submit a talk proposal
    Given a conference exists
    When a speaker submits a talk titled "Introduction to DDD" with abstract "An overview of Domain-Driven Design"
    Then the talk is stored with status Submitted

  Scenario: Submit a talk with tags
    Given a conference exists
    When a speaker submits a talk titled "Event Sourcing in Practice" with abstract "Learn about event sourcing" tagged "Architecture" and "CQRS"
    Then the talk is stored with status Submitted
    And the talk has the tag "Architecture"
    And the talk has the tag "CQRS"
