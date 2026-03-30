#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT=$(git rev-parse --show-toplevel)
DOCS_SOURCE="$REPO_ROOT/src/documentation"
DOCS_OUTPUT="$REPO_ROOT/docs"

mkdir -p "$DOCS_OUTPUT"

echo "Building architecture documentation with Asciidoctor..."

docker run --rm \
  -v "$DOCS_SOURCE":/documents \
  -v "$DOCS_OUTPUT":/output \
  asciidoctor/docker-asciidoctor \
  asciidoctor \
    --out-file /output/index.html \
    -r asciidoctor-diagram \
    /documents/arc42-template.adoc

# Copy images to docs output
if [ -d "$DOCS_SOURCE/images" ] && [ "$(ls -A "$DOCS_SOURCE/images" 2>/dev/null)" ]; then
  echo "Copying images..."
  cp -r "$DOCS_SOURCE/images/." "$DOCS_OUTPUT/images/"
fi

echo "Documentation built successfully: $DOCS_OUTPUT/index.html"
