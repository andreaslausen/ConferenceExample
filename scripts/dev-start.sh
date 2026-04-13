#!/bin/bash
# Start all development infrastructure containers

set -e

echo "🚀 Starting development infrastructure..."
echo ""

# Generate MongoDB keyfile if it doesn't exist
if [ ! -f "scripts/mongo-keyfile" ]; then
    echo "🔑 Generating MongoDB keyfile..."
    openssl rand -base64 756 > scripts/mongo-keyfile
    chmod 400 scripts/mongo-keyfile
    echo "✅ Keyfile generated"
    echo ""
fi

# Start Docker Compose services
docker-compose up -d

echo ""
echo "⏳ Waiting for MongoDB to be ready..."

# Wait for MongoDB to be healthy
MAX_RETRIES=30
RETRY_COUNT=0

while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    # Check if MongoDB port is accepting connections
    if docker exec conference-mongodb mongosh --quiet --eval "db.runCommand({ ping: 1 })" --username admin --password admin123 >/dev/null 2>&1; then
        echo "✅ MongoDB is ready!"
        break
    fi

    RETRY_COUNT=$((RETRY_COUNT + 1))
    if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
        echo "⚠️  MongoDB did not respond in time, but containers are running."
        echo "   You can check logs with: docker logs conference-mongodb"
        break
    fi

    echo "   Waiting... ($RETRY_COUNT/$MAX_RETRIES)"
    sleep 2
done

echo ""
echo "🎉 Development infrastructure is running!"
echo ""
echo "📊 Services:"
echo "   - MongoDB:        mongodb://localhost:27017"
echo "   - Mongo Express:  http://localhost:8081"
echo ""
echo "💡 Tips:"
echo "   - Stop:  ./scripts/dev-stop.sh"
echo "   - Reset: ./scripts/dev-reset.sh"
echo "   - Logs:  docker-compose logs -f"
echo ""
