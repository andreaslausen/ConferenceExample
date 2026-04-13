#!/bin/bash
# Stop all development infrastructure containers

set -e

echo "🛑 Stopping development infrastructure..."
echo ""

# Stop Docker Compose services
docker-compose stop

echo ""
echo "✅ Development infrastructure stopped!"
echo ""
echo "💡 Tips:"
echo "   - Start:  ./scripts/dev-start.sh"
echo "   - Reset:  ./scripts/dev-reset.sh"
echo "   - Remove: docker-compose down"
echo ""
