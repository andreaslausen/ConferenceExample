# Backend: Build, Test, Style

Solution: `src/backend/ConferenceExample.sln`.

## Build & restore

```bash
dotnet build
dotnet restore
```

## Test strategy

Two kinds of backend tests, nothing in between:

- **Unit tests — Domain layer only.** Every `*.Domain.UnitTests` project targets aggregates,
  entities, and value objects in isolation. **100% line coverage and a 100% mutation score
  are required** (see Mutation tests below). Application, Persistence, and Infrastructure
  code is *not* unit-tested in isolation — it's covered by the acceptance tests instead.
- **Acceptance tests — API level, Gherkin, over HTTP.** `.feature` files (Reqnroll) drive
  scenarios through the real REST API via `HttpClient` against a `WebApplicationFactory`
  (Testcontainers-backed MongoDB) — no in-process shortcuts to the application layer. `Then`
  steps that assert persisted state must check **both** paths, not just one: read the data back
  straight from the database (e.g. by resolving the read-model repository from the
  `WebApplicationFactory`'s DI container) to confirm it actually landed in MongoDB correctly,
  *and* read it back through the API's own GET endpoint to confirm the read path (routing,
  controller, DTO mapping) returns it correctly too — one can be right while the other is
  broken. These are the only tests exercising Application, Persistence, and Infrastructure
  code. One suite
  (`ConferenceExample.AcceptanceTests`) covers the whole API rather than one project per
  bounded context — there's a single Web API deployable, so the contexts aren't visible at
  the HTTP level anyway (a scenario in one context routinely needs another, e.g. submitting a
  talk needs a conference and a talk type to exist first). `.feature` files and step
  definitions are organized into subfolders per bounded context for readability
  (`Features/Speaker/`, `Features/Talk/`, `StepDefinitions/Talk/`, ...). Scenario state that
  crosses context folders (who is signed in, which conference/talk the steps are about) lives in
  `Infrastructure/ScenarioState.cs`, injected into each binding class — step classes must not
  reach into one another. Assertions on projected state poll via `Infrastructure/Eventually.cs`,
  because read models and cross-context submissions are eventually consistent. Preconditions such as user registration
  must be their own explicit `Given` step (e.g. `Given a speaker is registered`) — never a
  side effect hidden inside a step bound to unrelated Gherkin text (e.g. registering a user
  inside a `Given a conference exists` or `Then ... cannot view ...` step). A reader of the
  `.feature` file must be able to see every precondition without opening the step
  definitions.

Do not add new `*.Application.UnitTests` / `*.Persistence.UnitTests` projects, and don't add
scenarios to an acceptance suite that bypass HTTP — write an HTTP-driven Gherkin scenario
instead.

```bash
dotnet test
dotnet test --filter "FullyQualifiedName~Talk"   # single test class or namespace
```

Test projects today:

- `ConferenceExample.ArchitectureTests` — enforces layer dependency rules
- `*.Domain.UnitTests` — unit tests for domain logic only, one project per bounded context
- `ConferenceExample.AcceptanceTests` — the one Gherkin acceptance suite for the whole API
  (currently covers speaker profiles, talk management, talk submission and pagination; add
  scenarios for the other contexts the same way)

```bash
# run just the acceptance suite (starts a MongoDB Testcontainer, needs Docker)
dotnet test src/backend/Tests/ConferenceExample.AcceptanceTests
```

Tools:

- **xUnit** — assertion framework (`Assert.Equal`, `Assert.NotNull`, etc.). **Do not use
  FluentAssertions.**
- **NSubstitute** — mocking framework
- **Reqnroll** — Gherkin `.feature` files + step definitions for acceptance tests
- **Testcontainers** — containerized MongoDB for acceptance/integration tests

## Mutation tests

Mutation score threshold is **100%**, required for `*.Domain.UnitTests` projects (see
`stryker-config.json`).

```bash
./scripts/run-mutation-tests.sh   # runs it for every unit test project listed in the script

# single project:
cd src/backend/Conference/ConferenceExample.Conference.Domain.UnitTests
dotnet stryker --config-file ../../../../stryker-config.json
```

## Format

```bash
dotnet csharpier .
```

Runs automatically on staged `.cs` files via the pre-commit hook — don't skip it.

## Code style

- `TreatWarningsAsErrors` is enabled — all compiler warnings must be resolved.
- Nullable reference types are enabled project-wide.
- Banned APIs (`src/backend/BannedSymbols.txt`): use `DateTimeOffset` instead of
  `DateTime`; use `Guid.CreateVersion7()` instead of `Guid.NewGuid()`.
- One type per file: every non-private class, record, interface, struct, or enum gets its
  own file, named after the type. Private nested types are exempt.

## OpenAPI

`dotnet build` regenerates `openapi.json` in the repo root automatically (via
`Microsoft.Extensions.ApiDescription.Server`) — no separate script or running instance
needed. The frontend's TypeScript types are generated from it (see
`.agents/frontend.md`).
