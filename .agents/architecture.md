# Backend Architecture

Clean Architecture with Domain-Driven Design. Solution: `src/backend/ConferenceExample.sln`.

## Bounded contexts

Two bounded contexts are implemented today, each with its own layered stack:

- **Conference** — manages conferences, rooms, schedules
- **Talk** — manages talks, speakers, tags, abstracts

The arc42 docs (`src/documentation/chapters/04_solution_strategy.adoc`,
`05_building_block_view.adoc`) describe further planned bounded contexts (Voting,
Notification, Attendee, Content) that don't exist in code yet — check there before assuming
a subdomain has no home.

## Authentication & authorization

Authentication is delegated to **Keycloak** (local dev instance via Docker Compose, see
`.agents/infrastructure.md`). `ConferenceExample.Authentication` under
`src/backend/Infrastructure/` is not a bounded context, just cross-cutting infrastructure —
it validates Keycloak-issued JWTs (`ServiceCollectionExtensions`, JWKS-based, no shared
secret) and exposes `ICurrentUserService`/`CurrentUserService` to the Conference and Talk
Application layers. There is no `/api/auth/*` endpoint anymore; the frontend redirects to
Keycloak's own login/registration pages (Authorization Code + PKCE, see
`.agents/frontend.md`).

Authorization is still role-based (`UserRole`: `Speaker`/`Organizer`/`Attendee`), but the
roles now come from the token's `realm_access.roles` claim rather than being defined at
registration time in code — `RealmRoleClaimsTransformation` flattens that Keycloak-specific
claim shape into standard `ClaimTypes.Role` claims, ignoring Keycloak's own default roles
(`offline_access`, `uma_authorization`, `default-roles-*`). A user is expected to carry
exactly one of the three app roles; self-registered users default to `Attendee` via a
Keycloak default group, and `Speaker`/`Organizer` are granted by an organizer/admin.

Keycloak's `sub` claim (the external identity) is a plain UUID, not the app-minted `GuidV7`
used elsewhere for internally-created ids. Rather than building the full anti-corruption
layer with a local identity-mapping store that a stricter reading of "Identity becomes its
own bounded context" might suggest, this migration took the smaller, pragmatic step:
`OrganizerId` (Conference.Domain) and `SpeakerId` (Talk.Domain) now wrap a plain `Guid`
instead of `GuidV7`, since they represent an externally-issued identity, not something the
app itself generates. IDs the app does mint (`TalkId`, `ConferenceId`, `RoomId`, `TalkTypeId`,
...) are unaffected and still use `GuidV7`.

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
concurrency via MongoDB transactions; events are then published in-process via an
`InMemoryEventBus`). See `.agents/infrastructure.md` for running it locally, and ADR-2 in
`src/documentation/chapters/09_architecture_decisions.adoc` for the design rationale.

## CQRS: commands and queries

Application projects use one folder per use case (e.g. `SubmitTalk/`, `GetTalkById/`),
each following a fixed shape:

- **Command** (write side) — `{UseCase}Command` (input record) +
  `I{UseCase}CommandHandler` / `{UseCase}CommandHandler`. Command handlers rebuild
  aggregate state from the **event store** (via the Domain repository) and append new
  events — never via a ReadModel repository. `ConferenceExample.ArchitectureTests`
  (`EventSourcingRules`) enforces this: a `*CommandHandler` must not depend on or call any
  `*ReadModelRepository`.
- **Query** (read side) — `{UseCase}Query` (input record) + `I{UseCase}QueryHandler` /
  `{UseCase}QueryHandler`, returning a `{UseCase}Dto`. Query handlers read from
  `*ReadModelRepository` projections, never from the event store.
- Read models are kept up to date by event handlers wired up in `*EventSubscriptions`
  (Persistence layer), which subscribe to the `InMemoryEventBus` and update the
  corresponding `*ReadModelRepository` when an event is published.

Follow this shape for new use cases rather than improvising a different one. For the
rationale (why CQRS, why event sourcing) see ADR-2 in
`src/documentation/chapters/09_architecture_decisions.adoc`.

## Layer dependency rules

Enforced automatically by `ConferenceExample.ArchitectureTests` — run it after moving code
between layers to catch violations early.

## Deeper reference

For anything beyond this summary (system context, runtime views, architecture decisions,
quality requirements, glossary), read the arc42 docs under `src/documentation/chapters/`
(AsciiDoc source) or the rendered version at
https://andreaslausen.github.io/ConferenceExample/. See `.agents/documentation.md` for how
to regenerate it.
