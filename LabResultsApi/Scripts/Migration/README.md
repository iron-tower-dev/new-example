# Migration System Deployment Scripts

This directory contains scripts and configuration files for deploying the Lab Results API migration system.

## Overview

The migration system provides comprehensive database seeding, SQL validation against legacy systems, and SSO preparation capabilities. These deployment scripts automate the setup process for different environments.

## Files

### Scripts

- **`Deploy-MigrationSystem.ps1`** - PowerShell deployment script for Windows
- **`deploy-migration-system.sh`** - Bash deployment script for Linux/macOS  
- **`Validate-Environment.ps1`** - Environment validation script
- **`01-create-migration-tables.sql`** - Database schema setup script

### Configuration

- **`deployment-config.json`** - Deployment configuration and documentation
- **`README.md`** - This documentation file

## Quick Start

### Windows (PowerShell)

```powershell
# Basic deployment for development
.\Deploy-MigrationSystem.ps1 -Environment Development

# Production deployment with connection strings
.\Deploy-MigrationSystem.ps1 -Environment Production -ConnectionString "Server=prod-db;Database=LabResultsDb;..." -Force

# Validate environment before deployment
.\Validate-Environment.ps1 -Environment Production -ConnectionString "..." -Detailed
```

### Linux/macOS (Bash)

```bash
# Make script executable (first time only)
chmod +x deploy-migration-system.sh

# Basic deployment for development
./deploy-migration-system.sh -e Development

# Production deployment with connection strings
./deploy-migration-system.sh -e Production -c "Server=prod-db;Database=LabResultsDb;..." -f

# Get help
./deploy-migration-system.sh --help
```

## Prerequisites

### System Requirements

- **.NET 6.0 SDK** or later
- **SQL Server 2016** or later (or Azure SQL Database)
- **PowerShell 5.0+** (Windows) or **Bash 4.0+** (Linux/macOS)
- **2GB RAM** minimum, 4GB recommended
- **1GB disk space** minimum, 5GB recommended

### Database Requirements

- SQL Server instance with appropriate permissions
- Database created (default: `LabResultsDb`)
- Connection string with CREATE TABLE permissions
- Optional: Legacy database for validation

### File System Requirements

- Write permissions to project directories
- Access to CSV data files in `db-seeding/`
- Access to SQL scripts in `db-tables/`

## Deployment Process

### 1. Pre-Deployment Validation

Always run environment validation before deployment:

```powershell
# Windows
.\Validate-Environment.ps1 -Environment Production -ConnectionString "..." -Detailed

# Linux/macOS - use PowerShell Core if available
pwsh Validate-Environment.ps1 -Environment Production -ConnectionString "..." -Detailed
```

### 2. Environment-Specific Deployment

#### Development Environment

```powershell
# Windows
.\Deploy-MigrationSystem.ps1 -Environment Development

# Linux/macOS
./deploy-migration-system.sh -e Development
```

**Development Features:**
- Local database connection
- Detailed logging enabled
- Smaller batch sizes for testing
- Legacy validation enabled
- All safety features enabled

#### Staging Environment

```powershell
# Windows
.\Deploy-MigrationSystem.ps1 -Environment Staging -ConnectionString "Server=staging-db;..."

# Linux/macOS  
./deploy-migration-system.sh -e Staging -c "Server=staging-db;..."
```

**Staging Features:**
- Production-like configuration
- Performance monitoring
- Comprehensive validation
- Backup creation
- Error recovery testing

#### Production Environment

```powershell
# Windows
.\Deploy-MigrationSystem.ps1 -Environment Production -ConnectionString "Server=prod-db;..." -LegacyConnectionString "Server=legacy-db;..." -Force

# Linux/macOS
./deploy-migration-system.sh -e Production -c "Server=prod-db;..." -l "Server=legacy-db;..." -f
```

**Production Features:**
- Optimized performance settings
- Minimal logging
- Large batch sizes
- Strict error handling
- Security-focused configuration

### 3. Post-Deployment Verification

After deployment, verify the installation:

1. **Check database tables** - Ensure migration tables were created
2. **Test API build** - Verify the API compiles successfully  
3. **Validate configuration** - Check environment-specific settings
4. **Test endpoints** - Verify migration API endpoints respond
5. **Run sample migration** - Execute a small test migration

## Configuration

### Environment-Specific Settings

Each environment has its own configuration file in `Configuration/`:

- `migration-development.json` - Development settings
- `migration-staging.json` - Staging settings  
- `migration-production.json` - Production settings

### Connection Strings

Update connection strings in the appropriate `appsettings.{Environment}.json` file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=LabResultsDb;...",
    "LegacyDatabase": "Server=legacy-server;Database=LegacyDb;..."
  }
}
```

### Migration Settings

Key configuration options:

```json
{
  "Migration": {
    "DefaultOptions": {
      "MaxConcurrentOperations": 4,
      "OperationTimeoutMinutes": 30,
      "ValidateAgainstLegacy": true
    },
    "Seeding": {
      "BatchSize": 1000,
      "ContinueOnError": true,
      "CommandTimeoutMinutes": 5
    },
    "Validation": {
      "PerformanceThresholdPercent": 20.0,
      "MaxDiscrepanciesToReport": 100
    }
  }
}
```

## Command Reference

### Deploy-MigrationSystem.ps1 (Windows)

```powershell
Deploy-MigrationSystem.ps1 
    -Environment <String>           # Required: Development, Staging, Production
    [-ConnectionString <String>]    # Database connection string
    [-LegacyConnectionString <String>] # Legacy database connection string
    [-SkipDatabaseSetup]           # Skip database schema creation
    [-SkipValidation]              # Skip deployment validation
    [-Force]                       # Force deployment even if exists
    [-LogPath <String>]            # Custom log file path
```

### deploy-migration-system.sh (Linux/macOS)

```bash
deploy-migration-system.sh
    -e|--environment ENVIRONMENT   # Required: Development, Staging, Production
    [-c|--connection-string STRING] # Database connection string
    [-l|--legacy-connection STRING] # Legacy database connection string
    [-s|--skip-database-setup]     # Skip database schema creation
    [-v|--skip-validation]         # Skip deployment validation
    [-f|--force]                   # Force deployment even if exists
    [-p|--log-path PATH]           # Custom log file path
    [-h|--help]                    # Show help message
```

### Validate-Environment.ps1

```powershell
Validate-Environment.ps1
    -Environment <String>           # Required: Environment to validate
    [-ConnectionString <String>]    # Database connection string
    [-LegacyConnectionString <String>] # Legacy database connection string
    [-Detailed]                    # Show detailed validation information
    [-OutputFormat <String>]       # Output format: Console, JSON, CSV
    [-OutputPath <String>]         # Output file path
```

## Troubleshooting

### Common Issues

#### Database Connection Failed

**Symptoms:** Connection timeout or authentication errors

**Solutions:**
1. Verify connection string format
2. Check SQL Server service status
3. Test network connectivity
4. Verify credentials and permissions
5. Check firewall settings

#### Migration Tables Not Created

**Symptoms:** SQL script execution fails

**Solutions:**
1. Grant CREATE TABLE permissions
2. Check database exists
3. Verify SQL script syntax
4. Run script manually to see detailed errors

#### API Build Failed

**Symptoms:** Compilation errors during validation

**Solutions:**
1. Run `dotnet restore` to restore packages
2. Check .NET SDK version compatibility
3. Fix any code compilation errors
4. Verify project file integrity

#### Permission Denied

**Symptoms:** File system access errors

**Solutions:**
1. Run as administrator (Windows) or with sudo (Linux)
2. Grant write permissions to project directories
3. Check antivirus exclusions
4. Verify user account permissions

### Log Files

Check these locations for detailed error information:

- `logs/deployment.log` - Deployment script logs
- `logs/migration/` - Migration operation logs
- Windows Event Log (Application) - System-level errors
- SQL Server Error Log - Database-related errors

### Getting Help

For additional support:

1. **Check the deployment configuration** - Review `deployment-config.json` for detailed troubleshooting guidance
2. **Review log files** - Check deployment and migration logs for specific error messages
3. **Validate environment** - Run the validation script to identify configuration issues
4. **Contact support** - Reach out to the appropriate team based on the issue type

## Security Considerations

### Production Deployments

- Use service accounts for database connections
- Encrypt connection strings in configuration
- Enable audit logging for all migration operations
- Restrict file system permissions to minimum required
- Monitor migration operations for suspicious activity
- Create backups before any production migration

### Connection String Security

- Store sensitive connection strings in secure configuration
- Use Azure Key Vault or similar for production secrets
- Avoid hardcoding credentials in configuration files
- Rotate database passwords regularly
- Use integrated security where possible

## Performance Optimization

### Large Database Migrations

- Increase batch sizes for better throughput
- Use multiple concurrent operations
- Monitor memory usage during migration
- Consider running during off-peak hours
- Plan for extended operation timeouts

### Network Considerations

- Ensure stable network connection to databases
- Consider running migration from same network as databases
- Monitor network bandwidth usage
- Plan for potential connection interruptions

## Maintenance

### Regular Tasks

- Clean up old migration logs and backups
- Monitor disk space usage
- Update deployment scripts as needed
- Review and update configuration settings
- Test deployment process in staging environment

### Updates

When updating the migration system:

1. Test changes in development environment
2. Update deployment scripts if needed
3. Review configuration changes
4. Plan deployment window for production
5. Create rollback plan if needed

## Version History

- **v1.0.0** - Initial deployment script implementation
  - PowerShell and Bash deployment scripts
  - Environment validation
  - Database schema setup
  - Configuration management
  - Comprehensive documentation