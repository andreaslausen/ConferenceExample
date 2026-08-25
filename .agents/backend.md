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
  (Testcontainers-backed MongoDB) — no in-process shortcuts to the application layer. These
  are the only tests exercising Application, Persistence, and Infrastructure code.

Do not add new `*.Application.UnitTests` / `*.Persistence.UnitTests` projects, and don't add
scenarios to an acceptance suite that bypass HTTP — write an HTTP-driven Gherkin scenario
instead.

> **Existing code note:** this repo currently has `Application.UnitTests` /
> `Persistence.UnitTests` projects, an `EventStore.UnitTests` and
> `Authentication.UnitTests`, and a Gherkin acceptance suite
> (`ConferenceExample.Talk.AcceptanceTests`) that calls the application service in-process
> rather than over HTTP — these predate the strategy above and are being migrated
> incrementally. Follow the strategy above for new and changed tests; don't take the
> existing structure as the pattern to copy.

```bash
dotnet test
dotnet test --filter "FullyQualifiedName~Talk"   # single test class or namespace
```

Test projects today:

- `ConferenceExample.ArchitectureTests` — enforces layer dependency rules
- `*.Domain.UnitTests` — the target-state unit tests (domain logic only)
- `*.AcceptanceTests` — Gherkin acceptance tests per bounded context
- `ConferenceExample.IntegrationTests` — HTTP-level controller tests (Testcontainers), the
  non-Gherkin precursor to the acceptance-test approach above
- `*.Application.UnitTests`, `*.Persistence.UnitTests`, `EventStore.UnitTests`,
  `Authentication.UnitTests` — pre-strategy unit tests outside the domain layer (see note
  above)

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

```bash
./scripts/generate-openapi.sh   # creates openapi.json in the repo root
```

Regenerate this after changing API contracts — the frontend's TypeScript types are
generated from it (see `.agents/frontend.md`).
