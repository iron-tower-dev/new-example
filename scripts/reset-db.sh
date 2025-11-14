#!/bin/bash

# Reset Lab Results Database Infrastructure
# This script completely removes and recreates the database

echo "🔄 Resetting Lab Results Database Infrastructure..."

# Stop and remove all containers and volumes
echo "🗑️  Removing existing containers and volumes..."
docker-compose down -v

# Remove any orphaned containers
docker-compose rm -f

# Prune unused volumes (optional - be careful in production)
read -p "Do you want to remove all unused Docker volumes? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    docker volume prune -f
    echo "🧹 Cleaned up unused volumes"
fi

# Start fresh
echo "🚀 Starting fresh database infrastructure..."
./scripts/start-db.sh

echo "✅ Database infrastructure has been reset!"