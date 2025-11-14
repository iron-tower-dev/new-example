# Migration System Deployment Script
# This script sets up the migration system for the Lab Results API

param(
    [Parameter(Mandatory=$true)]
    [string]$Environment,
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString,
    
    [Parameter(Mandatory=$false)]
    [string]$LegacyConnectionString,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipDatabaseSetup,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipValidation,
    
    [Parameter(Mandatory=$false)]
    [switch]$Force,
    
    [Parameter(Mandatory=$false)]
    [string]$LogPath = "logs/deployment.log"
)

# Script configuration
$ErrorActionPreference = "Stop"
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $ScriptPath)
$ConfigPath = Join-Path $ProjectRoot "Configuration"

# Logging function
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    Write-Host $logMessage
    
    # Ensure log directory exists
    $logDir = Split-Path -Parent $LogPath
    if (!(Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    
    Add-Content -Path $LogPath -Value $logMessage
}

function Test-Prerequisites {
    Write-Log "Checking prerequisites..."
    
    # Check PowerShell version
    if ($PSVersionTable.PSVersion.Major -lt 5) {
        throw "PowerShell 5.0 or later is required"
    }
    Write-Log "PowerShell version: $($PSVersionTable.PSVersion)"
    
    # Check .NET version
    try {
        $dotnetVersion = dotnet --version
        Write-Log ".NET version: $dotnetVersion"
    }
    catch {
        throw ".NET SDK is not installed or not in PATH"
    }
    
    # Check SQL Server connectivity
    if (-not $SkipDatabaseSetup -and $ConnectionString) {
        Write-Log "Testing database connection..."
        try {
            $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
            $connection.Open()
            $connection.Close()
            Write-Log "Database connection successful"
        }
        catch {
            throw "Database connection failed: $($_.Exception.Message)"
        }
    }
    
    # Check required directories
    $requiredDirs = @("db-seeding", "db-tables", "logs")
    foreach ($dir in $requiredDirs) {
        $fullPath = Join-Path $ProjectRoot $dir
        if (!(Test-Path $fullPath)) {
            Write-Log "Creating directory: $fullPath"
            New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
        }
    }
    
    Write-Log "Prerequisites check completed successfully"
}

function Install-DatabaseSchema {
    if ($SkipDatabaseSetup) {
        Write-Log "Skipping database setup as requested"
        return
    }
    
    Write-Log "Installing migration database schema..."
    
    $sqlScript = Join-Path $ScriptPath "01-create-migration-tables.sql"
    if (!(Test-Path $sqlScript)) {
        throw "Migration SQL script not found: $sqlScript"
    }
    
    try {
        if ($ConnectionString) {
            # Use provided connection string
            $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
            $connection.Open()
            
            $command = $connection.CreateCommand()
            $command.CommandText = Get-Content $sqlScript -Raw
            $command.CommandTimeout = 300 # 5 minutes
            
            $result = $command.ExecuteNonQuery()
            $connection.Close()
            
            Write-Log "Database schema installed successfully"
        }
        else {
            # Use sqlcmd with integrated security
            $sqlcmdArgs = @(
                "-S", "localhost",
                "-d", "LabResultsDb",
                "-E",
                "-i", $sqlScript,
                "-b"
            )
            
            $process = Start-Process -FilePath "sqlcmd" -ArgumentList $sqlcmdArgs -Wait -PassThru -NoNewWindow
            if ($process.ExitCode -ne 0) {
                throw "sqlcmd failed with exit code $($process.ExitCode)"
            }
            
            Write-Log "Database schema installed successfully using sqlcmd"
        }
    }
    catch {
        throw "Failed to install database schema: $($_.Exception.Message)"
    }
}

function Deploy-Configuration {
    Write-Log "Deploying configuration for environment: $Environment"
    
    # Validate environment
    $validEnvironments = @("Development", "Staging", "Production")
    if ($Environment -notin $validEnvironments) {
        throw "Invalid environment. Must be one of: $($validEnvironments -join ', ')"
    }
    
    # Check if environment-specific config exists
    $configFile = Join-Path $ConfigPath "migration-$($Environment.ToLower()).json"
    if (!(Test-Path $configFile)) {
        Write-Log "Environment-specific configuration not found: $configFile" -Level "WARN"
        Write-Log "Using default configuration"
    }
    else {
        Write-Log "Found environment configuration: $configFile"
        
        # Validate JSON
        try {
            $config = Get-Content $configFile -Raw | ConvertFrom-Json
            Write-Log "Configuration JSON is valid"
        }
        catch {
            throw "Invalid JSON in configuration file: $($_.Exception.Message)"
        }
    }
    
    # Update appsettings if needed
    $appSettingsFile = Join-Path $ProjectRoot "appsettings.$Environment.json"
    if (Test-Path $appSettingsFile) {
        Write-Log "Found appsettings file: $appSettingsFile"
        
        # Update connection strings if provided
        if ($ConnectionString -or $LegacyConnectionString) {
            try {
                $appSettings = Get-Content $appSettingsFile -Raw | ConvertFrom-Json
                
                if ($ConnectionString) {
                    $appSettings.ConnectionStrings.DefaultConnection = $ConnectionString
                    Write-Log "Updated DefaultConnection in appsettings"
                }
                
                if ($LegacyConnectionString) {
                    $appSettings.ConnectionStrings.LegacyDatabase = $LegacyConnectionString
                    Write-Log "Updated LegacyDatabase connection in appsettings"
                }
                
                $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsFile
                Write-Log "Updated appsettings file"
            }
            catch {
                Write-Log "Failed to update appsettings: $($_.Exception.Message)" -Level "WARN"
            }
        }
    }
}

function Test-Deployment {
    if ($SkipValidation) {
        Write-Log "Skipping deployment validation as requested"
        return
    }
    
    Write-Log "Validating deployment..."
    
    # Test API build
    try {
        $buildResult = dotnet build $ProjectRoot --configuration Release --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed"
        }
        Write-Log "API build successful"
    }
    catch {
        throw "API build validation failed: $($_.Exception.Message)"
    }
    
    # Test database connectivity
    if ($ConnectionString) {
        try {
            $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
            $connection.Open()
            
            # Test migration tables exist
            $command = $connection.CreateCommand()
            $command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('MigrationHistory', 'MigrationStatistics', 'MigrationErrors')"
            $tableCount = $command.ExecuteScalar()
            
            $connection.Close()
            
            if ($tableCount -lt 3) {
                throw "Migration tables not found in database"
            }
            
            Write-Log "Database validation successful - found $tableCount migration tables"
        }
        catch {
            throw "Database validation failed: $($_.Exception.Message)"
        }
    }
    
    # Test configuration files
    $configFiles = Get-ChildItem -Path $ConfigPath -Filter "migration-*.json" -ErrorAction SilentlyContinue
    foreach ($file in $configFiles) {
        try {
            $config = Get-Content $file.FullName -Raw | ConvertFrom-Json
            Write-Log "Configuration file valid: $($file.Name)"
        }
        catch {
            Write-Log "Invalid configuration file: $($file.Name) - $($_.Exception.Message)" -Level "ERROR"
        }
    }
    
    Write-Log "Deployment validation completed successfully"
}

function Show-DeploymentSummary {
    Write-Log "=== DEPLOYMENT SUMMARY ==="
    Write-Log "Environment: $Environment"
    Write-Log "Project Root: $ProjectRoot"
    Write-Log "Configuration Path: $ConfigPath"
    Write-Log "Log Path: $LogPath"
    
    if (-not $SkipDatabaseSetup) {
        Write-Log "Database schema: Installed"
    }
    else {
        Write-Log "Database schema: Skipped"
    }
    
    if (-not $SkipValidation) {
        Write-Log "Validation: Completed"
    }
    else {
        Write-Log "Validation: Skipped"
    }
    
    Write-Log "=== NEXT STEPS ==="
    Write-Log "1. Review the deployment log: $LogPath"
    Write-Log "2. Update connection strings in appsettings.$Environment.json if needed"
    Write-Log "3. Test the migration endpoints using the API"
    Write-Log "4. Run a test migration to verify everything works"
    
    Write-Log "Deployment completed successfully!"
}

# Main execution
try {
    Write-Log "Starting migration system deployment..."
    Write-Log "Environment: $Environment"
    Write-Log "Force: $Force"
    
    # Check if already deployed
    if (-not $Force) {
        $migrationTablesScript = Join-Path $ScriptPath "01-create-migration-tables.sql"
        if ((Test-Path $migrationTablesScript) -and $ConnectionString) {
            try {
                $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
                $connection.Open()
                $command = $connection.CreateCommand()
                $command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MigrationHistory'"
                $exists = $command.ExecuteScalar()
                $connection.Close()
                
                if ($exists -gt 0) {
                    Write-Log "Migration system appears to already be deployed. Use -Force to redeploy." -Level "WARN"
                    exit 0
                }
            }
            catch {
                # Continue with deployment if we can't check
                Write-Log "Could not check existing deployment status, continuing..." -Level "WARN"
            }
        }
    }
    
    Test-Prerequisites
    Install-DatabaseSchema
    Deploy-Configuration
    Test-Deployment
    Show-DeploymentSummary
    
    exit 0
}
catch {
    Write-Log "Deployment failed: $($_.Exception.Message)" -Level "ERROR"
    Write-Log "Stack trace: $($_.ScriptStackTrace)" -Level "ERROR"
    exit 1
}