#!/bin/bash

# Stop Lab Results Database Infrastructure
# This script stops the SQL Server database containers

echo "🛑 Stopping Lab Results Database Infrastructure..."

# Stop all services
docker-compose down

echo "✅ Database infrastructure stopped!"
echo ""
echo "💡 To start again, run: ./scripts/start-db.sh"
echo "💡 To completely reset, run: ./scripts/reset-db.sh"