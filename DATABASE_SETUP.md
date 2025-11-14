# Lab Results Database Setup

This guide helps you set up the SQL Server database infrastructure for the Lab Results application using Docker Compose on Arch Linux.

## Prerequisites

### 1. Install Docker and Docker Compose

On Arch Linux, install Docker and Docker Compose:

```bash
# Install Docker
sudo pacman -S docker docker-compose

# Start and enable Docker service
sudo systemctl start docker
sudo systemctl enable docker

# Add your user to the docker group (logout/login required)
sudo usermod -aG docker $USER
```

### 2. Verify Installation

```bash
# Check Docker version
docker --version

# Check Docker Compose version
docker-compose --version

# Test Docker (should work without sudo after logout/login)
docker run hello-world
```

## Quick Start

### 1. Start the Database

```bash
# Start the database infrastructure
./scripts/start-db.sh
```

This will:
- Start SQL Server 2022 in a Docker container
- Create the `LabResultsDb` database
- Create all necessary tables
- Seed the database with sample data
- Set up proper networking

### 2. Start the API

```bash
# Navigate to the API directory
cd LabResultsApi

# Run the API (make sure you have .NET 8 installed)
dotnet run
```

The API will connect to the database automatically using the connection string in `appsettings.Development.json`.

### 3. Start the Frontend

```bash
# Navigate to the frontend directory
cd lab-results-frontend

# Install dependencies (if not already done)
npm install

# Start the development server
npm start
```

## Database Management Scripts

### Available Scripts

| Script | Purpose |
|--------|---------|
| `./scripts/start-db.sh` | Start the database infrastructure |
| `./scripts/stop-db.sh` | Stop the database containers |
| `./scripts/reset-db.sh` | Completely reset the database (removes all data) |
| `./scripts/connect-db.sh` | Open SQL command line interface |

### Manual Database Operations

#### Connect to SQL Server

```bash
# Using the script
./scripts/connect-db.sh

# Or manually
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'LabResults123!' -C -d LabResultsDb
```

#### View Database Logs

```bash
# View SQL Server logs
docker-compose logs -f sqlserver

# View all logs
docker-compose logs -f
```

#### Check Container Status

```bash
# Check running containers
docker-compose ps

# Check container health
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'LabResults123!' -C -Q "SELECT @@VERSION"
```

## Database Schema

The database includes the following main tables:

### Core Tables
- **UsedLubeSamples**: Main samples table with sample information
- **Test**: Test definitions and metadata
- **M_And_T_Equip**: MTE (Measuring and Test Equipment) information
- **TestReadings**: Test results and measurements (keyless entity)
- **EmSpectro**: Emission spectroscopy data (keyless entity)

### Sample Data Included

The database is seeded with:
- 8 test definitions (TAN, Water-KF, TBN, Viscosity, etc.)
- 15 pieces of MTE equipment (thermometers, timers, viscometer tubes, barometers)
- 8 sample records with various lubricant types
- Sample test results for demonstration
- Emission spectroscopy data

## Connection Details

| Setting | Value |
|---------|-------|
| **Server** | `localhost,1433` |
| **Database** | `LabResultsDb` |
| **Username** | `sa` |
| **Password** | `LabResults123!` |
| **Connection String** | `Server=localhost,1433;Database=LabResultsDb;User Id=sa;Password=LabResults123!;TrustServerCertificate=true;MultipleActiveResultSets=true` |

## Troubleshooting

### Common Issues

#### 1. Port 1433 Already in Use
```bash
# Check what's using port 1433
sudo netstat -tlnp | grep 1433

# Stop any existing SQL Server instances
sudo systemctl stop mssql-server  # If you have SQL Server installed locally
```

#### 2. Docker Permission Denied
```bash
# Make sure you're in the docker group
groups $USER

# If docker group is missing, add it and logout/login
sudo usermod -aG docker $USER
```

#### 3. Container Won't Start
```bash
# Check Docker daemon status
sudo systemctl status docker

# Check available disk space
df -h

# Check Docker logs
docker-compose logs sqlserver
```

#### 4. Database Connection Fails
```bash
# Check if container is running
docker-compose ps

# Test connection manually
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'LabResults123!' -C -Q "SELECT 1"

# Check API connection string in appsettings.Development.json
```

### Reset Everything

If you encounter persistent issues:

```bash
# Stop everything
./scripts/stop-db.sh

# Remove all containers and volumes
docker-compose down -v
docker system prune -f

# Start fresh
./scripts/start-db.sh
```

## Development Workflow

### Typical Development Session

1. **Start Database**
   ```bash
   ./scripts/start-db.sh
   ```

2. **Start API** (in new terminal)
   ```bash
   cd LabResultsApi
   dotnet run
   ```

3. **Start Frontend** (in new terminal)
   ```bash
   cd lab-results-frontend
   npm start
   ```

4. **When Done**
   ```bash
   # Stop database (optional - can leave running)
   ./scripts/stop-db.sh
   ```

### Making Database Changes

1. **Modify Scripts**: Update files in `db-init/` directory
2. **Reset Database**: Run `./scripts/reset-db.sh`
3. **Test Changes**: Verify with `./scripts/connect-db.sh`

## Security Notes

⚠️ **Important**: This setup is for development only!

- The SA password is hardcoded and simple
- The database is accessible without encryption
- No backup or recovery mechanisms are configured
- Data is stored in Docker volumes (can be lost)

For production deployment, implement proper security measures:
- Use strong, randomly generated passwords
- Enable SSL/TLS encryption
- Set up proper backup strategies
- Use dedicated database servers
- Implement network security

## Performance Tips

### For Development
- Keep the database container running between sessions
- Use `docker-compose logs -f sqlserver` to monitor performance
- Consider increasing Docker memory allocation if needed

### Docker Resource Allocation
```bash
# Check Docker resource usage
docker stats

# Adjust Docker Desktop settings if needed (memory, CPU)
```

## Additional Resources

- [SQL Server on Docker Documentation](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)