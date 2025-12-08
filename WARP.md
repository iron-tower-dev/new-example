# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

This is a Laboratory Test Results application with:
- **Backend**: ASP.NET Core 8.0 Web API with Entity Framework Core
- **Frontend**: Angular 20+ with Material UI
- **Database**: SQL Server 2022 (containerized via Docker)

The application manages laboratory test results for lubricant samples, including test entry, particle analysis, emission spectroscopy, and historical data tracking.

## Common Development Commands

### Database Management
```fish
# Start database infrastructure (Docker required)
make db-start
# OR
./scripts/start-db.sh

# Stop database
make db-stop

# Reset database (deletes all data)
make db-reset

# Connect to database CLI
make db-connect

# Update database structure from SQL files
./update-database.fish

# Test database connection
./test-connection.sh
```

**Database credentials:**
- Server: `localhost,1433`
- Database: `LabResultsDb`
- Username: `sa`
- Password: `LabResults123!`

### Backend (.NET API)

```bash
# Run the API (from project root)
cd LabResultsApi
dotnet run

# Run with watch (auto-restart on changes)
dotnet watch run

# Build the API
dotnet build

# Run tests
cd LabResultsApi
dotnet test ../LabResultsApi.Tests/LabResultsApi.Tests.csproj

# Run tests with coverage
dotnet test ../LabResultsApi.Tests/LabResultsApi.Tests.csproj --collect:"XPlat Code Coverage"
```

API runs at: `https://localhost:5001` (dev)
Swagger UI available at: `https://localhost:5001` (root path in development)

### Frontend (Angular)

```bash
# Navigate to frontend directory
cd lab-results-frontend

# Install dependencies (first time)
npm install

# Start dev server
npm start
# OR
ng serve

# Build for production
npm run build:prod

# Build for staging
npm run build:staging

# Run unit tests (uses Thorium browser)
npm test

# Run tests with coverage
npm run test:coverage

# Run tests in watch mode
npm run test:watch

# Lint code
npm run lint

# Run E2E tests
npm run e2e
npm run e2e:headed  # with browser UI
npm run e2e:ui       # with Playwright UI
```

Frontend runs at: `http://localhost:4200`

### Full Development Workflow

```fish
# Terminal 1: Start database
make db-start

# Terminal 2: Start API
cd LabResultsApi && dotnet run

# Terminal 3: Start frontend
cd lab-results-frontend && npm start
```

## Architecture and Code Structure

### Backend Architecture

**Layered Architecture Pattern:**
```
Controllers/Endpoints → Services → Data (DbContext) → Database
           ↓              ↓
         DTOs        Models/Entities
           ↓              
       Mappings (AutoMapper)
```

**Key Architectural Patterns:**
- **Minimal API Endpoints**: Uses endpoint mapping pattern (see `Endpoints/` directory)
- **Service Layer**: Business logic encapsulated in service interfaces/implementations
- **Repository Pattern**: DbContext acts as repository with EF Core
- **DTOs for API contracts**: Separate from database models
- **Middleware Pipeline**: Exception handling, auditing, performance monitoring

**Important Service Interfaces:**
- `ISampleService`: Sample CRUD operations
- `ITestResultService`: Test result management
- `ICalculationService`: Business calculations (TAN, viscosity, etc.)
- `IValidationService`: Data validation logic
- `ILookupService`: Reference data caching
- `IRawSqlService`: Legacy SQL query support with optimization layer
- `IPerformanceMonitoringService`: Performance tracking

**Database Context:**
- Location: `LabResultsApi/Data/LabDbContext.cs`
- **Critical**: Several entities are **keyless** (no primary key): `TestReadings`, `EmSpectro`, `ParticleType`, `ParticleSubType`, `InspectFilter`
- **Navigation properties don't work on keyless entities** - use raw SQL or joins instead
- Connection string in: `appsettings.json` → `ConnectionStrings:DefaultConnection`

**Performance Optimizations:**
- Query tracking disabled by default (`QueryTrackingBehavior.NoTracking`)
- In-memory caching for lookup data (1-hour expiration)
- Database connection pooling (128 connections)
- Performance monitoring middleware tracks slow queries (>1s) and endpoints (>2s)
- See `PERFORMANCE_OPTIMIZATIONS.md` for detailed performance guidance

### Frontend Architecture

**Feature-Based Structure:**
```
src/app/
├── features/          # Feature modules
│   ├── sample-management/
│   ├── test-entry/
│   └── migration/
├── shared/            # Shared components, services, utilities
└── layout/            # Layout components
```

**Angular Coding Guidelines:**
- **Use modern Angular APIs**: Input/output signals instead of decorators
- **Use new control flow**: `@if`, `@for`, `@switch` instead of structural directives (`*ngIf`, `*ngFor`)
- **Follow Angular style guide**: Component file naming per https://angular.dev/style-guide
- **Replace mock data with real API calls**: See `ferrogram-entry` component (line 421+) as example pattern

**API Integration:**
- Proxy configuration: `proxy.conf.json` routes `/api` to backend
- HTTP services in `shared/services/`
- Environment-based configuration

### Database Structure

**Core Tables:**
- `UsedLubeSamples`: Main sample records
- `Test`: Test definitions (TAN, Viscosity, Water-KF, etc.)
- `TestReadings`: Test results (**keyless entity**)
- `EmSpectro`: Emission spectroscopy data (**keyless entity**)
- `M_And_T_Equip`: Equipment/MTE information
- `ParticleType`, `ParticleSubType`: Particle analysis (**keyless entities**)
- `LubeTechList`, `ReviewerList`: Authentication tables
- `TestStand`, `TestStandMapping`: Test scheduling

**SQL Directory Structure:**
- `db-tables/`: 56 table creation scripts
- `db-functions/`: 8 user-defined functions
- `db-sp/`: 18 stored procedures
- `db-views/`: 93 views
- `db-seeding/`: CSV data for seeding
- `db-init/`: Docker initialization scripts

**Database Update Process:**
1. Drops existing views → procedures → functions (dependency order)
2. Creates tables in dependency order
3. Creates functions
4. Creates stored procedures
5. Creates views

## Testing Strategy

**Backend Testing:**
- Framework: NUnit with FluentAssertions
- Test project: `LabResultsApi.Tests/`
- Tests include: calculation logic, validation, integration tests
- In-memory database for isolated testing
- Moq for mocking dependencies

**Frontend Testing:**
- Framework: Jasmine/Karma for unit tests
- E2E: Playwright
- Browser: ThoriumHeadless (Chromium-based)
- Coverage reports available via `npm run test:coverage`

## Key Technical Considerations

### Entity Framework Core Limitations
- **Keyless entities cannot use navigation properties** - this affects `TestReadings`, `EmSpectro`, and particle analysis tables
- When querying keyless entities, use raw SQL or explicit joins instead of navigation properties
- Example: Use `context.TestReadings.FromSqlRaw(...)` instead of `sample.TestReadings`

### Authentication/Authorization
- JWT authentication removed for SSO migration
- Migration services in place but authentication endpoints removed
- Review `Program.cs` comments about JWT removal

### Performance Best Practices
- Use `IMemoryCache` for frequently accessed lookup data
- Leverage database indexes (see `Scripts/CreatePerformanceIndexes.sql`)
- Monitor performance via `/api/performance/*` endpoints
- Use async/await throughout for database operations
- Avoid unnecessary tracking with `.AsNoTracking()` where appropriate

### Migration and Legacy Support
- `IRawSqlService` provides compatibility layer for legacy SQL queries
- `OptimizedRawSqlService` wraps legacy queries with performance monitoring
- Migration services track data validation and performance comparison

## Development Environment

**Required Tools:**
- .NET 8.0 SDK
- Node.js (for Angular CLI)
- Docker & Docker Compose
- SQL Server command-line tools (sqlcmd)
- Fish shell preferred (bash compatible)

**Package Manager:**
- Uses `mise` for language/tool version management
- Ensure dotfiles and shell configs support mise CLI

## File Structure Summary

```
/
├── LabResultsApi/              # .NET API project
│   ├── Controllers/            # API controllers
│   ├── Endpoints/              # Minimal API endpoints
│   ├── Services/               # Business logic services
│   ├── Data/                   # EF Core DbContext
│   ├── Models/                 # Entity models
│   ├── DTOs/                   # Data transfer objects
│   ├── Mappings/               # AutoMapper profiles
│   ├── Middleware/             # Custom middleware
│   └── Configuration/          # Configuration models
├── LabResultsApi.Tests/        # Test project
├── lab-results-frontend/       # Angular application
│   ├── src/app/
│   │   ├── features/           # Feature modules
│   │   ├── shared/             # Shared code
│   │   └── layout/             # Layout components
│   ├── e2e/                    # E2E tests
│   └── public/                 # Static assets
├── db-tables/                  # Table creation SQL
├── db-functions/               # SQL functions
├── db-sp/                      # Stored procedures
├── db-views/                   # SQL views
├── db-seeding/                 # CSV seed data
├── db-init/                    # Docker init scripts
├── scripts/                    # Shell scripts for DB management
├── docker-compose.yml          # Database container config
└── Makefile                    # Development shortcuts
```

## Important Documentation Files

- `BUILD_STATUS.md`: Current build status and known issues
- `QUICK_START.md`: Quick database setup guide
- `DATABASE_SETUP.md`: Detailed database setup for Linux
- `LINUX_SETUP_GUIDE.md`: Linux-specific database operations
- `PERFORMANCE_OPTIMIZATIONS.md`: Backend performance details
- `LabResultsApi/PERFORMANCE_OPTIMIZATIONS.md`: API performance guide

## Common Pitfalls

1. **Keyless entity navigation**: Do not attempt to use navigation properties on `TestReadings`, `EmSpectro`, etc.
2. **Authentication code**: JWT has been removed - don't add authentication logic without considering SSO migration
3. **Database updates**: Always test connection with `./test-connection.sh` before running update scripts
4. **Linux environment**: SQL Server authentication required (Windows Auth doesn't work on Linux)
5. **Angular patterns**: Use modern APIs (signals, control flow) instead of legacy patterns
