# SSO Migration Guide

This document describes the authentication changes made during the SSO migration process and provides guidance for rollback if needed.

## Overview

The SSO migration removes the current JWT-based authentication system to prepare for Active Directory Single Sign-On integration. This process includes:

1. **API Changes**: Removal of JWT middleware, authentication services, and endpoints
2. **Frontend Changes**: Removal of authentication guards, interceptors, and token management
3. **Configuration Changes**: Removal of JWT configuration from appsettings files
4. **Backup System**: Complete backup and rollback capabilities

## Changes Made

### API Changes

#### Removed Components
- JWT Authentication middleware (`UseAuthentication()`, `UseAuthorization()`)
- JWT Bearer configuration in `Program.cs`
- Authentication service registrations (`IAuthenticationService`, `IAuthorizationService`)
- Authentication endpoint mappings (`MapAuthenticationEndpoints()`)
- JWT configuration sections from `appsettings.json` and `appsettings.Development.json`

#### Removed Using Statements
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.IdentityModel.Tokens`
- `System.Text` (JWT-related usage)

#### Files Modified
- `LabResultsApi/Program.cs`
- `LabResultsApi/appsettings.json`
- `LabResultsApi/appsettings.Development.json`

### Frontend Changes

#### Removed Components
- Authentication guards (`auth.guard.ts` → `auth.guard.ts.bak`)
- Authentication interceptors (`auth.interceptor.ts` → `auth.interceptor.ts.bak`)
- Guard references from routing configuration
- Interceptor registrations from app configuration

#### Modified Files
- `lab-results-frontend/src/app/app.routes.ts`
- `lab-results-frontend/src/app/app.config.ts`
- `lab-results-frontend/src/app/shared/services/auth.service.ts`

#### Added Methods
- `clearAuthenticationTokens()` method in AuthService for complete token cleanup

## Backup System

### Automatic Backups
The migration process automatically creates backups before making changes:

- **Backup Location**: `LabResultsApi/Backups/Authentication/`
- **Backup Format**: `auth_backup_YYYYMMDD_HHMMSS`
- **Backup Contents**:
  - `Program.cs`
  - `appsettings.json`
  - `appsettings.Development.json`
  - Frontend authentication files (if available)
  - Backup metadata (`backup_metadata.json`)

### Backup Metadata
Each backup includes metadata with:
```json
{
  "BackupId": "auth_backup_20231113_143022",
  "CreatedAt": "2023-11-13T14:30:22.123Z",
  "Description": "Authentication configuration backup before SSO migration",
  "BackedUpFiles": ["Program.cs", "appsettings.json", ...],
  "TotalSizeBytes": 12345
}
```

## API Endpoints

### SSO Migration Controller (`/api/SsoMigration`)

#### Create Backup
```http
POST /api/SsoMigration/backup
```
Creates a backup of current authentication configuration.

#### Remove JWT Authentication
```http
POST /api/SsoMigration/remove-jwt-auth
```
Removes JWT authentication system from API.

#### Update Frontend
```http
POST /api/SsoMigration/update-frontend
```
Updates frontend to remove authentication guards and interceptors.

#### Cleanup Configuration
```http
POST /api/SsoMigration/cleanup-config
```
Cleans up authentication configuration files.

#### Complete Migration
```http
POST /api/SsoMigration/migrate-to-sso
```
Performs complete SSO migration (backup + remove auth + cleanup).

#### Rollback
```http
POST /api/SsoMigration/rollback/{backupId}
```
Rolls back authentication changes using specified backup.

#### List Backups
```http
GET /api/SsoMigration/backups
```
Returns list of available authentication backups.

## Rollback Process

### Automatic Rollback
Use the API endpoint to rollback to a previous backup:

```bash
curl -X POST "https://localhost:7001/api/SsoMigration/rollback/auth_backup_20231113_143022"
```

### Manual Rollback
If automatic rollback fails, manually restore files from backup:

1. **Locate Backup**: Navigate to `LabResultsApi/Backups/Authentication/{backupId}/`
2. **Restore API Files**:
   - Copy `Program.cs` back to `LabResultsApi/Program.cs`
   - Copy `appsettings.json` back to `LabResultsApi/appsettings.json`
   - Copy `appsettings.Development.json` back to `LabResultsApi/appsettings.Development.json`
3. **Restore Frontend Files**:
   - Rename `auth.guard.ts.bak` back to `auth.guard.ts`
   - Rename `auth.interceptor.ts.bak` back to `auth.interceptor.ts`
   - Restore original `app.routes.ts` and `app.config.ts` from backup
4. **Restart Application**: Restart both API and frontend applications

## Verification Steps

### After Migration
1. **API Verification**:
   - Confirm authentication endpoints return 404
   - Verify no JWT middleware in request pipeline
   - Check that protected endpoints are accessible without tokens

2. **Frontend Verification**:
   - Confirm routing works without authentication guards
   - Verify no authentication interceptors are active
   - Check that authentication tokens are cleared

### After Rollback
1. **API Verification**:
   - Confirm authentication endpoints are accessible
   - Verify JWT middleware is active
   - Test login functionality

2. **Frontend Verification**:
   - Confirm authentication guards are protecting routes
   - Verify authentication interceptors are adding tokens
   - Test complete authentication flow

## Troubleshooting

### Common Issues

#### Migration Fails
- **Check Permissions**: Ensure write permissions to backup directory
- **Check File Locks**: Ensure no processes are locking configuration files
- **Check Disk Space**: Ensure sufficient space for backups

#### Rollback Fails
- **Verify Backup Exists**: Check backup directory and metadata
- **Check File Permissions**: Ensure write permissions to restore files
- **Manual Restore**: Use manual rollback process if automatic fails

#### Application Won't Start
- **Check Configuration**: Verify configuration files are valid JSON
- **Check Dependencies**: Ensure all required NuGet packages are available
- **Check Logs**: Review application logs for specific errors

### Support
For issues with the SSO migration process:
1. Check application logs for detailed error messages
2. Verify backup integrity using the `/api/SsoMigration/backups` endpoint
3. Use manual rollback process if automatic rollback fails
4. Contact system administrator if issues persist

## Next Steps

After successful SSO migration:
1. **Configure Active Directory**: Set up AD authentication
2. **Update Authorization**: Implement role-based authorization with AD groups
3. **Test Integration**: Thoroughly test AD authentication flow
4. **Update Documentation**: Document new authentication process
5. **Clean Up**: Remove old authentication-related code and dependencies

## Security Considerations

- **Backup Security**: Backups may contain sensitive configuration data
- **Access Control**: Restrict access to migration endpoints in production
- **Audit Trail**: All migration operations are logged for audit purposes
- **Testing**: Thoroughly test SSO integration before removing backups