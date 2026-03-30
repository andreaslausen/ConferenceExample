#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT=$(git rev-parse --show-toplevel)
cd "$REPO_ROOT"

echo "Initializing git hooks..."

# Register the .githooks directory as the hooks path for this clone
git config core.hooksPath .githooks

# Make all hooks executable
chmod +x .githooks/*

echo "Ensuring CSharpier is available..."

# Create a dotnet tool manifest if none exists yet
if [ ! -f ".config/dotnet-tools.json" ]; then
  echo "Creating dotnet tool manifest..."
  dotnet new tool-manifest
fi

# Add CSharpier to the manifest if it is not listed there yet
if ! grep -q '"csharpier"' .config/dotnet-tools.json; then
  echo "Installing CSharpier..."
  dotnet tool install csharpier
fi

# Restore all tools listed in the manifest (idempotent)
dotnet tool restore

echo ""
echo "Done! The pre-commit hook will format staged C# files with CSharpier."
echo "If .config/dotnet-tools.json was just created, commit it to the repository."
