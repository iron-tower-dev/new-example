# Database Structure Update Guide

This guide provides instructions for updating your database structure to match the complete schema defined in the SQL files located in the `db-tables`, `db-functions`, `db-sp`, and `db-views` directories.

## Overview

The database update process includes:
- **Tables**: Core data structures from `db-tables/` directory
- **Functions**: User-defined functions from `db-functions/` directory  
- **Stored Procedures**: Business logic procedures from `db-sp/` directory
- **Views**: Data access views from `db-views/` directory

## Prerequisites

1. **SQL Server Access**: Ensure you have appropriate permissions to create/modify database objects
2. **SQL Server Tools**: One of the following:
   - SQL Server Management Studio (SSMS) - Windows only
   - sqlcmd command-line utility (available on Linux)
   - PowerShell with SqlServer module - Windows only
3. **Linux Users**: Install mssql-tools package:
   ```bash
   # Arch Linux
   yay -S mssql-tools
   # or
   sudo pacman -S mssql-tools
   
   # Ubuntu/Debian
   sudo apt-get install mssql-tools
   
   # RHEL/CentOS/Fedora
   sudo yum install mssql-tools
   ```
4. **Backup**: Create a backup of your existing database before proceeding

## Update Methods

### Method 1: PowerShell Script (Recommended)

The PowerShell script provides the most comprehensive error handling and logging.

#### Prerequisites
- PowerShell 5.1 or later
- SQL Server PowerShell module (optional but recommended)

#### Usage
```powershell
# Using Windows Authentication
.\Update-DatabaseStructure.ps1 -ServerName "localhost" -DatabaseName "lubelab_dev"

# Using SQL Server Authentication
.\Update-DatabaseStructure.ps1 -ServerName "localhost" -DatabaseName "lubelab_dev" -Username "sa" -Password "yourpassword" -UseWindowsAuth:$false
```

#### Parameters
- `ServerName`: SQL Server instance name
- `DatabaseName`: Target database name
- `Username`: SQL Server username (if not using Windows auth)
- `Password`: SQL Server password (if not using Windows auth)
- `UseWindowsAuth`: Use Windows Authentication (default: true)

### Method 2: Bash Script (Linux/macOS)

Cross-platform bash script with comprehensive error handling.

#### Prerequisites
- bash shell
- sqlcmd utility (part of SQL Server Command Line Utilities)

#### Usage
```bash
# Using Windows Authentication (default)
./update-database.sh

# Custom server and database
./update-database.sh -s myserver -d mydb

# Using SQL Server Authentication
./update-database.sh -s localhost -d lubelab_dev -q -u sa -p yourpassword

# Show help
./update-database.sh --help
```

#### Parameters
- `-s, --server`: SQL Server instance name (default: localhost)
- `-d, --database`: Database name (default: lubelab_dev)
- `-u, --username`: SQL Server username (for SQL auth)
- `-p, --password`: SQL Server password (for SQL auth)
- `-w, --windows-auth`: Use Windows authentication (default)
- `-q, --sql-auth`: Use SQL Server authentication
- `-h, --help`: Show help message

### Method 3: Fish Shell Script

For Fish shell users, a wrapper script is provided.

#### Usage
```fish
# Using Windows Authentication (default)
./update-database.fish

# Custom server and database
./update-database.fish -s myserver -d mydb

# Using SQL Server Authentication
./update-database.fish -s localhost -d lubelab_dev -q -u sa -p yourpassword
```

### Method 4: Batch Script (Windows)

Simple command-line batch script using sqlcmd for Windows users.

#### Prerequisites
- sqlcmd utility (part of SQL Server Command Line Utilities)

#### Usage
1. Edit `update-database.bat` to configure your server settings:
   ```batch
   set SERVER_NAME=localhost
   set DATABASE_NAME=lubelab_dev
   set USE_WINDOWS_AUTH=1
   ```
2. Run the batch file:
   ```cmd
   update-database.bat
   ```

### Method 5: Manual Execution

Execute SQL files manually in SQL Server Management Studio.

#### Order of Execution

1. **Preparation**: Execute `update-database-structure.sql`
2. **Core Tables** (in order):
   - `db-tables/site.sql`
   - `db-tables/Component.sql`
   - `db-tables/Location.sql`
   - `db-tables/MeasurementType.sql`
   - `db-tables/TestStand.sql`
   - `db-tables/Test.sql`
   - `db-tables/UsedLubeSamples.sql`
   - `db-tables/TestReadings.sql`

3. **Lookup Tables**:
   - `db-tables/LookupList.sql`
   - `db-tables/NAS_lookup.sql`
   - `db-tables/NLGILookup.sql`
   - `db-tables/ParticleTypeDefinition.sql`
   - `db-tables/ParticleSubTypeDefinition.sql`
   - `db-tables/ParticleSubTypeCategoryDefinition.sql`

4. **Remaining Tables**: Execute all other files in `db-tables/`

5. **Functions**: Execute all files in `db-functions/`

6. **Stored Procedures**: Execute all files in `db-sp/`

7. **Views**: Execute all files in `db-views/`

## Key Tables Overview

Based on the FileUploads.sql example provided, the database includes:

### Core Tables
- **UsedLubeSamples**: Main sample tracking table
- **Test**: Test definitions and configurations
- **TestReadings**: Test results and measurements
- **FileUploads**: File attachments for samples and tests

### Lookup Tables
- **Component**: Equipment components
- **Location**: Sample locations
- **TestStand**: Test equipment definitions
- **ParticleTypeDefinition**: Particle analysis definitions

### Data Tables
- **EmSpectro**: Emission spectroscopy results
- **Ferrogram**: Ferrography analysis data
- **FTIR**: FTIR analysis results
- **ParticleCount**: Particle count data
- **rheometer**: Rheometer test results

## Important Notes

### Database Dependencies
- Tables are created in dependency order to avoid foreign key conflicts
- Views depend on tables and functions, so they're created last
- Some tables may have circular dependencies that require careful ordering

### Data Preservation
- The update scripts focus on structure, not data migration
- Existing data should be preserved, but verify after update
- Consider running data validation queries after the update

### Error Handling
- Scripts include error checking and will report failed operations
- Review error messages carefully - some may be expected (e.g., "object already exists")
- Critical errors should be investigated and resolved

## Post-Update Verification

After running the update scripts, verify the database structure:

```sql
-- Check table count
SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'

-- Check function count  
SELECT COUNT(*) as FunctionCount FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION'

-- Check stored procedure count
SELECT COUNT(*) as ProcedureCount FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'

-- Check view count
SELECT COUNT(*) as ViewCount FROM INFORMATION_SCHEMA.VIEWS

-- Verify key tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('UsedLubeSamples', 'Test', 'TestReadings', 'FileUploads')
ORDER BY TABLE_NAME
```

## Troubleshooting

### Common Issues

1. **Permission Errors**
   - Ensure your account has db_ddladmin or db_owner permissions
   - For system objects, sysadmin permissions may be required

2. **Object Already Exists**
   - These warnings are usually safe to ignore
   - The scripts attempt to drop objects before creating them

3. **Missing Dependencies**
   - Ensure all SQL files are present in their respective directories
   - Check that table creation order is maintained

4. **Timeout Errors**
   - Large scripts may timeout - increase command timeout settings
   - Consider running scripts in smaller batches

### Getting Help

If you encounter issues:
1. Check the error messages for specific object names and line numbers
2. Verify the SQL file exists and is readable
3. Check database permissions
4. Review the execution order for dependency issues

## Files Generated

This update process creates the following helper files:
- `update-database-structure.sql`: Preparation script
- `Update-DatabaseStructure.ps1`: PowerShell automation script
- `update-database.bat`: Batch file automation script
- `DATABASE_UPDATE_README.md`: This documentation file

## Next Steps

After successfully updating the database structure:
1. Test your application to ensure compatibility
2. Update your Entity Framework models if using .NET
3. Verify that all existing functionality works correctly
4. Consider updating your API documentation to reflect any new capabilities
5. Run any necessary data seeding or migration scripts