# Migration System Administrator Guide

## Overview

This guide provides comprehensive information for system administrators responsible for deploying, configuring, monitoring, and maintaining the Migration System. It covers installation procedures, configuration management, security considerations, and operational best practices.

## System Requirements

### Hardware Requirements

#### Minimum Requirements
- **CPU**: 4 cores, 2.4 GHz
- **RAM**: 8 GB
- **Storage**: 100 GB available space
- **Network**: 1 Gbps connection

#### Recommended Requirements
- **CPU**: 8 cores, 3.0 GHz or higher
- **RAM**: 16 GB or more
- **Storage**: 500 GB SSD with high IOPS
- **Network**: 10 Gbps connection for large migrations

### Software Requirements

#### Operating System
- **Windows**: Windows Server 2019 or later
- **Linux**: Ubuntu 20.04 LTS, CentOS 8, or RHEL 8
- **Container**: Docker 20.10+ with Docker Compose

#### Runtime Dependencies
- **.NET Runtime**: .NET 8.0 or later
- **Database**: SQL Server 2019 or later (Express, Standard, or Enterprise)
- **Web Server**: IIS 10.0+ (Windows) or Nginx/Apache (Linux)

#### Development Dependencies (for building from source)
- **.NET SDK**: .NET 8.0 SDK
- **Node.js**: 18.x or later
- **Angular CLI**: 17.x or later

## Installation and Deployment

### Option 1: Docker Deployment (Recommended)

#### Prerequisites
```bash
# Install Docker and Docker Compose
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh
sudo usermod -aG docker $USER

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/download/v2.20.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

#### Deployment Steps
1. **Download Docker Compose Configuration**
   ```bash
   mkdir /opt/migration-system
   cd /opt/migration-system
   wget https://releases.your-company.com/migration-system/docker-compose.yml
   wget https://releases.your-company.com/migration-system/.env.example
   cp .env.example .env
   ```

2. **Configure Environment Variables**
   ```bash
   # Edit .env file
   nano .env
   ```
   
   ```env
   # Database Configuration
   DB_CONNECTION_STRING=Server=db;Database=LabResults;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=true;
   LEGACY_DB_CONNECTION_STRING=Server=legacy-db;Database=OldLabResults;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=true;
   
   # File System Configuration
   CSV_DIRECTORY=/app/data/csv
   SQL_DIRECTORY=/app/data/sql
   BACKUP_DIRECTORY=/app/data/backups
   
   # Security Configuration
   JWT_SECRET_KEY=your-super-secret-jwt-key-here
   ENCRYPTION_KEY=your-encryption-key-here
   
   # Notification Configuration
   SMTP_HOST=smtp.your-company.com
   SMTP_PORT=587
   SMTP_USERNAME=migration-system@your-company.com
   SMTP_PASSWORD=your-smtp-password
   
   # Performance Configuration
   DEFAULT_BATCH_SIZE=1000
   MAX_CONCURRENT_TABLES=5
   COMMAND_TIMEOUT=300
   ```

3. **Deploy the System**
   ```bash
   # Pull and start containers
   docker-compose pull
   docker-compose up -d
   
   # Verify deployment
   docker-compose ps
   docker-compose logs -f migration-api
   ```

4. **Initialize Database**
   ```bash
   # Run database migrations
   docker-compose exec migration-api dotnet ef database update
   
   # Seed initial data (optional)
   docker-compose exec migration-api dotnet run --seed-data
   ```

### Option 2: Traditional Deployment

#### Windows Server with IIS

1. **Install Prerequisites**
   ```powershell
   # Install .NET 8.0 Runtime
   Invoke-WebRequest -Uri "https://download.microsoft.com/download/dotnet/8.0/dotnet-hosting-8.0-win.exe" -OutFile "dotnet-hosting.exe"
   .\dotnet-hosting.exe /quiet
   
   # Install IIS with ASP.NET Core Module
   Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpErrors, IIS-HttpLogging, IIS-RequestFiltering, IIS-StaticContent, IIS-DefaultDocument, IIS-DirectoryBrowsing, IIS-ASPNET45
   ```

2. **Deploy Application**
   ```powershell
   # Create application directory
   New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\migration-system"
   
   # Extract application files
   Expand-Archive -Path "migration-system-v1.0.0.zip" -DestinationPath "C:\inetpub\wwwroot\migration-system"
   
   # Create IIS application
   Import-Module WebAdministration
   New-WebApplication -Site "Default Web Site" -Name "migration-system" -PhysicalPath "C:\inetpub\wwwroot\migration-system"
   ```

3. **Configure Application Pool**
   ```powershell
   # Create dedicated application pool
   New-WebAppPool -Name "MigrationSystemPool"
   Set-ItemProperty -Path "IIS:\AppPools\MigrationSystemPool" -Name "managedRuntimeVersion" -Value ""
   Set-ItemProperty -Path "IIS:\AppPools\MigrationSystemPool" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
   
   # Assign application pool
   Set-ItemProperty -Path "IIS:\Sites\Default Web Site\migration-system" -Name "applicationPool" -Value "MigrationSystemPool"
   ```

#### Linux with Nginx

1. **Install Prerequisites**
   ```bash
   # Install .NET 8.0 Runtime
   wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   sudo apt-get update
   sudo apt-get install -y aspnetcore-runtime-8.0
   
   # Install Nginx
   sudo apt-get install -y nginx
   ```

2. **Deploy Application**
   ```bash
   # Create application directory
   sudo mkdir -p /var/www/migration-system
   
   # Extract application files
   sudo unzip migration-system-v1.0.0.zip -d /var/www/migration-system
   sudo chown -R www-data:www-data /var/www/migration-system
   sudo chmod +x /var/www/migration-system/LabResultsApi
   ```

3. **Configure Nginx**
   ```bash
   # Create Nginx configuration
   sudo nano /etc/nginx/sites-available/migration-system
   ```
   
   ```nginx
   server {
       listen 80;
       server_name migration.your-company.com;
       
       location / {
           proxy_pass http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host $host;
           proxy_cache_bypass $http_upgrade;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
       }
   }
   ```
   
   ```bash
   # Enable site
   sudo ln -s /etc/nginx/sites-available/migration-system /etc/nginx/sites-enabled/
   sudo nginx -t
   sudo systemctl reload nginx
   ```

4. **Create Systemd Service**
   ```bash
   sudo nano /etc/systemd/system/migration-system.service
   ```
   
   ```ini
   [Unit]
   Description=Migration System API
   After=network.target
   
   [Service]
   Type=notify
   ExecStart=/usr/bin/dotnet /var/www/migration-system/LabResultsApi.dll
   Restart=always
   RestartSec=5
   KillSignal=SIGINT
   SyslogIdentifier=migration-system
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
   WorkingDirectory=/var/www/migration-system
   
   [Install]
   WantedBy=multi-user.target
   ```
   
   ```bash
   # Enable and start service
   sudo systemctl enable migration-system.service
   sudo systemctl start migration-system.service
   sudo systemctl status migration-system.service
   ```

## Configuration Management

### Application Configuration

#### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LabResults;Integrated Security=true;TrustServerCertificate=true;",
    "LegacyConnection": "Server=legacy-server;Database=OldLabResults;Integrated Security=true;TrustServerCertificate=true;"
  },
  "MigrationSettings": {
    "DefaultBatchSize": 1000,
    "MaxConcurrentTables": 5,
    "CommandTimeout": 300,
    "EnableValidation": true,
    "ContinueOnError": true,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 30
  },
  "FileSettings": {
    "CsvDirectory": "/data/csv",
    "SqlDirectory": "/data/sql",
    "BackupDirectory": "/data/backups",
    "TempDirectory": "/tmp/migration",
    "MaxFileSize": 104857600,
    "AllowedExtensions": [".csv", ".sql"]
  },
  "SecuritySettings": {
    "RequireAuthentication": true,
    "AllowedOrigins": ["https://migration.your-company.com"],
    "JwtSettings": {
      "SecretKey": "your-secret-key",
      "Issuer": "migration-system",
      "Audience": "migration-users",
      "ExpirationMinutes": 60
    }
  },
  "NotificationSettings": {
    "EnableEmailNotifications": true,
    "EnableSlackNotifications": false,
    "SmtpSettings": {
      "Host": "smtp.your-company.com",
      "Port": 587,
      "Username": "migration-system@your-company.com",
      "Password": "your-password",
      "EnableSsl": true,
      "FromAddress": "migration-system@your-company.com",
      "FromName": "Migration System"
    },
    "SlackSettings": {
      "WebhookUrl": "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK",
      "Channel": "#migration-alerts",
      "Username": "Migration Bot"
    }
  },
  "PerformanceSettings": {
    "EnablePerformanceMonitoring": true,
    "MetricsRetentionDays": 30,
    "EnableDetailedLogging": false,
    "LogSlowQueries": true,
    "SlowQueryThresholdMs": 5000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "MigrationSystem": "Debug"
    },
    "File": {
      "Path": "/var/log/migration-system/",
      "MaxFileSize": "10MB",
      "MaxFiles": 10
    }
  }
}
```

#### Environment-Specific Configuration

Create separate configuration files for each environment:

**appsettings.Development.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LabResults_Dev;Trusted_Connection=true;",
    "LegacyConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LabResults_Legacy_Dev;Trusted_Connection=true;"
  },
  "MigrationSettings": {
    "DefaultBatchSize": 100,
    "EnableValidation": false
  },
  "SecuritySettings": {
    "RequireAuthentication": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "MigrationSystem": "Trace"
    }
  }
}
```

**appsettings.Production.json**
```json
{
  "MigrationSettings": {
    "ContinueOnError": false,
    "MaxRetryAttempts": 5
  },
  "SecuritySettings": {
    "RequireAuthentication": true,
    "AllowedOrigins": ["https://migration.your-company.com"]
  },
  "PerformanceSettings": {
    "EnableDetailedLogging": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "MigrationSystem": "Information"
    }
  }
}
```

### Database Configuration

#### Connection String Security

Use secure connection strings with proper authentication:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db-server;Database=LabResults;User Id=migration_user;Password=SecurePassword123!;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;Command Timeout=300;"
  }
}
```

#### Database User Permissions

Create dedicated database users with minimal required permissions:

```sql
-- Create migration user
CREATE LOGIN migration_user WITH PASSWORD = 'SecurePassword123!';
CREATE USER migration_user FOR LOGIN migration_user;

-- Grant necessary permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO migration_user;
GRANT CREATE TABLE TO migration_user;
GRANT ALTER ON SCHEMA::dbo TO migration_user;

-- Grant specific permissions for stored procedures
GRANT EXECUTE ON SCHEMA::dbo TO migration_user;

-- For legacy database (read-only)
CREATE LOGIN migration_readonly WITH PASSWORD = 'SecurePassword123!';
CREATE USER migration_readonly FOR LOGIN migration_readonly;
GRANT SELECT ON SCHEMA::dbo TO migration_readonly;
```

### File System Configuration

#### Directory Structure
```
/opt/migration-system/
├── app/                    # Application files
├── data/
│   ├── csv/               # CSV data files
│   ├── sql/               # SQL schema files
│   ├── backups/           # System backups
│   └── temp/              # Temporary files
├── logs/                  # Application logs
├── config/                # Configuration files
└── scripts/               # Deployment scripts
```

#### Permissions Setup
```bash
# Create migration system user
sudo useradd -r -s /bin/false migration-system

# Set directory ownership and permissions
sudo chown -R migration-system:migration-system /opt/migration-system
sudo chmod 755 /opt/migration-system
sudo chmod 750 /opt/migration-system/data
sudo chmod 700 /opt/migration-system/config
sudo chmod 755 /opt/migration-system/logs

# Set file permissions
sudo find /opt/migration-system/data/csv -type f -exec chmod 644 {} \;
sudo find /opt/migration-system/data/sql -type f -exec chmod 644 {} \;
sudo chmod 600 /opt/migration-system/config/appsettings.Production.json
```

## Security Configuration

### Authentication and Authorization

#### JWT Configuration
```json
{
  "SecuritySettings": {
    "JwtSettings": {
      "SecretKey": "your-256-bit-secret-key-here-make-it-long-and-random",
      "Issuer": "migration-system",
      "Audience": "migration-users",
      "ExpirationMinutes": 60,
      "RefreshTokenExpirationDays": 7,
      "RequireHttpsMetadata": true,
      "ValidateIssuer": true,
      "ValidateAudience": true,
      "ValidateLifetime": true,
      "ValidateIssuerSigningKey": true
    }
  }
}
```

#### Role-Based Access Control
```csharp
// Configure authorization policies
services.AddAuthorization(options =>
{
    options.AddPolicy("MigrationAdmin", policy =>
        policy.RequireRole("Administrator", "MigrationAdmin"));
    
    options.AddPolicy("MigrationOperator", policy =>
        policy.RequireRole("Administrator", "MigrationAdmin", "MigrationOperator"));
    
    options.AddPolicy("MigrationViewer", policy =>
        policy.RequireRole("Administrator", "MigrationAdmin", "MigrationOperator", "MigrationViewer"));
});
```

### HTTPS Configuration

#### Certificate Setup (Linux with Let's Encrypt)
```bash
# Install Certbot
sudo apt-get install certbot python3-certbot-nginx

# Obtain certificate
sudo certbot --nginx -d migration.your-company.com

# Verify auto-renewal
sudo certbot renew --dry-run
```

#### Nginx HTTPS Configuration
```nginx
server {
    listen 443 ssl http2;
    server_name migration.your-company.com;
    
    ssl_certificate /etc/letsencrypt/live/migration.your-company.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/migration.your-company.com/privkey.pem;
    
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    
    add_header Strict-Transport-Security "max-age=63072000" always;
    add_header X-Frame-Options DENY;
    add_header X-Content-Type-Options nosniff;
    add_header X-XSS-Protection "1; mode=block";
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# Redirect HTTP to HTTPS
server {
    listen 80;
    server_name migration.your-company.com;
    return 301 https://$server_name$request_uri;
}
```

### Firewall Configuration

#### UFW (Ubuntu Firewall)
```bash
# Enable firewall
sudo ufw enable

# Allow SSH
sudo ufw allow ssh

# Allow HTTP and HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Allow database access from application server only
sudo ufw allow from 10.0.1.100 to any port 1433

# Deny all other traffic
sudo ufw default deny incoming
sudo ufw default allow outgoing
```

## Monitoring and Logging

### Application Monitoring

#### Health Checks Configuration
```csharp
services.AddHealthChecks()
    .AddDbContext<LabDbContext>()
    .AddSqlServer(connectionString, name: "database")
    .AddCheck<FileSystemHealthCheck>("filesystem")
    .AddCheck<LegacyDatabaseHealthCheck>("legacy-database")
    .AddCheck<DiskSpaceHealthCheck>("disk-space");
```

#### Metrics Collection
```json
{
  "MetricsSettings": {
    "EnableMetrics": true,
    "MetricsEndpoint": "/metrics",
    "CollectionInterval": 15,
    "RetentionDays": 30,
    "CustomMetrics": {
      "MigrationDuration": true,
      "RecordsProcessed": true,
      "ErrorRate": true,
      "DatabaseConnections": true
    }
  }
}
```

### Logging Configuration

#### Structured Logging with Serilog
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "MigrationSystem": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/migration-system/migration-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq-server:5341",
          "apiKey": "your-seq-api-key"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    "Properties": {
      "Application": "MigrationSystem",
      "Environment": "Production"
    }
  }
}
```

#### Log Rotation Setup
```bash
# Create logrotate configuration
sudo nano /etc/logrotate.d/migration-system
```

```
/var/log/migration-system/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 644 migration-system migration-system
    postrotate
        systemctl reload migration-system
    endscript
}
```

### Performance Monitoring

#### Application Performance Monitoring (APM)
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key",
    "EnableAdaptiveSampling": true,
    "EnableQuickPulseMetricStream": true,
    "EnableSqlCommandTextInstrumentation": true
  }
}
```

#### Custom Performance Counters
```csharp
// Register custom metrics
services.AddSingleton<IMetricsLogger, MetricsLogger>();

// In your service
public class MigrationService
{
    private readonly IMetricsLogger _metrics;
    private readonly Counter<int> _recordsProcessed;
    private readonly Histogram<double> _migrationDuration;
    
    public MigrationService(IMetricsLogger metrics)
    {
        _metrics = metrics;
        _recordsProcessed = _metrics.CreateCounter<int>("migration_records_processed");
        _migrationDuration = _metrics.CreateHistogram<double>("migration_duration_seconds");
    }
}
```

## Backup and Recovery

### Database Backup Strategy

#### Automated Backup Script
```sql
-- Create backup job
EXEC dbo.sp_add_job
    @job_name = N'Migration System Database Backup';

EXEC dbo.sp_add_jobstep
    @job_name = N'Migration System Database Backup',
    @step_name = N'Backup Database',
    @command = N'
        DECLARE @BackupPath NVARCHAR(500)
        SET @BackupPath = N''C:\Backups\LabResults_'' + FORMAT(GETDATE(), ''yyyyMMdd_HHmmss'') + N''.bak''
        
        BACKUP DATABASE [LabResults] 
        TO DISK = @BackupPath
        WITH COMPRESSION, CHECKSUM, INIT
        
        -- Verify backup
        RESTORE VERIFYONLY FROM DISK = @BackupPath
    ';

-- Schedule daily at 2 AM
EXEC dbo.sp_add_schedule
    @schedule_name = N'Daily 2 AM',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_time = 020000;

EXEC dbo.sp_attach_schedule
    @job_name = N'Migration System Database Backup',
    @schedule_name = N'Daily 2 AM';
```

#### Backup Retention Policy
```bash
#!/bin/bash
# backup-cleanup.sh

BACKUP_DIR="/backups/database"
RETENTION_DAYS=30

# Remove backups older than retention period
find $BACKUP_DIR -name "*.bak" -type f -mtime +$RETENTION_DAYS -delete

# Log cleanup activity
echo "$(date): Cleaned up database backups older than $RETENTION_DAYS days" >> /var/log/backup-cleanup.log
```

### Application Backup

#### Configuration Backup
```bash
#!/bin/bash
# config-backup.sh

BACKUP_DIR="/backups/config"
CONFIG_DIR="/opt/migration-system/config"
DATE=$(date +%Y%m%d_%H%M%S)

# Create backup directory
mkdir -p $BACKUP_DIR

# Backup configuration files
tar -czf $BACKUP_DIR/config_backup_$DATE.tar.gz -C $CONFIG_DIR .

# Keep only last 10 backups
ls -t $BACKUP_DIR/config_backup_*.tar.gz | tail -n +11 | xargs rm -f

echo "Configuration backup completed: config_backup_$DATE.tar.gz"
```

### Disaster Recovery Plan

#### Recovery Procedures

1. **Database Recovery**
   ```sql
   -- Restore from backup
   RESTORE DATABASE [LabResults] 
   FROM DISK = N'C:\Backups\LabResults_20231113_020000.bak'
   WITH REPLACE, CHECKDB;
   ```

2. **Application Recovery**
   ```bash
   # Stop application
   sudo systemctl stop migration-system
   
   # Restore configuration
   sudo tar -xzf /backups/config/config_backup_20231113_020000.tar.gz -C /opt/migration-system/config/
   
   # Restore application files (if needed)
   sudo tar -xzf /backups/app/app_backup_20231113_020000.tar.gz -C /opt/migration-system/app/
   
   # Start application
   sudo systemctl start migration-system
   ```

3. **Data Recovery**
   ```bash
   # Restore CSV files
   sudo cp -r /backups/data/csv/* /opt/migration-system/data/csv/
   
   # Restore SQL files
   sudo cp -r /backups/data/sql/* /opt/migration-system/data/sql/
   
   # Set proper permissions
   sudo chown -R migration-system:migration-system /opt/migration-system/data/
   ```

## Maintenance Procedures

### Regular Maintenance Tasks

#### Daily Tasks
- [ ] Check system health dashboard
- [ ] Review error logs for critical issues
- [ ] Verify backup completion
- [ ] Monitor disk space usage
- [ ] Check migration queue status

#### Weekly Tasks
- [ ] Review performance metrics
- [ ] Analyze migration success rates
- [ ] Update security patches
- [ ] Clean up temporary files
- [ ] Review user access logs

#### Monthly Tasks
- [ ] Full system backup verification
- [ ] Performance optimization review
- [ ] Security audit
- [ ] Update documentation
- [ ] Review and update monitoring thresholds

### System Updates

#### Application Updates
```bash
# Download new version
wget https://releases.your-company.com/migration-system/v1.1.0/migration-system-v1.1.0.zip

# Stop application
sudo systemctl stop migration-system

# Backup current version
sudo cp -r /opt/migration-system/app /opt/migration-system/app.backup.$(date +%Y%m%d)

# Extract new version
sudo unzip migration-system-v1.1.0.zip -d /opt/migration-system/app/

# Update permissions
sudo chown -R migration-system:migration-system /opt/migration-system/app/

# Run database migrations (if needed)
sudo -u migration-system dotnet /opt/migration-system/app/LabResultsApi.dll --migrate

# Start application
sudo systemctl start migration-system

# Verify deployment
curl -f http://localhost:5000/health || echo "Deployment failed"
```

#### Database Updates
```sql
-- Check current schema version
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId DESC;

-- Apply pending migrations
-- (This is typically done automatically during application startup)
```

### Performance Optimization

#### Database Optimization
```sql
-- Update statistics
EXEC sp_updatestats;

-- Rebuild fragmented indexes
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 'ALTER INDEX ' + i.name + ' ON ' + SCHEMA_NAME(t.schema_id) + '.' + t.name + ' REBUILD;' + CHAR(13)
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.avg_fragmentation_in_percent > 30
AND i.index_id > 0;

EXEC sp_executesql @sql;

-- Update query execution plans
EXEC sp_recompile;
```

#### Application Optimization
```bash
# Clear application cache
curl -X POST http://localhost:5000/api/admin/clear-cache

# Restart application pool (IIS)
# iisreset /restart

# Restart systemd service (Linux)
sudo systemctl restart migration-system

# Monitor memory usage
ps aux | grep dotnet
free -h
```

## Troubleshooting

### Common Issues and Solutions

#### Application Won't Start

**Symptoms:**
- Service fails to start
- HTTP 500 errors
- Connection refused errors

**Diagnostic Steps:**
```bash
# Check service status
sudo systemctl status migration-system

# Check application logs
sudo journalctl -u migration-system -f

# Check application-specific logs
tail -f /var/log/migration-system/migration-*.log

# Test configuration
sudo -u migration-system dotnet /opt/migration-system/app/LabResultsApi.dll --validate-config
```

**Common Solutions:**
1. **Configuration Issues:**
   ```bash
   # Validate JSON configuration
   python3 -m json.tool /opt/migration-system/config/appsettings.json
   
   # Check connection strings
   sqlcmd -S "server" -d "database" -U "username" -P "password" -Q "SELECT 1"
   ```

2. **Permission Issues:**
   ```bash
   # Fix file permissions
   sudo chown -R migration-system:migration-system /opt/migration-system/
   sudo chmod 755 /opt/migration-system/app/LabResultsApi
   ```

3. **Port Conflicts:**
   ```bash
   # Check port usage
   sudo netstat -tlnp | grep :5000
   
   # Kill conflicting process
   sudo kill -9 <PID>
   ```

#### Database Connection Issues

**Symptoms:**
- Connection timeout errors
- Authentication failures
- SQL Server not accessible

**Diagnostic Steps:**
```bash
# Test database connectivity
sqlcmd -S "server" -d "database" -U "username" -P "password" -Q "SELECT GETDATE()"

# Check SQL Server status
sudo systemctl status mssql-server

# Review SQL Server logs
sudo tail -f /var/opt/mssql/log/errorlog
```

**Solutions:**
1. **Connection String Issues:**
   - Verify server name and port
   - Check authentication credentials
   - Ensure database exists

2. **Network Issues:**
   ```bash
   # Test network connectivity
   telnet sql-server 1433
   
   # Check firewall rules
   sudo ufw status
   ```

3. **SQL Server Configuration:**
   ```sql
   -- Enable TCP/IP connections
   EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', 
       N'Software\Microsoft\MSSQLServer\MSSQLServer', 
       N'LoginMode', REG_DWORD, 2;
   ```

#### Migration Performance Issues

**Symptoms:**
- Slow migration execution
- High CPU/memory usage
- Database timeouts

**Diagnostic Steps:**
```bash
# Monitor system resources
top -p $(pgrep dotnet)
iostat -x 1
free -h

# Check database performance
# Run SQL Server Profiler or Extended Events
```

**Solutions:**
1. **Reduce Batch Size:**
   ```json
   {
     "MigrationSettings": {
       "DefaultBatchSize": 500
     }
   }
   ```

2. **Optimize Database:**
   ```sql
   -- Add indexes for frequently queried columns
   CREATE INDEX IX_Sample_DateCreated ON Sample(DateCreated);
   
   -- Update statistics
   UPDATE STATISTICS Sample;
   ```

3. **Increase Resources:**
   - Add more RAM
   - Use SSD storage
   - Increase CPU cores

### Log Analysis

#### Common Log Patterns

**Successful Migration:**
```
[14:30:22 INF] Migration started: mig_20231113_143022
[14:30:23 INF] Database seeding started for 18 tables
[14:35:45 INF] Database seeding completed: 67,450 records inserted
[14:36:00 INF] SQL validation started for 25 queries
[14:38:15 INF] SQL validation completed: 23 passed, 2 failed
[14:38:30 INF] Migration completed successfully in 00:08:08
```

**Failed Migration:**
```
[14:30:22 INF] Migration started: mig_20231113_143022
[14:30:23 INF] Database seeding started for 18 tables
[14:32:15 ERR] Failed to seed table 'Equipment': Violation of PRIMARY KEY constraint
[14:32:15 ERR] Migration failed: Database seeding error
[14:32:16 INF] Rolling back migration changes
```

#### Log Analysis Tools

```bash
# Search for errors in logs
grep -i "error\|exception\|failed" /var/log/migration-system/migration-*.log

# Count migration attempts by status
grep "Migration completed" /var/log/migration-system/migration-*.log | wc -l
grep "Migration failed" /var/log/migration-system/migration-*.log | wc -l

# Find slow operations
grep "duration.*[5-9][0-9][0-9][0-9]ms" /var/log/migration-system/migration-*.log

# Extract migration statistics
awk '/Migration completed/ {print $0}' /var/log/migration-system/migration-*.log | tail -10
```

## Security Best Practices

### Access Control

#### User Management
```bash
# Create migration admin user
sudo useradd -m -s /bin/bash migration-admin
sudo usermod -aG sudo migration-admin

# Set up SSH key authentication
sudo mkdir /home/migration-admin/.ssh
sudo cp authorized_keys /home/migration-admin/.ssh/
sudo chown -R migration-admin:migration-admin /home/migration-admin/.ssh
sudo chmod 700 /home/migration-admin/.ssh
sudo chmod 600 /home/migration-admin/.ssh/authorized_keys
```

#### Database Security
```sql
-- Create role-based access
CREATE ROLE migration_admin;
CREATE ROLE migration_operator;
CREATE ROLE migration_viewer;

-- Grant permissions to roles
GRANT ALL PRIVILEGES ON SCHEMA::dbo TO migration_admin;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO migration_operator;
GRANT SELECT ON SCHEMA::dbo TO migration_viewer;

-- Assign users to roles
ALTER ROLE migration_admin ADD MEMBER [DOMAIN\MigrationAdmins];
ALTER ROLE migration_operator ADD MEMBER [DOMAIN\MigrationOperators];
ALTER ROLE migration_viewer ADD MEMBER [DOMAIN\MigrationViewers];
```

### Audit and Compliance

#### Audit Configuration
```json
{
  "AuditSettings": {
    "EnableAuditing": true,
    "AuditLevel": "Detailed",
    "RetentionDays": 365,
    "AuditEvents": [
      "MigrationStarted",
      "MigrationCompleted",
      "MigrationFailed",
      "ConfigurationChanged",
      "UserLogin",
      "DataAccess"
    ]
  }
}
```

#### Compliance Reporting
```sql
-- Generate audit report
SELECT 
    EventType,
    UserId,
    EventTime,
    Details,
    IpAddress
FROM AuditLog 
WHERE EventTime >= DATEADD(day, -30, GETDATE())
ORDER BY EventTime DESC;
```

## Support and Escalation

### Support Tiers

#### Tier 1: Self-Service
- Documentation and user guides
- System health dashboard
- Basic troubleshooting steps
- FAQ and knowledge base

#### Tier 2: Technical Support
- Email: migration-support@your-company.com
- Response time: 4 hours (business hours)
- Escalation: Critical issues within 1 hour

#### Tier 3: Engineering Support
- For complex technical issues
- Direct access to development team
- Response time: 2 hours for critical issues

### Escalation Procedures

#### Critical Issues (System Down)
1. **Immediate Actions:**
   - Check system health dashboard
   - Review recent logs for errors
   - Attempt basic troubleshooting steps

2. **Escalation:**
   - Contact Tier 2 support immediately
   - Provide system status and error details
   - Include recent configuration changes

3. **Communication:**
   - Notify stakeholders of outage
   - Provide regular status updates
   - Document resolution steps

#### Non-Critical Issues
1. **Documentation:**
   - Create detailed issue description
   - Include steps to reproduce
   - Attach relevant logs and screenshots

2. **Support Request:**
   - Submit through help desk system
   - Include system environment details
   - Specify business impact

### Emergency Contacts

- **System Administrator**: admin@your-company.com
- **Database Administrator**: dba@your-company.com
- **Development Team Lead**: dev-lead@your-company.com
- **IT Operations**: ops@your-company.com
- **Emergency Hotline**: +1-555-EMERGENCY

## Appendix

### Configuration Templates

#### Docker Compose Template
```yaml
version: '3.8'

services:
  migration-api:
    image: your-registry/migration-system:latest
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - ConnectionStrings__LegacyConnection=${LEGACY_DB_CONNECTION_STRING}
    volumes:
      - ./data:/app/data
      - ./logs:/app/logs
    depends_on:
      - database
    restart: unless-stopped

  database:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SA_PASSWORD}
    volumes:
      - db_data:/var/opt/mssql
    restart: unless-stopped

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - migration-api
    restart: unless-stopped

volumes:
  db_data:
```

#### Monitoring Script Template
```bash
#!/bin/bash
# system-monitor.sh

LOG_FILE="/var/log/migration-system-monitor.log"
ALERT_EMAIL="admin@your-company.com"

# Function to log messages
log_message() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" >> $LOG_FILE
}

# Check service status
check_service() {
    if ! systemctl is-active --quiet migration-system; then
        log_message "ERROR: Migration system service is not running"
        echo "Migration system service is down" | mail -s "Service Alert" $ALERT_EMAIL
        return 1
    fi
    return 0
}

# Check disk space
check_disk_space() {
    USAGE=$(df /opt/migration-system | awk 'NR==2 {print $5}' | sed 's/%//')
    if [ $USAGE -gt 80 ]; then
        log_message "WARNING: Disk usage is ${USAGE}%"
        echo "Disk usage is ${USAGE}%" | mail -s "Disk Space Alert" $ALERT_EMAIL
    fi
}

# Check database connectivity
check_database() {
    if ! sqlcmd -S "localhost" -d "LabResults" -Q "SELECT 1" > /dev/null 2>&1; then
        log_message "ERROR: Cannot connect to database"
        echo "Database connection failed" | mail -s "Database Alert" $ALERT_EMAIL
        return 1
    fi
    return 0
}

# Main monitoring loop
main() {
    log_message "Starting system monitoring"
    
    check_service
    check_disk_space
    check_database
    
    log_message "System monitoring completed"
}

# Run monitoring
main
```

### Useful Commands Reference

#### System Management
```bash
# Service management
sudo systemctl start migration-system
sudo systemctl stop migration-system
sudo systemctl restart migration-system
sudo systemctl status migration-system
sudo systemctl enable migration-system

# Log viewing
sudo journalctl -u migration-system -f
tail -f /var/log/migration-system/migration-*.log
grep -i error /var/log/migration-system/migration-*.log

# Process monitoring
ps aux | grep dotnet
top -p $(pgrep dotnet)
htop

# Disk usage
df -h
du -sh /opt/migration-system/*
```

#### Database Management
```sql
-- Check database size
SELECT 
    DB_NAME() AS DatabaseName,
    (SELECT SUM(size) FROM sys.database_files WHERE type = 0) * 8 / 1024 AS DataSizeMB,
    (SELECT SUM(size) FROM sys.database_files WHERE type = 1) * 8 / 1024 AS LogSizeMB;

-- Check active connections
SELECT 
    session_id,
    login_name,
    host_name,
    program_name,
    status,
    last_request_start_time
FROM sys.dm_exec_sessions
WHERE is_user_process = 1;

-- Check running queries
SELECT 
    r.session_id,
    r.status,
    r.command,
    r.percent_complete,
    r.estimated_completion_time,
    t.text
FROM sys.dm_exec_requests r
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
WHERE r.session_id > 50;
```

#### Network Diagnostics
```bash
# Check port connectivity
telnet hostname 1433
nc -zv hostname 1433

# Check network interfaces
ip addr show
netstat -tlnp

# Test DNS resolution
nslookup hostname
dig hostname

# Check firewall status
sudo ufw status
sudo iptables -L
```