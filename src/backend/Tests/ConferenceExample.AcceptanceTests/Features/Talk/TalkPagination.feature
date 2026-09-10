Feature: Talk Pagination

  Scenario: Listing my talks returns only the requested page
    Given a speaker is registered
    And the speaker has a profile
    And the speaker has created 5 talks
    When the speaker requests their talks with page 1 and page size 2 once 5 talks are listed
    Then the response contains 2 talks
    And the total count is 5
    When the speaker requests their talks with page 3 and page size 2
    Then the response contains 1 talk
    And the total count is 5

  Scenario: Listing my talks with an out-of-range page returns no items but the correct total count
    Given a speaker is registered
    And the speaker has a profile
    And the speaker has created 5 talks
    When the speaker requests their talks with page 10 and page size 2 once 5 talks are listed
    Then the response contains 0 talks
    And the total count is 5

  Scenario: Requesting talks with a page number below 1 is rejected
    Given a speaker is registered
    And the speaker has a profile
    When the speaker requests their talks with page 0 and page size 2
    Then the pagination request is rejected with status 400

  Scenario: Requesting talks with a page size above the maximum is rejected
    Given a speaker is registered
    And the speaker has a profile
    When the speaker requests their talks with page 1 and page size 101
    Then the pagination request is rejected with status 400
