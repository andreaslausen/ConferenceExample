# Backend Architecture

Clean Architecture with Domain-Driven Design. Solution: `src/backend/ConferenceExample.sln`.

## Bounded contexts

Two separate bounded contexts, each with its own layered stack:

- **Conference** — manages conferences, rooms, schedules
- **Talk** — manages talks, speakers, tags, abstracts

## Layers (per bounded context)

```
API  ──►  Application  ──►  Domain
                  └──────►  Persistence (shared models + IDatabaseContext)
```

- **Domain** projects contain aggregates, entities, and value objects. Strong typing is
  enforced via value objects (e.g. `TalkId`, `SpeakerId`, `GuidV7`, `TalkTitle`, `Abstract`).
- **Application** projects contain use cases and depend on `IDatabaseContext` from the
  shared Persistence project.
- **ConferenceExample.Persistence** holds shared data models (`Talk`, `Speaker`,
  `Conference`) and the `IDatabaseContext` interface.
- **ConferenceExample.API** is the ASP.NET Core entry point. `ServiceCollectionExtensions`
  registers infrastructure.

## Event sourcing

The system persists via **Event Sourcing** with MongoDB as the event store (optimistic
concurrency via MongoDB transactions, Change Streams for real-time notifications). See
`.agents/infrastructure.md` for running it locally, and
`docs/implementation-plan-event-sourcing-cqrs.md` for the design rationale.

## Layer dependency rules

Enforced automatically by `ConferenceExample.ArchitectureTests` — run it after moving code
between layers to catch violations early.

## Deeper reference

For anything beyond this summary (system context, runtime views, architecture decisions,
quality requirements, glossary), read the arc42 docs under `src/documentation/chapters/`
(AsciiDoc source) or the rendered version at
https://andreaslausen.github.io/ConferenceExample/. See `.agents/documentation.md` for how
to regenerate it.
