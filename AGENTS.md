# AGENTS.md

Guidance for AI coding agents (Claude Code, Codex, Cursor, ...) working in this repository.

## Project

Conference management system used in conference talks: .NET 10 / ASP.NET Core backend
(Clean Architecture + DDD, event-sourced) and a React/TypeScript frontend.

## Setup

```bash
./init-hooks.sh   # once after cloning: registers git hooks, installs CSharpier
```

## Everyday commands

```bash
dotnet build                          # backend build (src/backend/ConferenceExample.sln)
dotnet test                           # backend tests
dotnet csharpier .                    # format backend code

cd src/frontend && npm install        # once
cd src/frontend && npm run dev        # frontend dev server (http://localhost:5173)
cd src/frontend && npm run lint
```

## Rules that always apply

- `TreatWarningsAsErrors` is enabled for the backend — resolve all compiler warnings.
- Nullable reference types are enabled project-wide.
- Backend unit/integration tests: xUnit + NSubstitute. Do **not** use FluentAssertions.
- The pre-commit hook auto-formats staged `.cs` files with CSharpier and regenerates
  architecture docs when `.adoc` files are staged — don't bypass it.

## Load only when relevant

| Working on...                                         | Read                          |
|--------------------------------------------------------|--------------------------------|
| Backend architecture, bounded contexts, layers, DDD     | `.agents/architecture.md`      |
| Backend build/test/mutation-testing details, code style | `.agents/backend.md`           |
| Frontend build/lint/dev, API type generation            | `.agents/frontend.md`          |
| Event store, MongoDB, local dev infrastructure          | `.agents/infrastructure.md`    |
| Architecture documentation (arc42) / diagrams           | `.agents/documentation.md`     |
