Feature: Talk Submission

  A speaker submits an existing talk to a conference. The conference decides asynchronously
  whether it takes the submission into review.

  Scenario: Submit a talk to a conference
    Given an organizer is registered
    And a conference exists
    And a speaker is registered
    And the speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    When the speaker submits the talk to the conference
    Then the submission is accepted with status 202
    And the submission eventually has status Submitted
    And the submission shows the conference name "Test Conference"

  Scenario: The conference receives the talk with the speaker's details
    Given an organizer is registered
    And a conference exists
    And a speaker is registered
    And the speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    When the speaker submits the talk to the conference
    Then the submission eventually has status Submitted
    And the conference has the talk with the speaker name "Jane Doe"

  Scenario: Editing a talk after submitting it leaves the submission untouched
    Given an organizer is registered
    And a conference exists
    And a speaker is registered
    And the speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    When the speaker submits the talk to the conference
    Then the submission eventually has status Submitted
    When the speaker renames the talk to "Something Else Entirely"
    Then the submission still shows the title "Introduction to DDD"

  Scenario: Submitting to a nonexistent conference fails asynchronously
    Given a speaker is registered
    And the speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    When the speaker submits the talk to a nonexistent conference
    Then the submission is accepted with status 202
    And the submission eventually has status Failed

  Scenario: Submitting while the conference is not accepting submissions fails asynchronously
    Given an organizer is registered
    And a conference exists that is not yet accepting submissions
    And a speaker is registered
    And the speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    When the speaker submits the talk to the conference
    Then the submission is accepted with status 202
    And the submission eventually has status Failed

  Scenario: Submitting with a talk type the conference does not offer fails asynchronously
    Given an organizer is registered
    And a conference exists
    And a speaker is registered
    And the speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    When the speaker submits the talk with a talk type the conference does not offer
    Then the submission is accepted with status 202
    And the submission eventually has status Failed

  Scenario: The same talk cannot be submitted to the same conference twice
    Given an organizer is registered
    And a conference exists
    And a speaker is registered
    And the speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    When the speaker submits the talk to the conference
    Then the submission eventually has status Submitted
    When the speaker submits the talk to the conference again
    Then the submission is rejected with status 409
    And the talk has 1 submission

  Scenario: A deleted talk keeps the submissions the conference already received
    Given an organizer is registered
    And a conference exists
    And a speaker is registered
    And the speaker has a profile
    And the speaker has a talk titled "Introduction to DDD"
    When the speaker submits the talk to the conference
    Then the submission eventually has status Submitted
    When the speaker deletes the talk
    Then the conference has the talk with the speaker name "Jane Doe"
