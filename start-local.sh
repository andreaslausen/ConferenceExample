#!/bin/bash
# Start everything needed for local development:
#   1. MongoDB container (via scripts/dev-start.sh)
#   2. .NET backend  (http://localhost:5185)
#   3. Vite frontend (http://localhost:5173)
#
# Press Ctrl+C to stop all processes.

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

BACKEND_DIR="src/backend/API/ConferenceExample.API"
FRONTEND_DIR="src/frontend"

# ── Cleanup ────────────────────────────────────────────────────────────────────
PIDS=()

cleanup() {
    echo ""
    echo "Stopping local services..."
    for pid in "${PIDS[@]}"; do
        kill "$pid" 2>/dev/null || true
    done
    wait 2>/dev/null || true
    echo "Done."
}

trap cleanup INT TERM

# ── 1. MongoDB ─────────────────────────────────────────────────────────────────
echo "==> Starting MongoDB container..."
./scripts/dev-start.sh

# ── 2. Backend ─────────────────────────────────────────────────────────────────
echo ""
echo "==> Starting .NET backend (http://localhost:5185)..."
dotnet run --project "$BACKEND_DIR" --launch-profile http &
PIDS+=($!)

# ── 3. Frontend ────────────────────────────────────────────────────────────────
echo ""
echo "==> Starting Vite frontend (http://localhost:5173)..."
(cd "$FRONTEND_DIR" && npm run dev) &
PIDS+=($!)

# ── Summary ────────────────────────────────────────────────────────────────────
echo ""
echo "All services started. Press Ctrl+C to stop."
echo ""
echo "  Backend:       http://localhost:5185"
echo "  Frontend:      http://localhost:5173"
echo "  MongoDB:       mongodb://localhost:27017"
echo "  Mongo Express: http://localhost:8081"
echo ""

wait
