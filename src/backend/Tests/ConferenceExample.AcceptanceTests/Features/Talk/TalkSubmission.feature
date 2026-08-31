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

  Scenario: The organizer can view a submitted talk
    Given a conference exists
    When a speaker submits a talk titled "Introduction to DDD" with abstract "An overview of Domain-Driven Design"
    Then the organizer can view the talk

  Scenario: An unrelated user cannot view a submitted talk
    Given a conference exists
    When a speaker submits a talk titled "Introduction to DDD" with abstract "An overview of Domain-Driven Design"
    Then an unrelated user cannot view the talk

  Scenario: Submitting a talk for a nonexistent conference is rejected
    When a speaker submits a talk for a nonexistent conference
    Then the submission is rejected with status 404

  Scenario: Submitting a talk while the conference is not accepting submissions is rejected
    Given a conference exists that is not yet accepting submissions
    When a speaker submits a talk titled "Introduction to DDD" with abstract "An overview of Domain-Driven Design"
    Then the submission is rejected with status 409

  Scenario: Submitting a talk with a title that is too long is rejected
    Given a conference exists
    When a speaker submits a talk titled "This title is far too long for a talk and exceeds the maximum of one hundred characters allowed by the system" with abstract "An overview of Domain-Driven Design"
    Then the submission is rejected with status 400
