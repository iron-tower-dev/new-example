# Migration System API Reference

## Overview

The Migration System API provides comprehensive endpoints for database migration, SQL validation, and SSO preparation. All endpoints follow RESTful conventions and return consistent response formats.

## Base URL

- **Development**: `https://localhost:7001/api`
- **Staging**: `https://staging-api.labresults.com/api`
- **Production**: `https://api.labresults.com/api`

## Authentication

- **Development**: No authentication required
- **Staging/Production**: Bearer token authentication required

## Response Format

All API responses follow a consistent format:

```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully",
  "errors": [],
  "timestamp": "2023-11-13T14:30:22.123Z",
  "correlationId": "abc123-def456-ghi789"
}
```

### Error Response Format

```json
{
  "success": false,
  "data": null,
  "message": "Operation failed",
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Invalid input data",
      "field": "tableName",
      "details": "Table name cannot be empty"
    }
  ],
  "timestamp": "2023-11-13T14:30:22.123Z",
  "correlationId": "abc123-def456-ghi789"
}
```

## Migration Control Endpoints

### Start Full Migration

Initiates a complete migration process including database seeding, SQL validation, and SSO preparation.

```http
POST /api/Migration/start
Content-Type: application/json

{
  "clearExistingData": true,
  "createMissingTables": true,
  "validateAgainstLegacy": true,
  "removeAuthentication": false,
  "includeTables": ["Test", "Sample", "Equipment"],
  "excludeTables": ["TempData", "LogEntries"],
  "batchSize": 1000,
  "continueOnError": true
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "migrationId": "mig_20231113_143022",
    "status": "InProgress",
    "startTime": "2023-11-13T14:30:22.123Z",
    "estimatedDuration": "00:15:00"
  },
  "message": "Migration started successfully"
}
```

### Get Migration Status

Retrieves the current status of a migration operation.

```http
GET /api/Migration/status/{migrationId}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "migrationId": "mig_20231113_143022",
    "status": "InProgress",
    "startTime": "2023-11-13T14:30:22.123Z",
    "currentStep": "DatabaseSeeding",
    "progress": {
      "overallPercent": 45,
      "currentOperation": "Seeding Test table",
      "tablesProcessed": 12,
      "totalTables": 27,
      "recordsProcessed": 15000,
      "estimatedTimeRemaining": "00:08:30"
    },
    "statistics": {
      "tablesCreated": 3,
      "recordsInserted": 15000,
      "recordsSkipped": 25,
      "validationsPassed": 8,
      "validationsFailed": 1
    }
  }
}
```

### Cancel Migration

Cancels a running migration operation.

```http
POST /api/Migration/cancel/{migrationId}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "migrationId": "mig_20231113_143022",
    "status": "Cancelled",
    "cancelledAt": "2023-11-13T14:35:22.123Z",
    "reason": "User requested cancellation"
  },
  "message": "Migration cancelled successfully"
}
```

### Get Migration Report

Generates and retrieves a detailed migration report.

```http
GET /api/Migration/report/{migrationId}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "migrationId": "mig_20231113_143022",
    "status": "Completed",
    "duration": "00:12:45",
    "summary": {
      "tablesProcessed": 27,
      "tablesCreated": 3,
      "recordsInserted": 45000,
      "recordsSkipped": 125,
      "validationsPassed": 25,
      "validationsFailed": 2
    },
    "seedingResults": { ... },
    "validationResults": { ... },
    "ssoResults": { ... },
    "errors": [ ... ]
  }
}
```

### List Migrations

Retrieves a list of all migration operations.

```http
GET /api/Migration/list?page=1&pageSize=10&status=Completed
```

**Response:**
```json
{
  "success": true,
  "data": {
    "migrations": [
      {
        "migrationId": "mig_20231113_143022",
        "status": "Completed",
        "startTime": "2023-11-13T14:30:22.123Z",
        "endTime": "2023-11-13T14:43:07.456Z",
        "duration": "00:12:45",
        "tablesProcessed": 27,
        "recordsInserted": 45000
      }
    ],
    "totalCount": 15,
    "page": 1,
    "pageSize": 10
  }
}
```

## Database Seeding Endpoints

### Start Database Seeding

Initiates database seeding process for all or specific tables.

```http
POST /api/DatabaseSeeding/start
Content-Type: application/json

{
  "clearExistingData": true,
  "createMissingTables": true,
  "includeTables": ["Test", "Sample"],
  "excludeTables": ["TempData"],
  "batchSize": 1000,
  "continueOnError": true,
  "validateBeforeInsert": true
}
```

### Seed Specific Table

Seeds a specific table with data from its corresponding CSV file.

```http
POST /api/DatabaseSeeding/table/{tableName}
Content-Type: application/json

{
  "clearExistingData": true,
  "createTableIfMissing": true,
  "batchSize": 1000,
  "validateBeforeInsert": true
}
```

### Get Seeding Status

Retrieves the status of a database seeding operation.

```http
GET /api/DatabaseSeeding/status/{seedingId}
```

### Create Missing Tables

Creates all missing tables using SQL scripts from the schema directory.

```http
POST /api/DatabaseSeeding/create-tables
```

### Validate Data Integrity

Validates the integrity of seeded data.

```http
POST /api/DatabaseSeeding/validate-integrity
```

## SQL Validation Endpoints

### Start SQL Validation

Initiates SQL validation process to compare current API queries with legacy system.

```http
POST /api/SqlValidation/start
Content-Type: application/json

{
  "includeQueries": ["GetTestResults", "GetSampleData"],
  "excludeQueries": ["GetTempData"],
  "validatePerformance": true,
  "performanceThreshold": 2.0,
  "maxDiscrepancies": 100
}
```

### Validate Specific Query

Validates a specific query against the legacy system.

```http
POST /api/SqlValidation/query/{queryName}
Content-Type: application/json

{
  "currentQuery": "SELECT * FROM Test WHERE SampleId = @sampleId",
  "legacyQuery": "SELECT * FROM Test WHERE SampleId = ?",
  "parameters": {
    "sampleId": 12345
  },
  "validatePerformance": true
}
```

### Get Validation Status

Retrieves the status of a SQL validation operation.

```http
GET /api/SqlValidation/status/{validationId}
```

### Get Validation Report

Generates and retrieves a detailed validation report.

```http
GET /api/SqlValidation/report/{validationId}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "validationId": "val_20231113_143022",
    "status": "Completed",
    "summary": {
      "queriesValidated": 25,
      "queriesMatched": 23,
      "queriesFailed": 2,
      "averagePerformanceDifference": 0.15,
      "totalDiscrepancies": 12
    },
    "results": [
      {
        "queryName": "GetTestResults",
        "dataMatches": true,
        "currentRowCount": 1500,
        "legacyRowCount": 1500,
        "currentExecutionTime": "00:00:00.250",
        "legacyExecutionTime": "00:00:00.180",
        "discrepancies": []
      }
    ]
  }
}
```

## SSO Migration Endpoints

### Create Authentication Backup

Creates a backup of the current authentication configuration.

```http
POST /api/SsoMigration/backup
```

### Remove JWT Authentication

Removes JWT authentication system from the API.

```http
POST /api/SsoMigration/remove-jwt-auth
```

### Update Frontend Authentication

Updates frontend to remove authentication guards and interceptors.

```http
POST /api/SsoMigration/update-frontend
```

### Cleanup Configuration

Cleans up authentication configuration files.

```http
POST /api/SsoMigration/cleanup-config
```

### Complete SSO Migration

Performs complete SSO migration (backup + remove auth + cleanup).

```http
POST /api/SsoMigration/migrate-to-sso
```

### Rollback Authentication

Rolls back authentication changes using a specified backup.

```http
POST /api/SsoMigration/rollback/{backupId}
```

### List Authentication Backups

Returns a list of available authentication backups.

```http
GET /api/SsoMigration/backups
```

## Monitoring and Logging Endpoints

### Get System Health

Retrieves the health status of the migration system.

```http
GET /api/Migration/health
```

**Response:**
```json
{
  "success": true,
  "data": {
    "status": "Healthy",
    "checks": {
      "database": "Healthy",
      "fileSystem": "Healthy",
      "legacyDatabase": "Warning",
      "diskSpace": "Healthy"
    },
    "uptime": "2.15:30:45",
    "version": "1.0.0"
  }
}
```

### Get Performance Metrics

Retrieves performance metrics for migration operations.

```http
GET /api/Migration/metrics?from=2023-11-13T00:00:00Z&to=2023-11-13T23:59:59Z
```

### Download Logs

Downloads log files for a specific migration or time period.

```http
GET /api/Migration/logs/download?migrationId=mig_20231113_143022&format=json
```

## Configuration Endpoints

### Get Migration Configuration

Retrieves the current migration configuration.

```http
GET /api/Migration/config
```

### Update Migration Configuration

Updates migration configuration settings.

```http
PUT /api/Migration/config
Content-Type: application/json

{
  "defaultBatchSize": 1000,
  "maxConcurrentTables": 5,
  "commandTimeout": 300,
  "enableValidation": true,
  "continueOnError": true
}
```

### Validate Configuration

Validates migration configuration settings.

```http
POST /api/Migration/config/validate
Content-Type: application/json

{
  "connectionStrings": {
    "current": "Server=localhost;Database=LabResults;...",
    "legacy": "Server=legacy;Database=OldLabResults;..."
  },
  "fileSettings": {
    "csvDirectory": "/db-seeding/",
    "sqlDirectory": "/db-tables/"
  }
}
```

## File Management Endpoints

### List CSV Files

Lists available CSV files for database seeding.

```http
GET /api/Migration/files/csv
```

### List SQL Schema Files

Lists available SQL schema files for table creation.

```http
GET /api/Migration/files/sql
```

### Validate CSV File

Validates the structure and content of a CSV file.

```http
POST /api/Migration/files/csv/{fileName}/validate
```

### Get File Information

Retrieves information about a specific file.

```http
GET /api/Migration/files/{fileName}/info
```

## WebSocket Endpoints

### Real-time Migration Updates

Connect to receive real-time updates during migration operations.

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/migrationHub")
    .build();

connection.on("MigrationProgress", (data) => {
    console.log("Progress:", data);
});

connection.on("MigrationCompleted", (data) => {
    console.log("Completed:", data);
});

connection.on("MigrationError", (error) => {
    console.error("Error:", error);
});
```

## Error Codes

### Common Error Codes

| Code | Description | HTTP Status |
|------|-------------|-------------|
| `MIGRATION_NOT_FOUND` | Migration ID not found | 404 |
| `MIGRATION_IN_PROGRESS` | Migration already running | 409 |
| `INVALID_CONFIGURATION` | Invalid configuration settings | 400 |
| `DATABASE_CONNECTION_FAILED` | Cannot connect to database | 500 |
| `FILE_NOT_FOUND` | Required file not found | 404 |
| `INSUFFICIENT_PERMISSIONS` | Insufficient file/database permissions | 403 |
| `VALIDATION_FAILED` | Data validation failed | 422 |
| `LEGACY_SYSTEM_UNAVAILABLE` | Cannot connect to legacy system | 503 |

### Database Seeding Error Codes

| Code | Description |
|------|-------------|
| `CSV_PARSE_ERROR` | Error parsing CSV file |
| `TABLE_CREATION_FAILED` | Failed to create table |
| `DATA_VALIDATION_FAILED` | Data validation failed |
| `BULK_INSERT_FAILED` | Bulk insert operation failed |
| `FOREIGN_KEY_VIOLATION` | Foreign key constraint violation |

### SQL Validation Error Codes

| Code | Description |
|------|-------------|
| `QUERY_EXECUTION_FAILED` | Query execution failed |
| `RESULT_COMPARISON_FAILED` | Failed to compare results |
| `PERFORMANCE_THRESHOLD_EXCEEDED` | Query performance below threshold |
| `LEGACY_QUERY_FAILED` | Legacy query execution failed |

### SSO Migration Error Codes

| Code | Description |
|------|-------------|
| `BACKUP_CREATION_FAILED` | Failed to create backup |
| `AUTH_REMOVAL_FAILED` | Failed to remove authentication |
| `CONFIG_CLEANUP_FAILED` | Failed to cleanup configuration |
| `ROLLBACK_FAILED` | Failed to rollback changes |

## Rate Limiting

API endpoints are rate-limited to prevent abuse:

- **Migration Operations**: 5 requests per minute
- **Status Queries**: 60 requests per minute
- **Configuration Updates**: 10 requests per minute
- **File Operations**: 30 requests per minute

## Pagination

List endpoints support pagination using query parameters:

- `page`: Page number (1-based, default: 1)
- `pageSize`: Items per page (default: 10, max: 100)
- `sortBy`: Sort field (default: timestamp)
- `sortOrder`: Sort order (asc/desc, default: desc)

## Filtering

List endpoints support filtering using query parameters:

- `status`: Filter by status (InProgress, Completed, Failed, Cancelled)
- `from`: Start date filter (ISO 8601 format)
- `to`: End date filter (ISO 8601 format)
- `search`: Text search in migration names/descriptions

## SDK and Client Libraries

### .NET Client

```csharp
var client = new MigrationApiClient("https://api.labresults.com");
var result = await client.StartMigrationAsync(new MigrationOptions
{
    ClearExistingData = true,
    CreateMissingTables = true
});
```

### JavaScript Client

```javascript
const client = new MigrationApiClient('https://api.labresults.com');
const result = await client.startMigration({
    clearExistingData: true,
    createMissingTables: true
});
```

## Testing

### Test Endpoints

Development and staging environments include additional endpoints for testing:

```http
POST /api/Migration/test/seed-sample-data
POST /api/Migration/test/clear-all-data
POST /api/Migration/test/reset-configuration
```

### Mock Responses

Use the `X-Mock-Response` header to receive mock responses for testing:

```http
GET /api/Migration/status/test-migration-id
X-Mock-Response: success
```

## Versioning

The API uses semantic versioning with the version specified in the URL:

- `v1`: Current stable version
- `v2`: Next major version (when available)

Example: `https://api.labresults.com/api/v1/Migration/start`

## Support

For API support and questions:
- **Documentation**: [API Documentation Portal]
- **Support Email**: api-support@labresults.com
- **Developer Forum**: [Developer Community]
- **Status Page**: [API Status Page]