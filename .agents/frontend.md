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

## Testing

There are currently no frontend tests (no unit or component test setup). UI tests with
Playwright are a possible future addition but not planned imminently — don't add a test
framework or write frontend tests unless explicitly asked.

## API type generation

The frontend consumes generated TypeScript types from the backend's OpenAPI spec. After
backend API changes:

```bash
./scripts/generate-openapi.sh    # from repo root: generate openapi.json from the running backend
cd src/frontend && npm run generate-api
```
