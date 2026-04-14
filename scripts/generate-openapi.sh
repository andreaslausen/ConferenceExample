#!/bin/bash
set -e

# Navigate to the script directory's parent (project root)
cd "$(dirname "$0")/.."

API_PROJECT="src/backend/API/ConferenceExample.API"
OUTPUT_FILE="openapi.json"
API_URL="http://localhost:5185/openapi/v1.json"

echo "ℹ️  This script requires MongoDB to be running."
echo "   Start it with: ./scripts/dev-start.sh"
echo ""

echo "🔨 Building API project..."
dotnet build "$API_PROJECT/ConferenceExample.API.csproj" --configuration Release --verbosity quiet

echo "🚀 Starting API in background..."
dotnet run --project "$API_PROJECT/ConferenceExample.API.csproj" --no-build --configuration Release &
API_PID=$!

# Cleanup function to ensure API is stopped
cleanup() {
    echo "🛑 Stopping API..."
    kill $API_PID 2>/dev/null || true
    wait $API_PID 2>/dev/null || true
}
trap cleanup EXIT

echo "⏳ Waiting for API to start..."
for i in {1..30}; do
    if curl -s -o /dev/null -w "%{http_code}" "$API_URL" | grep -q "200"; then
        echo "✅ API is ready!"
        break
    fi
    if [ $i -eq 30 ]; then
        echo "❌ API failed to start within 30 seconds"
        exit 1
    fi
    sleep 1
done

echo "📥 Downloading OpenAPI specification..."
curl -s "$API_URL" -o "$OUTPUT_FILE"

if [ -f "$OUTPUT_FILE" ]; then
    echo "✅ OpenAPI specification generated successfully: $OUTPUT_FILE"
    
    # Pretty print the JSON file
    if command -v jq &> /dev/null; then
        jq '.' "$OUTPUT_FILE" > "${OUTPUT_FILE}.tmp" && mv "${OUTPUT_FILE}.tmp" "$OUTPUT_FILE"
        echo "✨ Formatted with jq"
    fi
    
    echo ""
    echo "File size: $(wc -c < "$OUTPUT_FILE") bytes"
    echo "Location: $(pwd)/$OUTPUT_FILE"
else
    echo "❌ Failed to generate OpenAPI specification"
    exit 1
fi
