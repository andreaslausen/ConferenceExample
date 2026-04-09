# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Setup

```bash
./init-hooks.sh   # Set up git hooks and install CSharpier (run once after cloning)
```

## Common Commands

```bash
# Build & restore
dotnet build
dotnet restore

# Run tests
dotnet test
dotnet test --filter "FullyQualifiedName~Talk"   # Run a single test class or namespace

# Format code
dotnet csharpier .

# Build architecture documentation (requires Docker)
./build-docs.sh
```

## Architecture Overview

This is a .NET 10 / ASP.NET Core conference management system demonstrating **Clean Architecture** with **Domain-Driven Design**. The solution is at `src/backend/ConferenceExample.sln`.

### Bounded Contexts

There are two separate bounded contexts, each with its own layered stack:

- **Conference** — manages conferences, rooms, schedules
- **Talk** — manages talks, speakers, tags, abstracts

### Layers (per bounded context)

```
API  ──►  Application  ──►  Domain
                  └──────►  Persistence (shared models + IDatabaseContext)
```

- **Domain** projects contain aggregates, entities, and value objects. Strong typing is enforced via value objects (e.g. `TalkId`, `SpeakerId`, `GuidV7`, `TalkTitle`, `Abstract`).
- **Application** projects contain use cases and depend on `IDatabaseContext` from the shared Persistence project.
- **ConferenceExample.Persistence** holds shared data models (`Talk`, `Speaker`, `Conference`) and the `IDatabaseContext` interface.
- **ConferenceExample.API** is the ASP.NET Core entry point. `ServiceCollectionExtensions` registers infrastructure; an in-memory `DatabaseContext` implements `IDatabaseContext`.

### Testing

- `ConferenceExample.ArchitectureTests` — enforces layer dependency rules
- `*.AcceptanceTests` — end-to-end acceptance tests per bounded context
- `*.Tests` — unit tests per domain/application layer

## Code Style

- CSharpier is the formatter; it runs automatically on staged `.cs` files via the pre-commit hook.
- `TreatWarningsAsErrors` is enabled — all compiler warnings must be resolved.
- Nullable reference types are enabled project-wide.

## Documentation

Architecture docs use the arc42 template in AsciiDoc format under `src/documentation/`. Running `./build-docs.sh` generates `docs/index.html` using Asciidoctor via Docker. The pre-commit hook regenerates docs automatically when `.adoc` files are staged.
