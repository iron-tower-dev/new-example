# Database Update Guide for Linux (Arch Linux)

This guide is specifically tailored for your Arch Linux system with Fish shell.

## Quick Start

1. **Check Prerequisites**:
   ```bash
   ./check-prerequisites.sh
   ```

2. **Run Database Update**:
   ```fish
   # For Fish shell users (recommended for you)
   ./update-database.fish -s your_server -d lubelab_dev

   # Or using bash
   ./update-database.sh -s your_server -d lubelab_dev
   ```

## Your System Setup

Based on your environment:
- **OS**: Arch Linux
- **Primary Shell**: Fish
- **Secondary Shell**: Bash (available)
- **SQL Tools**: sqlcmd (installed)

## Files Created for You

### Executable Scripts
- `update-database.sh` - Main bash script with full functionality
- `update-database.fish` - Fish shell wrapper (recommended for you)
- `check-prerequisites.sh` - Verifies your system is ready

### Configuration Files
- `update-database-structure.sql` - Preparation script for database
- `DATABASE_UPDATE_README.md` - Complete documentation
- `LINUX_SETUP_GUIDE.md` - This file

## Usage Examples for Your Setup

### Basic Usage (Fish Shell)
```fish
# IMPORTANT: Linux requires SQL Server authentication (Windows auth doesn't work)
# Test connection first (uses defaults from your API config)
./test-connection.sh

# Run database update with default settings (matches your API configuration)
./update-database.fish

# Or specify custom credentials
./update-database.fish -s localhost -d LabResultsDb -u sa -p LabResults123!
```

### Basic Usage (Bash)
```bash
# IMPORTANT: Linux requires SQL Server authentication (Windows auth doesn't work)
# Test connection first (uses defaults from your API config)
./test-connection.sh

# Run database update with default settings (matches your API configuration)
./update-database.sh

# Or specify custom credentials
./update-database.sh -s localhost -d LabResultsDb -u sa -p LabResults123!
```

## What the Scripts Do

1. **Drop existing database objects** in the correct order (views → procedures → functions)
2. **Create tables** in dependency order (core tables first, then dependent tables)
3. **Create functions** from `db-functions/` directory
4. **Create stored procedures** from `db-sp/` directory  
5. **Create views** from `db-views/` directory
6. **Provide detailed progress** and error reporting

## Directory Structure Expected

```
your-project/
├── db-tables/          # 56 SQL files for table creation
├── db-functions/       # 8 SQL files for functions
├── db-sp/             # 18 SQL files for stored procedures
├── db-views/          # 93 SQL files for views
├── update-database.sh      # Main bash script
├── update-database.fish    # Fish wrapper script
└── check-prerequisites.sh  # System check script
```

## Troubleshooting for Linux

### If sqlcmd is not found:
```bash
# Install on Arch Linux
yay -S mssql-tools
# or
sudo pacman -S mssql-tools

# Add to PATH if needed
echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc
echo 'set -gx PATH $PATH /opt/mssql-tools/bin' >> ~/.config/fish/config.fish
```

### If you get permission errors:
```bash
# Make scripts executable
chmod +x update-database.sh
chmod +x update-database.fish
chmod +x check-prerequisites.sh
```

### If you get "Login failed for user ''" error:
This means Windows Authentication was attempted but failed (expected on Linux).

**Solution**: Use SQL Server Authentication instead:
```bash
# Test your connection first
./test-connection.sh -u sa -p your_password

# Then run the update with explicit credentials
./update-database.sh -u sa -p your_password
```

### If connection fails:
- Verify SQL Server is running and accessible
- Check firewall settings (SQL Server default port is 1433)
- Verify connection string parameters
- Test connection manually: `sqlcmd -S your_server -d your_database -U username -P password`
- Ensure SQL Server is configured for SQL Server Authentication (not just Windows Auth)

## Expected Results

After successful execution, you should have:
- **Tables**: All 56+ tables from your SQL files
- **Functions**: 8 user-defined functions
- **Stored Procedures**: 18 business logic procedures
- **Views**: 93 data access views

This will resolve your test type issues by ensuring the database has the correct structure and no duplicate/incorrect test definitions.

## Verification Commands

After running the update, verify with these SQL commands:

```sql
-- Check table count
SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Check if key tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('UsedLubeSamples', 'Test', 'TestReadings', 'FileUploads')
ORDER BY TABLE_NAME;

-- Check function count
SELECT COUNT(*) as FunctionCount FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION';

-- Check stored procedure count
SELECT COUNT(*) as ProcedureCount FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE';

-- Check view count
SELECT COUNT(*) as ViewCount FROM INFORMATION_SCHEMA.VIEWS;
```

## Next Steps

1. Run the database update using your preferred method
2. Test your API to ensure it connects properly
3. Verify the frontend test selection shows correct test types
4. Manually seed any required reference data using the CSV files in `db-seeding/`