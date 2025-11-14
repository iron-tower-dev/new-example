#!/bin/bash

# Start Lab Results Database Infrastructure
# This script starts the SQL Server database using Docker Compose

echo "🚀 Starting Lab Results Database Infrastructure..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi

# Check if Docker Compose is available
if ! command -v docker-compose > /dev/null 2>&1; then
    echo "❌ Docker Compose is not installed. Please install docker-compose."
    exit 1
fi

# Start the database services
echo "📦 Starting SQL Server container..."
docker-compose up -d sqlserver

echo "⏳ Waiting for SQL Server to be ready..."
sleep 10

# Check if SQL Server is healthy
echo "🔍 Checking SQL Server health..."
for i in {1..30}; do
    if docker-compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "LabResults123!" -C -Q "SELECT 1" > /dev/null 2>&1; then
        echo "✅ SQL Server is ready!"
        break
    fi
    echo "⏳ Waiting for SQL Server... (attempt $i/30)"
    sleep 2
done

# Run database initialization
echo "🗄️  Initializing database..."
docker-compose up db-init

echo "✅ Database infrastructure is ready!"
echo ""
echo "📋 Connection Details:"
echo "   Server: localhost,1433"
echo "   Database: LabResultsDb"
echo "   Username: sa"
echo "   Password: LabResults123!"
echo ""
echo "🔧 Useful commands:"
echo "   Stop database: ./scripts/stop-db.sh"
echo "   Reset database: ./scripts/reset-db.sh"
echo "   View logs: docker-compose logs -f sqlserver"
echo "   Connect to SQL: docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'LabResults123!' -C -d LabResultsDb"