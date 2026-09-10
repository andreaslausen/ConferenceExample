# Backend Architecture

Clean Architecture with Domain-Driven Design. Solution: `src/backend/ConferenceExample.sln`.

## Bounded contexts

Three bounded contexts are implemented today, each with its own layered stack:

- **Speaker** — speaker profiles (name, biography). A profile has its own `SpeakerId`,
  deliberately distinct from the account's `UserId`, so one account can hold several role
  profiles later on.
- **Talk** — a speaker's talks, independent of any conference: created, edited, deleted, and
  submitted to conferences. The `Talk` aggregate also records each submission it made.
- **Conference** — conferences, talk types, rooms, schedules, and the program. Receives talk
  submissions, decides on them, schedules accepted talks.

The arc42 docs (`src/documentation/chapters/04_solution_strategy.adoc`,
`05_building_block_view.adoc`) describe further planned bounded contexts (Voting,
Notification, Attendee, Content) that don't exist in code yet — check there before assuming
a subdomain has no home.

### How a submission works

This is the one flow that crosses all three contexts, so it's worth knowing before touching
any of them. See ADR-5 in `src/documentation/chapters/09_architecture_decisions.adoc` for the
rationale.

1. A speaker submits an existing talk: `Talk.SubmitToConference(conferenceId, talkTypeId)`
   raises `TalkSubmittedToConferenceEvent` on the **talk's own** stream, carrying the talk's
   title, abstract and tags as a **snapshot**.
2. The Conference BC handles that event (`Conference.Persistence`'s `TalkEventHandler`) and
   calls `Conference.SubmitTalk(talkId, talkTypeId)`, which either raises
   `TalkSubmissionRegisteredEvent` or turns the submission away with
   `TalkSubmissionRejectedEvent` (unknown conference, not accepting submissions, talk type not
   offered, already submitted). On registration it writes a `ConferenceTalkDocument` combining
   the event's snapshot with the speaker's name and biography from its own speaker projection.
3. The Talk BC projects the outcome back into its submission read model
   (`TalkSubmissionEventHandler`), so the speaker sees `Pending` → `Submitted` → `Accepted` /
   `Rejected`, or `Failed` with a reason.

**Snapshots don't resync.** Editing a talk or a speaker profile afterwards affects *future*
submissions only. The Conference BC deliberately does **not** subscribe to Talk's edit events
— a conference reviews and publishes what was actually submitted. Don't "fix" this by adding
those subscriptions.

**Deleting a talk** removes it from the speaker's list only; conferences keep the submissions
they already registered.

### Cross-BC projections

A context keeps a minimal replica of another's data where it genuinely needs one, fed by
domain events — never a synchronous call or a shared model across the boundary:

- `Talk.Domain.SpeakerManagement.ISpeakerDirectory` — `UserId` → `SpeakerId`. An *identity
  index*, not a read model: it answers "whose talks are these?", never "what does that profile
  say". Command handlers may use it (via `ICurrentSpeakerProvider`) to establish ownership
  before loading the aggregate from the event store; that's why it is deliberately not named
  `*ReadModelRepository` and not covered by the CQRS rule below.
- `Talk.Domain.ConferenceManagement.IConferenceDirectory` — conference names, so a speaker's
  submission list reads as names rather than ids. Display data only.
- `Conference.Persistence.ReadModels.ISpeakerDocumentRepository` — speaker profiles, read once
  when a submission is registered to freeze the speaker's details into it.

## Authentication & authorization

Authentication is currently a self-built construct (`ConferenceExample.Authentication`
under `src/backend/Infrastructure/`) — not a bounded context, just cross-cutting
infrastructure. Per the arc42 solution strategy, it's planned to be replaced later by an
external identity provider (e.g. Keycloak), at which point Identity becomes its own bounded
context with an anti-corruption layer at the boundary. Don't build toward that migration
preemptively — work with the current construct as-is.

Authorization is role-based, with roles defined in code (not yet coming from claims/an IdP).
That's expected to stay as-is for now.

## Layers (per bounded context)

```
API  ──►  Application  ──►  Domain
                  └──────►  Persistence (per bounded context)
```

- **Domain** projects contain aggregates, entities, and value objects. Strong typing is
  enforced via value objects (e.g. `TalkId`, `SpeakerId`, `UserId`, `GuidV7`, `TalkTitle`,
  `Abstract`). Each context has its own copy of the shared kernel — they are separate contexts,
  not a shared library.
- **Application** projects contain use cases and depend on repository interfaces defined in
  their own Domain project.
- **Persistence** projects are per bounded context (`*.Persistence`) and own that context's
  event store collection, read models, event handlers and event subscriptions.
- **ConferenceExample.API** is the ASP.NET Core entry point. `ServiceCollectionExtensions`
  registers infrastructure and wires each context's `*EventSubscriptions` to the event bus.

## Event sourcing

The system persists via **Event Sourcing** with MongoDB as the event store (optimistic
concurrency via MongoDB transactions; events are then published in-process via an
`InMemoryEventBus`). See `.agents/infrastructure.md` for running it locally, and ADR-2 in
`src/documentation/chapters/09_architecture_decisions.adoc` for the design rationale.

## CQRS: commands and queries

Application projects use one folder per use case (e.g. `CreateTalk/`,
`SubmitTalkToConference/`, `GetTalkById/`),
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
