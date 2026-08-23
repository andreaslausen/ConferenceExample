# Event Store / Local Infrastructure

The backend persists via **Event Sourcing** with **MongoDB** (Replica Set, required for
Change Streams).

## Quick start

```bash
./scripts/dev-start.sh                                         # start MongoDB + Mongo Express
dotnet run --project src/backend/API/ConferenceExample.API      # run the API
```

## Scripts

```bash
./scripts/dev-start.sh   # start MongoDB and all dev services
./scripts/dev-stop.sh    # stop all dev services (keeps data)
./scripts/dev-reset.sh   # ⚠️ destructive: deletes all data, then starts fresh
```

## Services

- MongoDB: `mongodb://localhost:27017` (`mongodb://admin:admin123@localhost:27017`),
  database `conference_example`
- Mongo Express (web UI): http://localhost:8081

## Details and troubleshooting

Full script documentation (logs, keyfile regeneration, port conflicts, etc.) is in
`scripts/README.md`. Production/configuration considerations are in
`docs/mongodb-setup.md`.
