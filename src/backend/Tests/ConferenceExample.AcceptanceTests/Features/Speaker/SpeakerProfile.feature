Feature: Speaker Profile

  Scenario: A registered user creates their speaker profile
    Given a speaker is registered
    When the speaker creates a profile named "Jane" "Doe"
    Then the profile is stored for "Jane" "Doe"
    And the speaker profile id differs from the account id

  Scenario: A speaker updates their profile
    Given a speaker is registered
    And the speaker has a profile
    When the speaker renames their profile to "Janet" "Doe"
    Then the profile is stored for "Janet" "Doe"

  Scenario: A speaker cannot create a second profile
    Given a speaker is registered
    And the speaker has a profile
    When the speaker creates a second profile
    Then the profile request is rejected with status 409
