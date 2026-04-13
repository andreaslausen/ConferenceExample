#!/bin/bash
# Reset all development infrastructure (removes all data!)

set -e

echo "⚠️  WARNING: This will DELETE ALL DATA in your development infrastructure!"
echo ""
read -p "Are you sure you want to continue? (yes/no): " -r
echo ""

if [[ ! $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
    echo "❌ Reset cancelled."
    exit 0
fi

echo "🗑️  Stopping and removing containers..."
docker-compose down

echo ""
echo "🗑️  Removing volumes (this deletes all data)..."
docker-compose down -v

echo ""
echo "🔑 Generating fresh MongoDB keyfile..."
rm -f scripts/mongo-keyfile
openssl rand -base64 756 > scripts/mongo-keyfile
chmod 400 scripts/mongo-keyfile

echo ""
echo "🔄 Starting fresh infrastructure..."
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
echo "✅ Development infrastructure has been reset!"
echo ""
echo "📊 Services:"
echo "   - MongoDB:        mongodb://localhost:27017"
echo "   - Mongo Express:  http://localhost:8081"
echo ""
echo "💡 All data has been wiped. You're starting fresh!"
echo ""
