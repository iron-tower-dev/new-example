# Migration System Troubleshooting Guide and FAQ

## Quick Reference

### Emergency Contacts
- **System Administrator**: admin@your-company.com
- **Database Administrator**: dba@your-company.com
- **Development Team**: dev-team@your-company.com
- **Emergency Hotline**: +1-555-EMERGENCY

### Critical Commands
```bash
# Check system status
curl -f http://localhost:5000/health

# View recent logs
tail -f /var/log/migration-system/migration-*.log

# Restart service
sudo systemctl restart migration-system

# Check database connectivity
sqlcmd -S localhost -d LabResults -Q "SELECT 1"
```

## Common Issues and Solutions

### 1. Migration Won't Start

#### Symptoms
- "Migration failed to start" error message
- HTTP 500 error when starting migration
- Service appears to hang on startup

#### Diagnostic Steps
```bash
# Check service status
sudo systemctl status migration-system

# Check application logs
tail -100 /var/log/migration-system/migration-*.log

# Test database connection
sqlcmd -S "server" -d "database" -U "username" -P "password" -Q "SELECT 1"

# Check file permissions
ls -la /opt/migration-system/data/csv/
ls -la /opt/migration-system/data/sql/
```

#### Common Causes and Solutions

**Database Connection Issues**
```bash
# Solution 1: Fix connection string
nano /opt/migration-system/config/appsettings.json
# Verify server name, database name, credentials

# Solution 2: Test connectivity
telnet sql-server 1433
# If fails, check network/firewall

# Solution 3: Check SQL Server status
sudo systemctl status mssql-server
# Restart if needed: sudo systemctl restart mssql-server
```

**File Permission Issues**
```bash
# Solution: Fix permissions
sudo chown -R migration-system:migration-system /opt/migration-system/
sudo chmod 755 /opt/migration-system/data/
sudo chmod 644 /opt/migration-system/data/csv/*.csv
sudo chmod 644 /opt/migration-system/data/sql/*.sql
```

**Configuration Issues**
```bash
# Solution: Validate configuration
python3 -m json.tool /opt/migration-system/config/appsettings.json
# Fix any JSON syntax errors

# Check required settings
grep -i "connectionstring\|csvdirectory\|sqldirectory" /opt/migration-system/config/appsettings.json
```

**Insufficient Disk Space**
```bash
# Check disk usage
df -h /opt/migration-system/
df -h /var/log/

# Solution: Clean up space
sudo find /var/log/migration-system/ -name "*.log" -mtime +30 -delete
sudo find /opt/migration-system/data/temp/ -type f -mtime +7 -delete
```

### 2. Migration Runs Slowly

#### Symptoms
- Migration takes much longer than expected
- Progress indicators move very slowly
- High CPU or memory usage

#### Diagnostic Steps
```bash
# Monitor system resources
top -p $(pgrep dotnet)
iostat -x 1 10
free -h

# Check database performance
# Run SQL Server Activity Monitor or:
SELECT 
    r.session_id,
    r.status,
    r.command,
    r.percent_complete,
    r.estimated_completion_time/1000/60 AS estimated_minutes,
    t.text
FROM sys.dm_exec_requests r
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
WHERE r.session_id > 50;
```

#### Solutions

**Reduce Batch Size**
```json
{
  "MigrationSettings": {
    "DefaultBatchSize": 500,
    "MaxConcurrentTables": 3
  }
}
```

**Optimize Database**
```sql
-- Update statistics
EXEC sp_updatestats;

-- Check for missing indexes
SELECT 
    migs.avg_total_user_cost * (migs.avg_user_impact / 100.0) * (migs.user_seeks + migs.user_scans) AS improvement_measure,
    'CREATE INDEX [missing_index_' + CONVERT(varchar, mig.index_group_handle) + '_' + CONVERT(varchar, mid.index_handle) + ']'
    + ' ON ' + mid.statement + ' (' + ISNULL(mid.equality_columns,'') 
    + CASE WHEN mid.equality_columns IS NOT NULL AND mid.inequality_columns IS NOT NULL THEN ',' ELSE '' END
    + ISNULL(mid.inequality_columns, '') + ')' 
    + ISNULL(' INCLUDE (' + mid.included_columns + ')', '') AS create_index_statement
FROM sys.dm_db_missing_index_groups mig
INNER JOIN sys.dm_db_missing_index_group_stats migs ON migs.group_handle = mig.index_group_handle
INNER JOIN sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
WHERE migs.avg_total_user_cost * (migs.avg_user_impact / 100.0) * (migs.user_seeks + migs.user_scans) > 10
ORDER BY improvement_measure DESC;
```

**Increase System Resources**
- Add more RAM if memory usage is high
- Use SSD storage for better I/O performance
- Increase CPU cores for parallel processing

### 3. Data Validation Errors

#### Symptoms
- Many records are skipped during migration
- "Data validation failed" error messages
- Inconsistent record counts between source and destination

#### Common Validation Errors

**Primary Key Violations**
```
Error: Violation of PRIMARY KEY constraint 'PK_Test'. Cannot insert duplicate key in object 'dbo.Test'.
```

**Solution:**
```sql
-- Find duplicate records in CSV data
SELECT TestId, COUNT(*) 
FROM TempTestData 
GROUP BY TestId 
HAVING COUNT(*) > 1;

-- Remove duplicates (keep first occurrence)
WITH CTE AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY TestId ORDER BY (SELECT NULL)) as rn
    FROM TempTestData
)
DELETE FROM CTE WHERE rn > 1;
```

**Foreign Key Violations**
```
Error: The INSERT statement conflicted with the FOREIGN KEY constraint "FK_Test_Sample". 
```

**Solution:**
```sql
-- Find orphaned records
SELECT t.* 
FROM TempTestData t
LEFT JOIN Sample s ON t.SampleId = s.SampleId
WHERE s.SampleId IS NULL;

-- Either remove orphaned records or create missing parent records
DELETE FROM TempTestData 
WHERE SampleId NOT IN (SELECT SampleId FROM Sample);
```

**Data Type Conversion Errors**
```
Error: Conversion failed when converting the varchar value 'N/A' to data type int.
```

**Solution:**
```sql
-- Find invalid data
SELECT * FROM TempTestData 
WHERE ISNUMERIC(NumericColumn) = 0 AND NumericColumn IS NOT NULL;

-- Clean data
UPDATE TempTestData 
SET NumericColumn = NULL 
WHERE ISNUMERIC(NumericColumn) = 0;
```

**Date Format Issues**
```
Error: Conversion failed when converting date and/or time from character string.
```

**Solution:**
```sql
-- Standardize date formats
UPDATE TempTestData 
SET DateColumn = CONVERT(datetime, DateColumn, 101) -- MM/dd/yyyy format
WHERE ISDATE(DateColumn) = 1;

-- Handle invalid dates
UPDATE TempTestData 
SET DateColumn = NULL 
WHERE ISDATE(DateColumn) = 0;
```

### 4. SQL Validation Failures

#### Symptoms
- "Query results don't match legacy system" errors
- Performance warnings in validation reports
- Timeout errors during validation

#### Common Causes and Solutions

**Schema Differences**
```sql
-- Compare table schemas
SELECT 
    c1.COLUMN_NAME,
    c1.DATA_TYPE as Current_Type,
    c1.IS_NULLABLE as Current_Nullable,
    c2.DATA_TYPE as Legacy_Type,
    c2.IS_NULLABLE as Legacy_Nullable
FROM INFORMATION_SCHEMA.COLUMNS c1
FULL OUTER JOIN LegacyDB.INFORMATION_SCHEMA.COLUMNS c2 
    ON c1.COLUMN_NAME = c2.COLUMN_NAME 
    AND c1.TABLE_NAME = c2.TABLE_NAME
WHERE c1.TABLE_NAME = 'Test'
    AND (c1.DATA_TYPE != c2.DATA_TYPE OR c1.IS_NULLABLE != c2.IS_NULLABLE);
```

**Data Transformation Differences**
```sql
-- Check for rounding differences in calculations
SELECT 
    TestId,
    ROUND(CurrentValue, 2) as Current_Rounded,
    ROUND(LegacyValue, 2) as Legacy_Rounded
FROM ValidationResults 
WHERE ABS(CurrentValue - LegacyValue) > 0.01;
```

**Query Timeout Issues**
```json
{
  "MigrationSettings": {
    "CommandTimeout": 600,
    "ValidationTimeout": 300
  }
}
```

### 5. SSO Migration Issues

#### Symptoms
- Application won't start after authentication removal
- Users still prompted for login
- Authorization errors

#### Solutions

**Backup Not Created**
```bash
# Manually create backup
mkdir -p /opt/migration-system/backups/auth/manual_backup_$(date +%Y%m%d_%H%M%S)
cp /opt/migration-system/config/appsettings.json /opt/migration-system/backups/auth/manual_backup_*/
cp /opt/migration-system/app/Program.cs /opt/migration-system/backups/auth/manual_backup_*/
```

**Authentication Still Active**
```csharp
// Check Program.cs for remaining authentication code
grep -n "UseAuthentication\|UseAuthorization\|AddAuthentication" /opt/migration-system/app/Program.cs

// Remove any remaining authentication middleware
sed -i '/UseAuthentication/d' /opt/migration-system/app/Program.cs
sed -i '/UseAuthorization/d' /opt/migration-system/app/Program.cs
```

**Frontend Still Requires Authentication**
```typescript
// Check Angular routes for auth guards
grep -r "canActivate" /opt/migration-system/frontend/src/app/

// Remove auth guards from routes
// Edit route configuration files to remove canActivate properties
```

### 6. Performance Issues

#### Symptoms
- High CPU usage
- High memory consumption
- Slow response times
- Database connection pool exhaustion

#### Diagnostic Commands
```bash
# Check CPU usage
top -p $(pgrep dotnet)
htop

# Check memory usage
free -h
ps aux --sort=-%mem | head -10

# Check disk I/O
iostat -x 1 5
iotop

# Check network usage
iftop
nethogs
```

#### Solutions

**High CPU Usage**
```json
{
  "MigrationSettings": {
    "MaxConcurrentTables": 2,
    "DefaultBatchSize": 500
  }
}
```

**High Memory Usage**
```json
{
  "PerformanceSettings": {
    "EnableMemoryOptimization": true,
    "MaxMemoryUsageMB": 2048,
    "GarbageCollectionMode": "Server"
  }
}
```

**Database Connection Issues**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LabResults;Pooling=true;Min Pool Size=5;Max Pool Size=100;Connection Timeout=30;"
  }
}
```

## Frequently Asked Questions (FAQ)

### General Questions

**Q: How long does a typical migration take?**
A: Migration time depends on data volume:
- Small database (< 100K records): 5-15 minutes
- Medium database (100K-1M records): 15-60 minutes  
- Large database (> 1M records): 1-4 hours

**Q: Can I run migrations during business hours?**
A: Yes, but consider:
- Database performance impact during migration
- Potential downtime if issues occur
- User notification requirements
- Backup and rollback procedures

**Q: What happens if a migration fails partway through?**
A: The system will:
- Stop processing new data
- Log detailed error information
- Maintain existing data (no automatic rollback)
- Provide options to resume or restart migration

**Q: Can I cancel a running migration?**
A: Yes, you can cancel migrations through:
- Web dashboard "Cancel Migration" button
- API endpoint: `POST /api/Migration/cancel/{migrationId}`
- Emergency stop: restart the migration service

### Configuration Questions

**Q: Where are configuration files located?**
A: Configuration files are located at:
- Docker: `/app/config/` (mounted from host)
- Linux: `/opt/migration-system/config/`
- Windows: `C:\inetpub\wwwroot\migration-system\config\`

**Q: How do I change the batch size for better performance?**
A: Edit the configuration file:
```json
{
  "MigrationSettings": {
    "DefaultBatchSize": 1000
  }
}
```
Smaller values (500-1000) for slower systems, larger values (2000-5000) for faster systems.

**Q: Can I exclude certain tables from migration?**
A: Yes, use the exclude tables option:
```json
{
  "excludeTables": ["AuditLog", "TempData", "SystemLog"]
}
```

**Q: How do I configure email notifications?**
A: Update the notification settings:
```json
{
  "NotificationSettings": {
    "EnableEmailNotifications": true,
    "SmtpSettings": {
      "Host": "smtp.your-company.com",
      "Port": 587,
      "Username": "migration@your-company.com",
      "Password": "your-password",
      "EnableSsl": true
    }
  }
}
```

### Data Questions

**Q: What CSV file format is required?**
A: CSV files should:
- Have headers matching database column names
- Use comma separators
- Quote text fields containing commas
- Use standard date formats (YYYY-MM-DD or MM/DD/YYYY)
- Encode in UTF-8

**Q: How do I handle missing or NULL values in CSV files?**
A: Use these approaches:
- Empty fields for NULL values: `"John","","Manager"`
- Explicit NULL text: `"John","NULL","Manager"`
- Configure default values in migration settings

**Q: Can I migrate only specific tables?**
A: Yes, use selective migration:
1. Choose "Selective Migration" option
2. Select specific tables to include
3. Ensure dependent tables are also included

**Q: What happens to existing data during migration?**
A: Depends on configuration:
- `clearExistingData: true` - All existing data is deleted first
- `clearExistingData: false` - New data is appended to existing data

### Security Questions

**Q: Is the migration system secure?**
A: Security features include:
- HTTPS encryption for all communications
- Database connection encryption
- Role-based access control
- Audit logging of all operations
- Secure file handling

**Q: Who can access the migration system?**
A: Access is controlled by:
- Authentication requirements (configurable)
- Role-based permissions
- Network access controls
- Database user permissions

**Q: Are passwords stored securely?**
A: Yes:
- Configuration passwords are encrypted at rest
- Database connections use secure authentication
- No passwords are logged in plain text

**Q: How is sensitive data protected during migration?**
A: Protection measures include:
- Data masking in logs
- Encrypted database connections
- Secure temporary file handling
- Automatic cleanup of temporary data

### Troubleshooting Questions

**Q: Migration fails with "Access Denied" error. What should I do?**
A: Check these items:
1. Database user permissions
2. File system permissions
3. Network connectivity
4. Firewall settings
5. Service account privileges

**Q: Why is my migration running slowly?**
A: Common causes:
- Large batch sizes
- Database performance issues
- Network latency
- Insufficient system resources
- Missing database indexes

**Q: How do I view detailed error information?**
A: Access error details through:
- Migration dashboard error section
- Log files: `/var/log/migration-system/`
- API endpoint: `GET /api/Migration/report/{migrationId}`
- Database audit tables

**Q: Can I resume a failed migration?**
A: Currently, migrations cannot be resumed. You must:
1. Identify and fix the cause of failure
2. Start a new migration
3. Consider using selective migration for remaining tables

### Maintenance Questions

**Q: How often should I run migrations?**
A: Frequency depends on your needs:
- Development: As needed for testing
- Staging: Weekly or before deployments
- Production: Monthly or quarterly

**Q: What maintenance tasks are required?**
A: Regular maintenance includes:
- Log file cleanup
- Database backup verification
- Performance monitoring
- Security updates
- Configuration reviews

**Q: How do I backup the migration system?**
A: Backup these components:
- Configuration files
- Database (application and migration metadata)
- CSV and SQL files
- Custom scripts and extensions

**Q: How do I update the migration system?**
A: Follow these steps:
1. Create full system backup
2. Stop migration service
3. Deploy new version
4. Run database migrations
5. Test functionality
6. Start service

### Integration Questions

**Q: Can I integrate the migration system with other tools?**
A: Yes, integration options include:
- REST API for programmatic access
- Webhook notifications
- PowerShell/Bash scripts
- CI/CD pipeline integration

**Q: How do I automate migrations?**
A: Use these approaches:
- Scheduled tasks/cron jobs
- CI/CD pipeline triggers
- PowerShell/Bash scripts
- API automation

**Q: Can I customize the migration process?**
A: Yes, customization options include:
- Custom data processors
- Custom validators
- Custom notification providers
- Configuration-based behavior changes

**Q: How do I monitor migration system health?**
A: Monitoring options include:
- Built-in health check endpoints
- Application performance monitoring (APM)
- Log aggregation and analysis
- Custom monitoring scripts
- Database performance monitoring

## Error Code Reference

### System Errors (1000-1999)

| Code | Error | Description | Solution |
|------|-------|-------------|----------|
| 1001 | SERVICE_UNAVAILABLE | Migration service is not running | Restart migration service |
| 1002 | CONFIGURATION_ERROR | Invalid configuration settings | Check configuration file syntax |
| 1003 | INSUFFICIENT_PERMISSIONS | Inadequate file/database permissions | Review and fix permissions |
| 1004 | DISK_SPACE_LOW | Insufficient disk space | Free up disk space |
| 1005 | MEMORY_EXHAUSTED | Out of memory error | Increase available memory or reduce batch size |

### Database Errors (2000-2999)

| Code | Error | Description | Solution |
|------|-------|-------------|----------|
| 2001 | CONNECTION_FAILED | Cannot connect to database | Check connection string and database status |
| 2002 | AUTHENTICATION_FAILED | Database authentication failed | Verify credentials and permissions |
| 2003 | TIMEOUT_EXCEEDED | Database operation timeout | Increase timeout or optimize query |
| 2004 | CONSTRAINT_VIOLATION | Database constraint violation | Check data integrity and constraints |
| 2005 | DEADLOCK_DETECTED | Database deadlock occurred | Retry operation or optimize queries |

### Data Errors (3000-3999)

| Code | Error | Description | Solution |
|------|-------|-------------|----------|
| 3001 | CSV_PARSE_ERROR | Error parsing CSV file | Check CSV format and encoding |
| 3002 | DATA_VALIDATION_FAILED | Data validation failed | Review and clean source data |
| 3003 | DUPLICATE_KEY | Duplicate primary key found | Remove duplicates from source data |
| 3004 | FOREIGN_KEY_VIOLATION | Foreign key constraint violation | Ensure parent records exist |
| 3005 | DATA_TYPE_MISMATCH | Data type conversion error | Fix data types in source data |

### Migration Errors (4000-4999)

| Code | Error | Description | Solution |
|------|-------|-------------|----------|
| 4001 | MIGRATION_IN_PROGRESS | Another migration is running | Wait for current migration to complete |
| 4002 | MIGRATION_NOT_FOUND | Migration ID not found | Check migration ID and try again |
| 4003 | VALIDATION_FAILED | SQL validation failed | Review query differences and fix |
| 4004 | BACKUP_FAILED | Backup creation failed | Check backup directory permissions |
| 4005 | ROLLBACK_FAILED | Rollback operation failed | Perform manual rollback |

## Best Practices Summary

### Before Migration
- [ ] Create full system backup
- [ ] Verify CSV file formats and data quality
- [ ] Test database connections
- [ ] Check available disk space
- [ ] Review configuration settings
- [ ] Notify stakeholders of planned migration

### During Migration
- [ ] Monitor progress actively
- [ ] Watch for error messages
- [ ] Check system resource usage
- [ ] Be prepared to cancel if issues arise
- [ ] Document any unexpected behavior

### After Migration
- [ ] Verify data integrity
- [ ] Test application functionality
- [ ] Review migration reports
- [ ] Update documentation
- [ ] Clean up temporary files
- [ ] Schedule next migration if needed

### Performance Optimization
- [ ] Use appropriate batch sizes
- [ ] Monitor database performance
- [ ] Optimize queries and indexes
- [ ] Consider off-peak scheduling
- [ ] Scale resources as needed

### Security
- [ ] Use encrypted connections
- [ ] Implement proper access controls
- [ ] Audit all operations
- [ ] Protect sensitive data
- [ ] Keep system updated

## Getting Additional Help

### Documentation Resources
- [User Guide](USER_GUIDE.md) - Step-by-step procedures
- [Administrator Guide](ADMINISTRATOR_GUIDE.md) - System administration
- [API Reference](MIGRATION_API_REFERENCE.md) - API documentation
- [Developer Guide](DEVELOPER_GUIDE.md) - Extension development

### Support Channels
1. **Self-Service**: Check documentation and system health dashboard
2. **Help Desk**: Submit ticket through internal system
3. **Email Support**: migration-support@your-company.com
4. **Emergency**: Call emergency hotline for critical issues

### Information to Include in Support Requests
- Migration ID (if applicable)
- Exact error messages
- Steps taken before the issue
- System environment (dev/staging/production)
- Configuration settings used
- Screenshots of error screens
- Recent log entries

Remember: Most issues can be resolved by checking logs, verifying configuration, and ensuring proper permissions. Always try basic troubleshooting steps before escalating to support.