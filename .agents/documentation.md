# Architecture Documentation

Architecture docs follow the [arc42](https://arc42.org/) template in AsciiDoc format under
`src/documentation/` (source chapters in `src/documentation/chapters/`).

## Build

```bash
./build-docs.sh   # requires Docker
```

Renders the AsciiDoc sources (via Asciidoctor + Asciidoctor Diagram) into `docs/index.html`
and `docs/images/`. The pre-commit hook regenerates docs automatically when `.adoc` files
are staged.

Published version: https://andreaslausen.github.io/ConferenceExample/

## When to update

Update the relevant chapter under `src/documentation/chapters/` whenever a change affects
system scope, solution strategy, building blocks, runtime behavior, deployment, or an
architecture decision — not just the code.
