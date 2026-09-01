# Event Store / Local Infrastructure

The backend persists via **Event Sourcing** with **MongoDB** (Replica Set, required for
multi-document transactions used by optimistic concurrency control on append). Events are
published in-process via an `InMemoryEventBus` after a successful append — not via MongoDB
Change Streams.

Authentication runs against **Keycloak** (see `.agents/architecture.md`), also started via
Docker Compose for local development.

## Quick start

```bash
./scripts/dev-start.sh                                         # start MongoDB, Keycloak, Mongo Express
dotnet run --project src/backend/API/ConferenceExample.API      # run the API
```

## Scripts

```bash
./scripts/dev-start.sh   # start MongoDB, Keycloak, and all dev services
./scripts/dev-stop.sh    # stop all dev services (keeps data)
./scripts/dev-reset.sh   # ⚠️ destructive: deletes all data, then starts fresh
```

## Services

- MongoDB: `mongodb://localhost:27017` (`mongodb://admin:admin123@localhost:27017`),
  database `conference_example`
- Mongo Express (web UI): http://localhost:8081
- Keycloak: http://localhost:8080, realm `conference-example`, admin console at
  http://localhost:8080/admin (`admin`/`admin`). Runs in `start-dev` mode with an ephemeral
  in-container database — realm config is (re-)imported from
  `scripts/keycloak/realm-export.json` on every container start, so edit that file (not the
  Admin Console) for changes that should persist across resets. Client `conference-example-frontend`
  is the public OIDC client the frontend uses (Authorization Code + PKCE); the realm has
  self-registration enabled and new users default to the `Attendee` role via the `attendees`
  default group — an organizer or the Admin Console must grant `Speaker`/`Organizer` roles
  manually today (see `.agents/architecture.md`).

## Details and troubleshooting

Full script documentation (logs, keyfile regeneration, port conflicts, etc.) is in
`scripts/README.md`. Production/configuration considerations are in
`docs/mongodb-setup.md`.
