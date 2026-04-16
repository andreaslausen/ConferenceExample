# Development Scripts

This directory contains helper scripts for managing the local development infrastructure.

## Available Scripts

### `dev-start.sh`

Starts all development infrastructure containers (MongoDB, Mongo Express).

**Features:**
- Automatically generates MongoDB keyfile if it doesn't exist
- Starts all Docker Compose services
- Waits for MongoDB to be ready
- Shows connection information

**Usage:**
```bash
./scripts/dev-start.sh
```

**Output:**
```
🚀 Starting development infrastructure...
✅ MongoDB is ready!
🎉 Development infrastructure is running!

📊 Services:
   - MongoDB:        mongodb://localhost:27017
   - Mongo Express:  http://localhost:8081
```

---

### `dev-stop.sh`

Stops all development infrastructure containers **without** removing data.

**Features:**
- Gracefully stops all containers
- Preserves all data in Docker volumes
- Containers can be restarted with `dev-start.sh`

**Usage:**
```bash
./scripts/dev-stop.sh
```

**When to use:**
- End of workday
- Temporarily free up system resources
- Switch between projects

---

### `dev-reset.sh`

**⚠️ DESTRUCTIVE OPERATION** - Resets all development infrastructure and **deletes all data**.

**Features:**
- Prompts for confirmation before proceeding
- Stops and removes all containers
- Deletes all Docker volumes (all data is lost!)
- Generates fresh MongoDB keyfile
- Starts fresh infrastructure

**Usage:**
```bash
./scripts/dev-reset.sh
```

**When to use:**
- Start completely fresh
- Clean up corrupted data
- Reset to initial state for testing

**Warning:**
This will delete:
- All events in MongoDB
- All data in all containers
- Docker volumes

---

## Infrastructure Components

### MongoDB
- **Port:** 27017
- **Connection String:** `mongodb://admin:admin123@localhost:27017`
- **Database:** `conference_example`
- **Configuration:** Replica Set (required for Change Streams)

### Mongo Express (Web UI)
- **Port:** 8081
- **URL:** http://localhost:8081
- **Purpose:** Browse MongoDB data, debug events, inspect collections

---

## Troubleshooting

### MongoDB won't start

**Check logs:**
```bash
docker logs conference-mongodb
```

**Common issues:**
1. Port 27017 already in use
   ```bash
   lsof -i :27017  # See what's using the port
   ```

2. Keyfile permissions
   ```bash
   ls -la scripts/mongo-keyfile  # Should show: -r--------
   ```

### Mongo Express can't connect

**Check if MongoDB is running:**
```bash
docker ps | grep mongodb
```

**Restart Mongo Express:**
```bash
docker restart conference-mongo-express
```

### Permission denied on keyfile

**Fix permissions:**
```bash
chmod 600 scripts/mongo-keyfile
rm scripts/mongo-keyfile
./scripts/dev-start.sh  # Will regenerate
```

---

## Files Generated

### `mongo-keyfile`
- Auto-generated on first `dev-start.sh`
- Required for MongoDB Replica Set with authentication
- Ignored by git (see `.gitignore`)
- Recreated on `dev-reset.sh`

**Manual generation:**
```bash
openssl rand -base64 756 > scripts/mongo-keyfile
chmod 400 scripts/mongo-keyfile
```

---

## Tips

**View logs:**
```bash
docker-compose logs -f          # All services
docker logs conference-mongodb  # MongoDB only
```

**Check container status:**
```bash
docker-compose ps
docker ps
```

**Access MongoDB shell:**
```bash
docker exec -it conference-mongodb mongosh -u admin -p admin123
```

**Remove everything (including volumes):**
```bash
docker-compose down -v
rm scripts/mongo-keyfile
```
