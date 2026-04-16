#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT=$(git rev-parse --show-toplevel)
cd "$REPO_ROOT"

echo "🧬 Running Mutation Testing with Stryker.NET"
echo "=============================================="
echo ""

# Define all unit test projects
UNIT_TEST_PROJECTS=(
  "src/backend/Conference/ConferenceExample.Conference.Domain.UnitTests"
  "src/backend/Conference/ConferenceExample.Conference.Application.UnitTests"
  "src/backend/Conference/ConferenceExample.Conference.Persistence.UnitTests"
  "src/backend/Talk/ConferenceExample.Talk.Domain.UnitTests"
  "src/backend/Talk/ConferenceExample.Talk.Application.UnitTests"
  "src/backend/Talk/ConferenceExample.Talk.Persistence.UnitTests"
  "src/backend/EventStore/ConferenceExample.EventStore.UnitTests"
  "src/backend/Infrastructure/ConferenceExample.EventStore.UnitTests"
  "src/backend/Infrastructure/ConferenceExample.Authentication.UnitTests"
)

FAILED_PROJECTS=()
PASSED_PROJECTS=()

for PROJECT in "${UNIT_TEST_PROJECTS[@]}"; do
  echo ""
  echo "──────────────────────────────────────────────────────────────"
  echo "Running mutation tests for: $(basename "$PROJECT")"
  echo "──────────────────────────────────────────────────────────────"
  echo ""

  cd "$REPO_ROOT/$PROJECT"

  if dotnet stryker --config-file "$REPO_ROOT/stryker-config.json"; then
    PASSED_PROJECTS+=("$PROJECT")
    echo "✅ PASSED: $(basename "$PROJECT")"
  else
    FAILED_PROJECTS+=("$PROJECT")
    echo "❌ FAILED: $(basename "$PROJECT")"
  fi

  cd "$REPO_ROOT"
done

echo ""
echo "=============================================="
echo "📊 Mutation Testing Summary"
echo "=============================================="
echo ""
echo "Passed: ${#PASSED_PROJECTS[@]}/${#UNIT_TEST_PROJECTS[@]}"
echo "Failed: ${#FAILED_PROJECTS[@]}/${#UNIT_TEST_PROJECTS[@]}"
echo ""

if [ ${#PASSED_PROJECTS[@]} -gt 0 ]; then
  echo "✅ Passed Projects:"
  for PROJECT in "${PASSED_PROJECTS[@]}"; do
    echo "   - $(basename "$PROJECT")"
  done
  echo ""
fi

if [ ${#FAILED_PROJECTS[@]} -gt 0 ]; then
  echo "❌ Failed Projects:"
  for PROJECT in "${FAILED_PROJECTS[@]}"; do
    echo "   - $(basename "$PROJECT")"
  done
  echo ""
  echo "Mutation score threshold of 100% was not met for the above projects."
  exit 1
fi

echo "🎉 All mutation tests passed with 100% mutation score!"
