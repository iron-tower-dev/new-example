# Lab Results System Deployment Guide

## Prerequisites

### Server Requirements
- Windows Server 2019 or later
- IIS 10.0 or later with ASP.NET Core Hosting Bundle
- .NET 8.0 Runtime
- SQL Server 2019 or later
- PowerShell 5.1 or later

### Software Installation
1. Install IIS with the following features:
   - Web Server (IIS)
   - ASP.NET Core Module v2
   - URL Rewrite Module
   - Application Request Routing (ARR)

2. Install .NET 8.0 Hosting Bundle:
   - Download from Microsoft's official site
   - Restart IIS after installation

3. Install SQL Server and configure:
   - Enable TCP/IP protocol
   - Configure firewall rules
   - Set up SQL Server authentication

## Pre-Deployment Steps

### 1. Database Setup
```sql
-- Run the configure-database.sql script
sqlcmd -S [SERVER_NAME] -i configure-database.sql
```

### 2. Security Configuration
1. Replace placeholder values in `appsettings.Production.json`:
   - `#{DatabaseServer}#` - Your SQL Server instance
   - `#{DatabaseName}#` - Database name (LabResultsDb)
   - `#{DatabaseUser}#` - Database user (LabResultsUser)
   - `#{DatabasePassword}#` - Strong password for database user
   - `#{JwtSecretKey}#` - Strong JWT secret key (minimum 32 characters)
   - `#{JwtIssuer}#` - JWT issuer (e.g., LabResultsApi)
   - `#{JwtAudience}#` - JWT audience (e.g., LabResultsClient)
   - `#{FileUploadPath}#` - File upload directory path
   - `#{ProductionAngularUrl}#` - Production Angular application URL

### 3. File System Preparation
```powershell
# Create necessary directories
New-Item -ItemType Directory -Path "C:\LabResults\Uploads" -Force
New-Item -ItemType Directory -Path "C:\DatabaseBackups" -Force
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\LabResultsApi\logs" -Force

# Set permissions
icacls "C:\LabResults\Uploads" /grant "IIS AppPool\LabResultsApiPool:(OI)(CI)F"
icacls "C:\inetpub\wwwroot\LabResultsApi\logs" /grant "IIS AppPool\LabResultsApiPool:(OI)(CI)F"
```

## Deployment Process

### Option 1: Automated Deployment
```batch
# Run as Administrator
cd deployment
deploy-full.bat production
```

### Option 2: Manual Deployment

#### Step 1: Build Applications
```batch
# Build API
cd LabResultsApi
dotnet publish -c Release -o ..\deployment\api-build

# Build Frontend
cd ..\lab-results-frontend
npm run build:prod
```

#### Step 2: Deploy API
```powershell
# Run as Administrator
cd deployment
.\deploy-api.ps1 -SiteName "LabResultsApi" -SitePath "C:\inetpub\wwwroot\LabResultsApi" -AppPoolName "LabResultsApiPool" -Port "8080"

# Copy files
xcopy /E /I /Y api-build\* C:\inetpub\wwwroot\LabResultsApi\
```

#### Step 3: Deploy Frontend
```powershell
# Run as Administrator
.\deploy-frontend.ps1 -SiteName "LabResultsFrontend" -SitePath "C:\inetpub\wwwroot\LabResultsFrontend" -AppPoolName "LabResultsFrontendPool" -Port "80"
```

## Post-Deployment Verification

### 1. Health Checks
```bash
# API Health Check
curl http://localhost:8080/health

# Database Health Check
curl http://localhost:8080/health/database

# Detailed Health Check
curl http://localhost:8080/health/detailed
```

### 2. Application Testing
1. Navigate to `http://localhost` (Frontend)
2. Navigate to `http://localhost:8080/swagger` (API Documentation)
3. Test login functionality
4. Test sample selection and test entry
5. Verify file upload functionality

### 3. Performance Verification
```bash
# Memory usage
curl http://localhost:8080/health/memory

# File system access
curl http://localhost:8080/health/filesystem
```

## Configuration Management

### Environment-Specific Settings

#### Production (`appsettings.Production.json`)
- Logging: Warning level and above
- Database: Production connection string with encryption
- JWT: Strong secret keys
- CORS: Restricted to production domains
- Caching: Extended cache times
- Performance: Optimized settings

#### Staging (`appsettings.Staging.json`)
Create similar to production but with staging-specific values.

### Security Considerations

1. **Database Security**:
   - Use dedicated service account
   - Enable encryption in transit
   - Regular password rotation
   - Principle of least privilege

2. **Application Security**:
   - Strong JWT secret keys
   - HTTPS enforcement
   - Security headers enabled
   - File upload restrictions

3. **IIS Security**:
   - Remove default IIS pages
   - Configure custom error pages
   - Enable request filtering
   - Set appropriate file permissions

## Monitoring and Maintenance

### 1. Log Monitoring
- API logs: `C:\inetpub\wwwroot\LabResultsApi\logs\`
- IIS logs: `C:\inetpub\logs\LogFiles\`
- Windows Event Logs

### 2. Performance Monitoring
- Use `/health/detailed` endpoint
- Monitor IIS performance counters
- Database performance metrics
- File system usage

### 3. Backup Strategy
```sql
-- Daily backup script
BACKUP DATABASE LabResultsDb 
TO DISK = 'C:\DatabaseBackups\LabResultsDb_Daily.bak'
WITH FORMAT, INIT, COMPRESSION;
```

### 4. Update Process
1. Test updates in staging environment
2. Create database backup
3. Stop application pools
4. Deploy new version
5. Run database migrations if needed
6. Start application pools
7. Verify functionality

## Troubleshooting

### Common Issues

1. **API not starting**:
   - Check application pool identity
   - Verify .NET 8 hosting bundle installation
   - Check database connectivity
   - Review logs in `logs\stdout`

2. **Database connection issues**:
   - Verify connection string
   - Check SQL Server service status
   - Confirm firewall rules
   - Test with SQL Server Management Studio

3. **File upload issues**:
   - Check directory permissions
   - Verify disk space
   - Review file size limits
   - Check IIS request filtering

4. **Frontend routing issues**:
   - Verify URL Rewrite module installation
   - Check web.config rewrite rules
   - Confirm default document settings

### Log Locations
- API Application Logs: `C:\inetpub\wwwroot\LabResultsApi\logs\`
- IIS Logs: `C:\inetpub\logs\LogFiles\W3SVC[site-id]\`
- Windows Event Logs: Application and System logs
- SQL Server Logs: SQL Server Error Log

## Support Contacts

- System Administrator: [Contact Information]
- Database Administrator: [Contact Information]
- Development Team: [Contact Information]

## Rollback Procedure

In case of deployment issues:

1. **Stop new application pools**:
   ```powershell
   Stop-WebAppPool -Name "LabResultsApiPool"
   Stop-WebAppPool -Name "LabResultsFrontendPool"
   ```

2. **Restore previous version**:
   - Restore API files from backup
   - Restore frontend files from backup
   - Restore database if schema changes were made

3. **Start application pools**:
   ```powershell
   Start-WebAppPool -Name "LabResultsApiPool"
   Start-WebAppPool -Name "LabResultsFrontendPool"
   ```

4. **Verify rollback**:
   - Test health endpoints
   - Verify application functionality
   - Check logs for errors