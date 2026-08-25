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
http://localhost:5185.

## Other commands

```bash
npm run build          # type-check and build for production
npm run lint            # ESLint
npm run generate-api   # regenerate TypeScript types from openapi.json
```

## API type generation

The frontend consumes generated TypeScript types from the backend's OpenAPI spec.
`openapi.json` is (re)generated automatically at the repo root whenever the backend is
built (see `.agents/backend.md`). After backend API changes:

```bash
dotnet build src/backend/ConferenceExample.sln   # regenerates openapi.json
cd src/frontend && npm run generate-api
```
