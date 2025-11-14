# Lab Results Application Makefile
# Convenient commands for development workflow

.PHONY: help db-start db-stop db-reset db-connect api frontend dev clean

# Default target
help:
	@echo "Lab Results Application - Development Commands"
	@echo ""
	@echo "Database Commands:"
	@echo "  make db-start    - Start the database infrastructure"
	@echo "  make db-stop     - Stop the database containers"
	@echo "  make db-reset    - Reset database (removes all data)"
	@echo "  make db-connect  - Connect to database CLI"
	@echo ""
	@echo "Application Commands:"
	@echo "  make api         - Start the .NET API server"
	@echo "  make frontend    - Start the Angular frontend"
	@echo "  make dev         - Start database + API + frontend"
	@echo ""
	@echo "Utility Commands:"
	@echo "  make clean       - Clean up Docker resources"
	@echo "  make logs        - View database logs"
	@echo "  make status      - Check container status"

# Database commands
db-start:
	@echo "🚀 Starting database infrastructure..."
	@./scripts/start-db.sh

db-stop:
	@echo "🛑 Stopping database infrastructure..."
	@./scripts/stop-db.sh

db-reset:
	@echo "🔄 Resetting database infrastructure..."
	@./scripts/reset-db.sh

db-connect:
	@echo "🔌 Connecting to database..."
	@./scripts/connect-db.sh

# Application commands
api:
	@echo "🚀 Starting .NET API server..."
	@cd LabResultsApi && dotnet run

frontend:
	@echo "🚀 Starting Angular frontend..."
	@cd lab-results-frontend && npm start

# Development workflow
dev:
	@echo "🚀 Starting full development environment..."
	@echo "Starting database..."
	@./scripts/start-db.sh
	@echo ""
	@echo "✅ Database ready!"
	@echo ""
	@echo "Next steps:"
	@echo "1. In a new terminal, run: make api"
	@echo "2. In another terminal, run: make frontend"
	@echo "3. Open http://localhost:4200 in your browser"

# Utility commands
clean:
	@echo "🧹 Cleaning up Docker resources..."
	@docker-compose down -v
	@docker system prune -f
	@echo "✅ Cleanup complete!"

logs:
	@echo "📋 Viewing database logs..."
	@docker-compose logs -f sqlserver

status:
	@echo "📊 Container status:"
	@docker-compose ps
	@echo ""
	@echo "📊 Docker system info:"
	@docker system df