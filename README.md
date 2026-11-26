# ConferenceExample

This is an example which I use in some of my conference talks: a conference management
system with an ASP.NET Core backend (Clean Architecture, DDD, Event Sourcing) and a
React/TypeScript frontend.

![](logo.png)

## ⚠️ Disclaimer

**This is an example project for demonstration and educational purposes only.** It is **not production-ready** and has **not been reviewed for security concerns**. Do not use this code in production environments without thorough review, testing, and security hardening.

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)
- [Node.js](https://nodejs.org/) (e.g. via [nvm](https://github.com/nvm-sh/nvm))

### Setup (once after cloning)

```bash
./init-hooks.sh                 # registers git hooks, installs CSharpier
cd src/frontend && npm install
```

The pre-commit hook formats staged C# files with CSharpier and regenerates the architecture
docs when `.adoc` files are staged. Don't bypass it.

### Run

```bash
./scripts/dev-start.sh                                       # 1. MongoDB + Mongo Express
dotnet run --project src/backend/API/ConferenceExample.API   # 2. Backend  → http://localhost:5185
cd src/frontend && npm run dev                               # 3. Frontend → http://localhost:5173
```

The frontend proxies `/api/*` to the backend. Mongo Express is available at http://localhost:8081.

```bash
./scripts/dev-stop.sh    # Stop dev services (keeps data)
./scripts/dev-reset.sh   # Reset dev services (deletes all data!)
```

More details: [scripts/README.md](scripts/README.md), [docs/mongodb-setup.md](docs/mongodb-setup.md).

## Everyday commands

```bash
dotnet build                       # Build backend (also regenerates openapi.json)
dotnet test                        # Backend tests (acceptance tests need Docker)
dotnet csharpier .                 # Format backend code

cd src/frontend
npm run lint                       # Lint
npm run build                      # Type-check and build
npm run generate-api               # Regenerate API types from openapi.json (build backend first)
```

## Architecture Documentation

The arc42 documentation (AsciiDoc, in [src/documentation/](src/documentation/)) is published at
**https://andreaslausen.github.io/ConferenceExample/**. To build it locally (requires Docker):

```bash
./build-docs.sh
```

## AI coding agents

See [AGENTS.md](AGENTS.md).
