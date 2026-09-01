# Frontend

React + TypeScript app in `src/frontend/`, built with Vite, styled with Tailwind CSS v4
and shadcn/ui components.

## Prerequisites

- Node.js v20+ (e.g. via [nvm](https://github.com/nvm-sh/nvm))

## Setup

```bash
cd src/frontend
npm install
```

## Development

Start the backend and its MongoDB event store first (see `.agents/infrastructure.md`),
then:

```bash
cd src/frontend
npm run dev
```

App runs at http://localhost:5173. Requests to `/api/*` are proxied to the backend at
http://localhost:5185. Also needs Keycloak running (see `.agents/infrastructure.md`) —
login/registration redirect to Keycloak's own pages (Authorization Code + PKCE via
`keycloak-js`, wired up in `shared/auth/keycloak.ts` and `shared/auth/AuthContext.tsx`);
there is no in-app login/register form. The Keycloak URL is a constant in `keycloak.ts`
(defaults to `http://localhost:8080`, matching the Docker Compose service) — edit it there
for other environments.

## Other commands

```bash
npm run build          # type-check and build for production
npm run lint            # ESLint
npm run generate-api   # regenerate TypeScript types from openapi.json
```

## Testing

There are currently no frontend tests (no unit or component test setup). UI tests with
Playwright are a possible future addition but not planned imminently — don't add a test
framework or write frontend tests unless explicitly asked.

## API type generation

The frontend consumes generated TypeScript types from the backend's OpenAPI spec.
`openapi.json` is (re)generated automatically at the repo root whenever the backend is
built (see `.agents/backend.md`). After backend API changes:

```bash
dotnet build src/backend/ConferenceExample.sln   # regenerates openapi.json
cd src/frontend && npm run generate-api
```
