# Lab Results System Deployment Files

This directory contains all the necessary files and scripts for deploying the Lab Results System to production and staging environments.

## Files Overview

### Configuration Files
- `appsettings.Production.json` - Production API configuration
- `appsettings.Staging.json` - Staging API configuration
- `web.config` - IIS configuration for API
- `deployment-config.json` - Environment-specific deployment settings

### Deployment Scripts
- `deploy-full.bat` - Complete automated deployment (Windows batch)
- `deploy-api.ps1` - PowerShell script for API deployment
- `deploy-frontend.ps1` - PowerShell script for Frontend deployment
- `validate-deployment.ps1` - Comprehensive deployment validation

### Database Scripts
- `configure-database.sql` - Database setup and configuration

### Documentation
- `DEPLOYMENT_GUIDE.md` - Complete deployment guide
- `README.md` - This file

## Quick Start

### Prerequisites
1. Windows Server with IIS installed
2. .NET 8.0 Hosting Bundle
3. SQL Server
4. PowerShell 5.1+
5. Administrator privileges

### Automated Deployment
```batch
# Run as Administrator
cd deployment
deploy-full.bat production
```

### Manual Steps Required
Before running the deployment scripts, you must:

1. **Configure Database**:
   - Run `configure-database.sql` on your SQL Server
   - Replace password placeholders with secure values

2. **Update Configuration**:
   - Edit `appsettings.Production.json`
   - Replace all `#{placeholder}#` values with actual configuration

3. **Prepare File System**:
   - Ensure target directories exist
   - Set appropriate permissions

## Environment Configurations

### Production
- API Port: 8080
- Frontend Port: 80
- Database: Production SQL Server
- Logging: Warning level
- Caching: Extended timeouts
- Security: Full security headers

### Staging
- API Port: 8081
- Frontend Port: 8082
- Database: Staging SQL Server
- Logging: Information level
- Caching: Moderate timeouts
- Security: Full security headers with debug info

## Security Checklist

- [ ] Strong database passwords
- [ ] Secure JWT secret keys (32+ characters)
- [ ] HTTPS enforcement
- [ ] Security headers enabled
- [ ] File upload restrictions
- [ ] Database encryption in transit
- [ ] Principle of least privilege for service accounts

## Validation

After deployment, run the validation script:
```powershell
.\validate-deployment.ps1
```

This will test:
- API health endpoints
- Database connectivity
- File system access
- IIS configuration
- Frontend accessibility

## Troubleshooting

### Common Issues
1. **API won't start**: Check .NET hosting bundle installation
2. **Database connection**: Verify connection string and SQL Server status
3. **File permissions**: Ensure IIS app pool identity has proper access
4. **Frontend routing**: Verify URL Rewrite module is installed

### Log Locations
- API Logs: `C:\inetpub\wwwroot\LabResultsApi\logs\`
- IIS Logs: `C:\inetpub\logs\LogFiles\`
- Windows Event Logs: Application and System

## Support

For deployment issues:
1. Check the validation script output
2. Review the deployment guide
3. Check application logs
4. Contact the development team

## Version History

- v1.0 - Initial deployment configuration
- v1.1 - Added staging environment support
- v1.2 - Enhanced health checks and validation