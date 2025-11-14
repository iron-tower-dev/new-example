#!/bin/bash

# Connect to Lab Results Database
# This script opens a SQL command line interface to the database

echo "🔌 Connecting to Lab Results Database..."

# Check if the database container is running
if ! docker-compose ps sqlserver | grep -q "Up"; then
    echo "❌ Database container is not running. Starting it first..."
    ./scripts/start-db.sh
fi

echo "💻 Opening SQL command line interface..."
echo "💡 Type 'exit' or press Ctrl+C to disconnect"
echo ""

# Connect to the database
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "LabResults123!" -C -d LabResultsDb