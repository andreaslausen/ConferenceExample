# Backend: Build, Test, Style

Solution: `src/backend/ConferenceExample.sln`.

## Build & restore

```bash
dotnet build
dotnet restore
```

## Tests

```bash
dotnet test
dotnet test --filter "FullyQualifiedName~Talk"   # single test class or namespace
```

Test projects:

- `ConferenceExample.ArchitectureTests` — enforces layer dependency rules
- `*.AcceptanceTests` — end-to-end acceptance tests per bounded context
- `*.Tests` — unit tests per domain/application layer
- `ConferenceExample.IntegrationTests` — integration tests (Testcontainers)

Tools:

- **xUnit** — assertion framework (`Assert.Equal`, `Assert.NotNull`, etc.). **Do not use
  FluentAssertions.**
- **NSubstitute** — mocking framework
- **Testcontainers** — containerized dependencies for integration tests

## Mutation tests

Mutation score threshold is **100%** (see `stryker-config.json`).

```bash
./scripts/run-mutation-tests.sh   # all unit test projects

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

## OpenAPI

```bash
./scripts/generate-openapi.sh   # creates openapi.json in the repo root
```

Regenerate this after changing API contracts — the frontend's TypeScript types are
generated from it (see `.agents/frontend.md`).
