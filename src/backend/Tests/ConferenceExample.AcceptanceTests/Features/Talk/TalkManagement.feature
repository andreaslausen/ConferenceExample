Feature: Talk Management

  A speaker creates and maintains their talks independently of any conference.

  Scenario: Create a talk
    Given a speaker is registered
    And the speaker has a profile
    When the speaker creates a talk titled "Introduction to DDD" with abstract "An overview of Domain-Driven Design"
    Then the talk request succeeds with status 201
    And the talk is stored titled "Introduction to DDD" with abstract "An overview of Domain-Driven Design"

  Scenario: Create a talk with tags
    Given a speaker is registered
    And the speaker has a profile
    When the speaker creates a talk titled "Event Sourcing in Practice" with abstract "Learn about event sourcing" tagged "Architecture" and "CQRS"
    Then the talk is stored titled "Event Sourcing in Practice" with abstract "Learn about event sourcing"
    And the talk has the tag "Architecture"
    And the talk has the tag "CQRS"

  Scenario: Creating a talk requires a speaker profile
    Given a speaker is registered
    When the speaker creates a talk titled "Introduction to DDD" with abstract "An overview of Domain-Driven Design"
    Then the talk request is rejected with status 404

  Scenario: Edit a talk
    Given a speaker is registered
    And the speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    When the speaker renames the talk to "Introduction to Domain-Driven Design"
    Then the talk is stored titled "Introduction to Domain-Driven Design" with abstract "An overview of Domain-Driven Design"

  Scenario: Delete a talk
    Given a speaker is registered
    And the speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    When the speaker deletes the talk
    Then the talk request succeeds with status 204
    And the talk is gone

  Scenario: Creating a talk with a title that is too long is rejected
    Given a speaker is registered
    And the speaker has a profile
    When the speaker creates a talk titled "This title is far too long for a talk and exceeds the maximum of one hundred characters allowed by the system" with abstract "An overview of Domain-Driven Design"
    Then the talk request is rejected with status 400

  Scenario: A talk belongs to its speaker alone
    Given a speaker is registered
    And the speaker has a profile
    And another speaker is registered
    And the other speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    Then the other speaker cannot view the talk

  Scenario: Another speaker cannot delete someone else's talk
    Given a speaker is registered
    And the speaker has a profile
    And another speaker is registered
    And the other speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    When the other speaker tries to delete the talk
    Then the talk request is rejected with status 403
