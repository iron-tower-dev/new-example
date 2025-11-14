# Migration System User Guide

## Overview

The Migration System provides a comprehensive solution for migrating laboratory test data, validating SQL queries, and preparing for Single Sign-On integration. This guide walks you through common migration scenarios and provides step-by-step instructions for successful migrations.

## Getting Started

### Prerequisites

Before starting a migration, ensure you have:

- **Database Access**: Connection to both current and legacy databases
- **File Access**: Access to CSV data files and SQL schema files
- **Permissions**: Administrative permissions for migration operations
- **Backup**: Current system backup (recommended)

### Accessing the Migration System

1. **Web Interface**: Navigate to `https://your-domain.com/migration`
2. **API Access**: Use the REST API at `https://your-domain.com/api/Migration`
3. **Command Line**: Use the migration CLI tool (if available)

## Migration Scenarios

### Scenario 1: Complete Database Migration

This is the most common scenario where you want to completely refresh your database with production data.

#### Step-by-Step Process

1. **Prepare for Migration**
   ```
   ✓ Backup current database
   ✓ Verify CSV files are in /db-seeding/ directory
   ✓ Verify SQL schema files are in /db-tables/ directory
   ✓ Check database connections
   ```

2. **Start Migration**
   - Navigate to Migration Dashboard
   - Click "Start New Migration"
   - Select "Complete Database Migration"
   - Configure options:
     - ✅ Clear existing data
     - ✅ Create missing tables
     - ✅ Validate against legacy system
     - ❌ Remove authentication (unless preparing for SSO)

3. **Monitor Progress**
   - Watch real-time progress indicators
   - Review table-by-table status
   - Monitor error messages and warnings

4. **Review Results**
   - Check migration summary report
   - Verify data integrity
   - Review validation results
   - Address any errors or warnings

#### Expected Timeline
- **Small Database** (< 100K records): 5-15 minutes
- **Medium Database** (100K-1M records): 15-60 minutes
- **Large Database** (> 1M records): 1-4 hours

### Scenario 2: Selective Table Migration

When you only need to refresh specific tables with new data.

#### Step-by-Step Process

1. **Identify Target Tables**
   - Determine which tables need updating
   - Check dependencies between tables
   - Verify CSV files exist for target tables

2. **Configure Selective Migration**
   - Start new migration
   - Select "Selective Migration"
   - Choose specific tables:
     ```
     Include Tables:
     ✓ Test
     ✓ Sample
     ✓ Equipment
     
     Exclude Tables:
     ✓ AuditLog
     ✓ UserSettings
     ✓ Configuration
     ```

3. **Execute Migration**
   - Review configuration
   - Start migration process
   - Monitor table-specific progress

4. **Validate Results**
   - Check only selected tables were affected
   - Verify data integrity for updated tables
   - Test dependent functionality

### Scenario 3: SQL Validation Only

When you want to validate that your current API queries return the same results as the legacy system.

#### Step-by-Step Process

1. **Prepare Validation**
   - Ensure legacy database is accessible
   - Verify current database has data
   - Check query definitions are available

2. **Start Validation**
   - Navigate to SQL Validation section
   - Click "Start Validation"
   - Configure validation options:
     - ✅ Include performance comparison
     - ✅ Generate detailed reports
     - Set performance threshold (e.g., 2x slower)

3. **Monitor Validation**
   - Watch query-by-query progress
   - Review real-time comparison results
   - Note any discrepancies or performance issues

4. **Review Validation Report**
   - Check overall validation summary
   - Review failed validations
   - Analyze performance differences
   - Export detailed report for development team

### Scenario 4: SSO Preparation

When you need to remove JWT authentication in preparation for Active Directory integration.

#### Step-by-Step Process

1. **Create Authentication Backup**
   - Navigate to SSO Migration section
   - Click "Create Backup"
   - Verify backup was created successfully
   - Note backup ID for potential rollback

2. **Remove JWT Authentication**
   - Click "Remove JWT Authentication"
   - Confirm the operation
   - Wait for completion
   - Verify authentication endpoints are disabled

3. **Update Frontend**
   - Click "Update Frontend"
   - Confirm frontend authentication removal
   - Test that application loads without authentication

4. **Cleanup Configuration**
   - Click "Cleanup Configuration"
   - Remove JWT-related configuration
   - Verify configuration files are updated

5. **Test and Verify**
   - Test application functionality
   - Verify no authentication prompts appear
   - Confirm all features work without authentication
   - Document any issues for AD integration team

## Using the Migration Dashboard

### Dashboard Overview

The Migration Dashboard provides a centralized interface for all migration operations:

```
┌─────────────────────────────────────────────────────────┐
│ Migration System Dashboard                              │
├─────────────────────────────────────────────────────────┤
│ System Status: ● Healthy    Last Migration: 2 days ago │
├─────────────────────────────────────────────────────────┤
│ Quick Actions:                                          │
│ [Start Migration] [View History] [System Health]       │
├─────────────────────────────────────────────────────────┤
│ Active Migrations:                                      │
│ ● Migration #123 - In Progress (45% complete)          │
│   Started: 2023-11-13 14:30                           │
│   ETA: 8 minutes remaining                             │
├─────────────────────────────────────────────────────────┤
│ Recent Activity:                                        │
│ ✓ Migration #122 - Completed (2 hours ago)            │
│ ✓ Migration #121 - Completed (1 day ago)              │
│ ✗ Migration #120 - Failed (2 days ago)                │
└─────────────────────────────────────────────────────────┘
```

### Navigation Menu

- **Dashboard**: Overview and quick actions
- **Start Migration**: Begin new migration process
- **Migration History**: View past migrations
- **SQL Validation**: Query validation tools
- **SSO Migration**: Authentication management
- **System Health**: Monitor system status
- **Configuration**: System settings
- **Help**: Documentation and support

### Migration Configuration

When starting a new migration, you'll see the configuration screen:

```
Migration Configuration
┌─────────────────────────────────────────────────────────┐
│ Migration Type:                                         │
│ ○ Complete Database Migration                           │
│ ○ Selective Table Migration                             │
│ ○ SQL Validation Only                                   │
│ ○ SSO Preparation                                       │
├─────────────────────────────────────────────────────────┤
│ Options:                                                │
│ ☑ Clear existing data before seeding                   │
│ ☑ Create missing tables automatically                  │
│ ☑ Validate data integrity after seeding                │
│ ☐ Remove JWT authentication                             │
├─────────────────────────────────────────────────────────┤
│ Advanced Settings:                                      │
│ Batch Size: [1000    ] records per batch               │
│ Timeout: [300     ] seconds per operation               │
│ ☑ Continue on non-critical errors                      │
│ ☑ Generate detailed reports                            │
├─────────────────────────────────────────────────────────┤
│ Table Selection: (for selective migration)             │
│ Available Tables    │ Selected Tables                   │
│ ☐ AuditLog         │ ☑ Test                           │
│ ☐ Configuration    │ ☑ Sample                         │
│ ☐ Equipment        │ ☑ Equipment                      │
│ ☐ Sample           │                                   │
│ ☐ Test             │                                   │
├─────────────────────────────────────────────────────────┤
│           [Cancel]              [Start Migration]       │
└─────────────────────────────────────────────────────────┘
```

## Monitoring Migration Progress

### Real-Time Progress Tracking

During migration, you'll see detailed progress information:

```
Migration Progress - ID: mig_20231113_143022
┌─────────────────────────────────────────────────────────┐
│ Overall Progress: ████████████░░░░░░░░ 65% Complete     │
│ Current Step: Database Seeding                          │
│ Elapsed Time: 00:08:32    ETA: 00:04:15                │
├─────────────────────────────────────────────────────────┤
│ Step Progress:                                          │
│ ✓ 1. Backup Creation (00:00:15)                        │
│ ✓ 2. Table Creation (00:01:30)                         │
│ ● 3. Database Seeding (00:06:47) - In Progress         │
│   └─ Processing: Equipment table (1,250/2,000 records) │
│ ○ 4. Data Validation - Pending                         │
│ ○ 5. SQL Validation - Pending                          │
│ ○ 6. Report Generation - Pending                       │
├─────────────────────────────────────────────────────────┤
│ Statistics:                                             │
│ Tables Processed: 12/18                                 │
│ Records Inserted: 45,230                               │
│ Records Skipped: 23                                     │
│ Errors: 2 (non-critical)                               │
├─────────────────────────────────────────────────────────┤
│ Recent Activity:                                        │
│ 14:38:45 - Equipment table seeding started             │
│ 14:38:32 - Sample table completed (2,000 records)      │
│ 14:37:15 - Test table completed (15,000 records)       │
├─────────────────────────────────────────────────────────┤
│                    [Cancel Migration]                   │
└─────────────────────────────────────────────────────────┘
```

### Progress Indicators

- **Green ✓**: Completed successfully
- **Blue ●**: Currently in progress
- **Gray ○**: Pending/not started
- **Red ✗**: Failed or error occurred
- **Yellow ⚠**: Warning or non-critical issue

### Error Handling

When errors occur, you'll see detailed information:

```
Error Details
┌─────────────────────────────────────────────────────────┐
│ Error Type: Data Validation Error                       │
│ Table: Equipment                                        │
│ Record: Row 1,247                                       │
│ Message: Invalid equipment type 'XYZ123'               │
├─────────────────────────────────────────────────────────┤
│ Actions:                                                │
│ ○ Skip this record and continue                         │
│ ○ Stop migration and fix data                           │
│ ○ Use default value and continue                        │
├─────────────────────────────────────────────────────────┤
│                [Skip]  [Stop]  [Use Default]           │
└─────────────────────────────────────────────────────────┘
```

## Migration Reports

### Summary Report

After migration completion, you'll receive a comprehensive summary:

```
Migration Summary Report
Migration ID: mig_20231113_143022
Status: Completed Successfully
Duration: 00:12:45 (Started: 2023-11-13 14:30:22)

Database Seeding Results:
├─ Tables Processed: 18/18
├─ Tables Created: 3
├─ Records Inserted: 67,450
├─ Records Skipped: 45
└─ Errors: 2 (non-critical)

SQL Validation Results:
├─ Queries Validated: 25/25
├─ Queries Matched: 23
├─ Queries Failed: 2
├─ Average Performance Difference: +15%
└─ Critical Issues: 0

SSO Migration Results:
├─ Authentication Backup: Created
├─ JWT Removal: Completed
├─ Frontend Update: Completed
└─ Configuration Cleanup: Completed

Recommendations:
• Review failed SQL validations for GetHistoricalData and GetAuditLog
• Consider optimizing Equipment table seeding (slower than expected)
• Test application functionality after authentication removal
```

### Detailed Reports

Access detailed reports for in-depth analysis:

- **Data Seeding Report**: Table-by-table results with error details
- **SQL Validation Report**: Query-by-query comparison results
- **Performance Report**: Timing and resource usage analysis
- **Error Report**: Complete list of errors and warnings

## Best Practices

### Before Migration

1. **Plan Your Migration**
   - Identify which tables need updating
   - Determine acceptable downtime window
   - Prepare rollback plan if needed

2. **Verify Prerequisites**
   - Test database connections
   - Validate CSV file formats
   - Check available disk space
   - Ensure backup systems are working

3. **Communicate with Stakeholders**
   - Notify users of planned downtime
   - Coordinate with development team
   - Prepare support team for potential issues

### During Migration

1. **Monitor Actively**
   - Watch progress indicators
   - Review error messages promptly
   - Be prepared to intervene if needed

2. **Document Issues**
   - Note any unexpected errors
   - Record performance observations
   - Save error messages for analysis

3. **Stay Available**
   - Remain available during migration
   - Have contact information for support team
   - Keep rollback procedures accessible

### After Migration

1. **Validate Results**
   - Test critical application functions
   - Verify data integrity
   - Check performance benchmarks

2. **Review Reports**
   - Analyze migration summary
   - Address any failed validations
   - Document lessons learned

3. **Update Documentation**
   - Record successful procedures
   - Update troubleshooting guides
   - Share insights with team

## Common Migration Patterns

### Weekly Data Refresh

For regular data updates:

1. **Schedule**: Run every Sunday at 2 AM
2. **Configuration**:
   - Clear existing data: ✅
   - Create missing tables: ❌ (tables should exist)
   - Validate against legacy: ✅
   - Continue on error: ✅

3. **Monitoring**: Set up automated alerts for failures

### Development Environment Setup

For setting up new development environments:

1. **Configuration**:
   - Clear existing data: ✅
   - Create missing tables: ✅
   - Validate against legacy: ❌ (may not have access)
   - Use sample data: ✅

2. **Post-Migration**: Configure development-specific settings

### Production Deployment

For production system updates:

1. **Pre-Migration**:
   - Create full system backup
   - Schedule maintenance window
   - Prepare rollback procedures

2. **Configuration**:
   - Clear existing data: ❌ (preserve existing data)
   - Create missing tables: ✅
   - Validate against legacy: ✅
   - Continue on error: ❌ (stop on any error)

3. **Post-Migration**:
   - Comprehensive testing
   - Performance validation
   - User acceptance testing

## Troubleshooting Common Issues

### Migration Won't Start

**Symptoms**: Migration fails to start or immediately fails

**Possible Causes**:
- Database connection issues
- File permission problems
- Configuration errors
- Insufficient disk space

**Solutions**:
1. Check database connection strings
2. Verify file system permissions
3. Validate configuration settings
4. Check available disk space
5. Review system logs for detailed errors

### Migration Runs Slowly

**Symptoms**: Migration takes much longer than expected

**Possible Causes**:
- Large dataset size
- Database performance issues
- Network connectivity problems
- Resource constraints

**Solutions**:
1. Reduce batch size in configuration
2. Check database performance metrics
3. Verify network connectivity
4. Monitor system resources (CPU, memory, disk)
5. Consider running during off-peak hours

### Data Validation Errors

**Symptoms**: Many records are skipped due to validation errors

**Possible Causes**:
- Data format changes
- Missing reference data
- Corrupted CSV files
- Outdated validation rules

**Solutions**:
1. Review CSV file formats
2. Check for missing lookup data
3. Validate CSV file integrity
4. Update validation rules if needed
5. Clean source data before migration

### SQL Validation Failures

**Symptoms**: Current queries don't match legacy results

**Possible Causes**:
- Business logic changes
- Database schema differences
- Data transformation issues
- Query optimization differences

**Solutions**:
1. Compare database schemas
2. Review business logic changes
3. Check data transformation rules
4. Analyze query execution plans
5. Consult with development team

## Getting Help

### Self-Service Resources

1. **Documentation**: Check this user guide and API documentation
2. **System Health**: Review system health dashboard
3. **Migration History**: Look at previous successful migrations
4. **Error Logs**: Check detailed error logs for specific issues

### Support Channels

1. **Help Desk**: Submit ticket through internal help desk system
2. **Email Support**: migration-support@your-company.com
3. **Emergency Contact**: For critical production issues
4. **Developer Team**: For technical questions about queries or data

### Information to Provide

When requesting support, include:
- Migration ID
- Error messages (exact text)
- Configuration used
- System environment (dev/staging/production)
- Steps taken before the issue occurred
- Screenshots of error screens

## Appendix

### Configuration Reference

#### Migration Options
- `clearExistingData`: Remove all data before seeding
- `createMissingTables`: Create tables from SQL scripts
- `validateAgainstLegacy`: Compare with legacy system
- `removeAuthentication`: Remove JWT authentication
- `batchSize`: Records processed per batch
- `continueOnError`: Continue processing after non-critical errors

#### File Locations
- CSV Data Files: `/db-seeding/`
- SQL Schema Files: `/db-tables/`
- Backup Files: `/Backups/`
- Log Files: `/Logs/`

#### Database Connections
- Current Database: Primary application database
- Legacy Database: VB.NET application database
- Connection Timeout: Default 30 seconds
- Command Timeout: Default 300 seconds

### Glossary

- **Migration**: Process of updating database with new data
- **Seeding**: Inserting data from CSV files into database tables
- **Validation**: Comparing query results between systems
- **SSO**: Single Sign-On authentication system
- **JWT**: JSON Web Token authentication
- **Batch Size**: Number of records processed at once
- **Rollback**: Reverting changes to previous state