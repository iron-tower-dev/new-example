# Environment Validation Script for Migration System
# This script validates that the environment is ready for migration operations

param(
    [Parameter(Mandatory=$true)]
    [string]$Environment,
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString,
    
    [Parameter(Mandatory=$false)]
    [string]$LegacyConnectionString,
    
    [Parameter(Mandatory=$false)]
    [switch]$Detailed,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputFormat = "Console", # Console, JSON, CSV
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath
)

$ErrorActionPreference = "Continue"
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $ScriptPath)

# Validation results
$ValidationResults = @{
    Environment = $Environment
    Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    OverallStatus = "Unknown"
    Checks = @()
    Errors = @()
    Warnings = @()
    Recommendations = @()
}

function Add-ValidationCheck {
    param(
        [string]$Name,
        [string]$Category,
        [string]$Status, # Pass, Fail, Warning, Skip
        [string]$Message,
        [string]$Details = "",
        [string]$Recommendation = ""
    )
    
    $check = @{
        Name = $Name
        Category = $Category
        Status = $Status
        Message = $Message
        Details = $Details
        Recommendation = $Recommendation
        Timestamp = Get-Date -Format "HH:mm:ss"
    }
    
    $ValidationResults.Checks += $check
    
    switch ($Status) {
        "Fail" { $ValidationResults.Errors += $check }
        "Warning" { $ValidationResults.Warnings += $check }
    }
    
    if ($Recommendation) {
        $ValidationResults.Recommendations += $check
    }
    
    # Console output
    $color = switch ($Status) {
        "Pass" { "Green" }
        "Fail" { "Red" }
        "Warning" { "Yellow" }
        "Skip" { "Gray" }
        default { "White" }
    }
    
    Write-Host "[$Status] $Category - $Name" -ForegroundColor $color
    if ($Message) {
        Write-Host "  $Message" -ForegroundColor $color
    }
    if ($Detailed -and $Details) {
        Write-Host "  Details: $Details" -ForegroundColor Gray
    }
    if ($Recommendation) {
        Write-Host "  Recommendation: $Recommendation" -ForegroundColor Cyan
    }
}

function Test-SystemRequirements {
    Write-Host "`n=== System Requirements ===" -ForegroundColor Blue
    
    # PowerShell version
    $psVersion = $PSVersionTable.PSVersion
    if ($psVersion.Major -ge 5) {
        Add-ValidationCheck -Name "PowerShell Version" -Category "System" -Status "Pass" -Message "Version $psVersion"
    } else {
        Add-ValidationCheck -Name "PowerShell Version" -Category "System" -Status "Fail" -Message "Version $psVersion (requires 5.0+)" -Recommendation "Upgrade to PowerShell 5.0 or later"
    }
    
    # .NET version
    try {
        $dotnetVersion = & dotnet --version 2>$null
        if ($dotnetVersion) {
            $versionNumber = [Version]$dotnetVersion
            if ($versionNumber.Major -ge 6) {
                Add-ValidationCheck -Name ".NET SDK" -Category "System" -Status "Pass" -Message "Version $dotnetVersion"
            } else {
                Add-ValidationCheck -Name ".NET SDK" -Category "System" -Status "Warning" -Message "Version $dotnetVersion (recommend 6.0+)" -Recommendation "Upgrade to .NET 6.0 or later"
            }
        } else {
            Add-ValidationCheck -Name ".NET SDK" -Category "System" -Status "Fail" -Message "Not found" -Recommendation "Install .NET 6.0 SDK or later"
        }
    }
    catch {
        Add-ValidationCheck -Name ".NET SDK" -Category "System" -Status "Fail" -Message "Not found or not in PATH" -Recommendation "Install .NET 6.0 SDK and ensure it's in PATH"
    }
    
    # Operating System
    $os = Get-WmiObject -Class Win32_OperatingSystem -ErrorAction SilentlyContinue
    if ($os) {
        Add-ValidationCheck -Name "Operating System" -Category "System" -Status "Pass" -Message "$($os.Caption) $($os.Version)" -Details "Architecture: $($os.OSArchitecture)"
    } else {
        Add-ValidationCheck -Name "Operating System" -Category "System" -Status "Warning" -Message "Could not determine OS version"
    }
    
    # Available Memory
    try {
        $memory = Get-WmiObject -Class Win32_ComputerSystem -ErrorAction SilentlyContinue
        if ($memory) {
            $memoryGB = [math]::Round($memory.TotalPhysicalMemory / 1GB, 2)
            if ($memoryGB -ge 4) {
                Add-ValidationCheck -Name "System Memory" -Category "System" -Status "Pass" -Message "$memoryGB GB available"
            } elseif ($memoryGB -ge 2) {
                Add-ValidationCheck -Name "System Memory" -Category "System" -Status "Warning" -Message "$memoryGB GB available (recommend 4GB+)" -Recommendation "Consider adding more memory for better performance"
            } else {
                Add-ValidationCheck -Name "System Memory" -Category "System" -Status "Fail" -Message "$memoryGB GB available (minimum 2GB required)" -Recommendation "Add more system memory"
            }
        }
    }
    catch {
        Add-ValidationCheck -Name "System Memory" -Category "System" -Status "Warning" -Message "Could not determine memory"
    }
    
    # Disk Space
    try {
        $drive = Get-WmiObject -Class Win32_LogicalDisk -Filter "DeviceID='C:'" -ErrorAction SilentlyContinue
        if ($drive) {
            $freeSpaceGB = [math]::Round($drive.FreeSpace / 1GB, 2)
            if ($freeSpaceGB -ge 5) {
                Add-ValidationCheck -Name "Disk Space" -Category "System" -Status "Pass" -Message "$freeSpaceGB GB free on C:"
            } elseif ($freeSpaceGB -ge 1) {
                Add-ValidationCheck -Name "Disk Space" -Category "System" -Status "Warning" -Message "$freeSpaceGB GB free on C: (recommend 5GB+)" -Recommendation "Free up disk space"
            } else {
                Add-ValidationCheck -Name "Disk Space" -Category "System" -Status "Fail" -Message "$freeSpaceGB GB free on C: (minimum 1GB required)" -Recommendation "Free up disk space immediately"
            }
        }
    }
    catch {
        Add-ValidationCheck -Name "Disk Space" -Category "System" -Status "Warning" -Message "Could not determine disk space"
    }
}

function Test-DatabaseConnectivity {
    Write-Host "`n=== Database Connectivity ===" -ForegroundColor Blue
    
    # Main database connection
    if ($ConnectionString) {
        try {
            $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
            $connection.Open()
            
            # Get SQL Server version
            $command = $connection.CreateCommand()
            $command.CommandText = "SELECT @@VERSION"
            $version = $command.ExecuteScalar()
            
            $connection.Close()
            
            Add-ValidationCheck -Name "Main Database Connection" -Category "Database" -Status "Pass" -Message "Connection successful" -Details $version
        }
        catch {
            Add-ValidationCheck -Name "Main Database Connection" -Category "Database" -Status "Fail" -Message "Connection failed: $($_.Exception.Message)" -Recommendation "Check connection string and database availability"
        }
    } else {
        Add-ValidationCheck -Name "Main Database Connection" -Category "Database" -Status "Skip" -Message "No connection string provided"
    }
    
    # Legacy database connection
    if ($LegacyConnectionString) {
        try {
            $connection = New-Object System.Data.SqlClient.SqlConnection($LegacyConnectionString)
            $connection.Open()
            
            $command = $connection.CreateCommand()
            $command.CommandText = "SELECT @@VERSION"
            $version = $command.ExecuteScalar()
            
            $connection.Close()
            
            Add-ValidationCheck -Name "Legacy Database Connection" -Category "Database" -Status "Pass" -Message "Connection successful" -Details $version
        }
        catch {
            Add-ValidationCheck -Name "Legacy Database Connection" -Category "Database" -Status "Fail" -Message "Connection failed: $($_.Exception.Message)" -Recommendation "Check legacy connection string and database availability"
        }
    } else {
        Add-ValidationCheck -Name "Legacy Database Connection" -Category "Database" -Status "Skip" -Message "No legacy connection string provided"
    }
    
    # Check for SQL Server tools
    try {
        $sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
        if ($sqlcmd) {
            Add-ValidationCheck -Name "SQL Server Tools" -Category "Database" -Status "Pass" -Message "sqlcmd found at $($sqlcmd.Source)"
        } else {
            Add-ValidationCheck -Name "SQL Server Tools" -Category "Database" -Status "Warning" -Message "sqlcmd not found" -Recommendation "Install SQL Server Command Line Utilities"
        }
    }
    catch {
        Add-ValidationCheck -Name "SQL Server Tools" -Category "Database" -Status "Warning" -Message "Could not check for SQL Server tools"
    }
}

function Test-ProjectStructure {
    Write-Host "`n=== Project Structure ===" -ForegroundColor Blue
    
    # Required directories
    $requiredDirs = @(
        @{Path="db-seeding"; Description="CSV data files"},
        @{Path="db-tables"; Description="SQL table scripts"},
        @{Path="Configuration"; Description="Migration configurations"},
        @{Path="Scripts/Migration"; Description="Migration scripts"}
    )
    
    foreach ($dir in $requiredDirs) {
        $fullPath = Join-Path $ProjectRoot $dir.Path
        if (Test-Path $fullPath) {
            $fileCount = (Get-ChildItem $fullPath -File -ErrorAction SilentlyContinue).Count
            Add-ValidationCheck -Name "Directory: $($dir.Path)" -Category "Project" -Status "Pass" -Message "Exists with $fileCount files" -Details $dir.Description
        } else {
            Add-ValidationCheck -Name "Directory: $($dir.Path)" -Category "Project" -Status "Fail" -Message "Missing" -Details $dir.Description -Recommendation "Create directory: $fullPath"
        }
    }
    
    # Configuration files
    $configFile = Join-Path $ProjectRoot "Configuration/migration-$($Environment.ToLower()).json"
    if (Test-Path $configFile) {
        try {
            $config = Get-Content $configFile -Raw | ConvertFrom-Json
            Add-ValidationCheck -Name "Environment Configuration" -Category "Project" -Status "Pass" -Message "Valid JSON configuration found"
        }
        catch {
            Add-ValidationCheck -Name "Environment Configuration" -Category "Project" -Status "Fail" -Message "Invalid JSON in configuration file" -Recommendation "Fix JSON syntax in $configFile"
        }
    } else {
        Add-ValidationCheck -Name "Environment Configuration" -Category "Project" -Status "Warning" -Message "No environment-specific configuration" -Recommendation "Create $configFile for environment-specific settings"
    }
    
    # App settings
    $appSettingsFile = Join-Path $ProjectRoot "appsettings.$Environment.json"
    if (Test-Path $appSettingsFile) {
        try {
            $appSettings = Get-Content $appSettingsFile -Raw | ConvertFrom-Json
            Add-ValidationCheck -Name "App Settings" -Category "Project" -Status "Pass" -Message "Valid appsettings file found"
        }
        catch {
            Add-ValidationCheck -Name "App Settings" -Category "Project" -Status "Fail" -Message "Invalid JSON in appsettings file" -Recommendation "Fix JSON syntax in $appSettingsFile"
        }
    } else {
        Add-ValidationCheck -Name "App Settings" -Category "Project" -Status "Warning" -Message "No environment-specific appsettings" -Recommendation "Create $appSettingsFile"
    }
}

function Test-MigrationSystem {
    Write-Host "`n=== Migration System ===" -ForegroundColor Blue
    
    # Check if migration tables exist
    if ($ConnectionString) {
        try {
            $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
            $connection.Open()
            
            $command = $connection.CreateCommand()
            $command.CommandText = @"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME IN ('MigrationHistory', 'MigrationStatistics', 'MigrationErrors', 'TableProcessingStatus')
"@
            $tableCount = $command.ExecuteScalar()
            
            $connection.Close()
            
            if ($tableCount -eq 4) {
                Add-ValidationCheck -Name "Migration Tables" -Category "Migration" -Status "Pass" -Message "All migration tables exist"
            } elseif ($tableCount -gt 0) {
                Add-ValidationCheck -Name "Migration Tables" -Category "Migration" -Status "Warning" -Message "$tableCount of 4 migration tables exist" -Recommendation "Run migration database setup script"
            } else {
                Add-ValidationCheck -Name "Migration Tables" -Category "Migration" -Status "Fail" -Message "No migration tables found" -Recommendation "Run migration database setup script"
            }
        }
        catch {
            Add-ValidationCheck -Name "Migration Tables" -Category "Migration" -Status "Fail" -Message "Could not check migration tables: $($_.Exception.Message)"
        }
    } else {
        Add-ValidationCheck -Name "Migration Tables" -Category "Migration" -Status "Skip" -Message "No connection string provided"
    }
    
    # Check API build
    try {
        $buildResult = & dotnet build $ProjectRoot --configuration Release --verbosity quiet --no-restore 2>&1
        if ($LASTEXITCODE -eq 0) {
            Add-ValidationCheck -Name "API Build" -Category "Migration" -Status "Pass" -Message "Project builds successfully"
        } else {
            Add-ValidationCheck -Name "API Build" -Category "Migration" -Status "Fail" -Message "Build failed" -Details $buildResult -Recommendation "Fix build errors"
        }
    }
    catch {
        Add-ValidationCheck -Name "API Build" -Category "Migration" -Status "Fail" -Message "Could not test build: $($_.Exception.Message)"
    }
}

function Test-SecurityAndPermissions {
    Write-Host "`n=== Security and Permissions ===" -ForegroundColor Blue
    
    # Check write permissions for required directories
    $testDirs = @("logs", "auth-backup", "Configuration")
    
    foreach ($dir in $testDirs) {
        $fullPath = Join-Path $ProjectRoot $dir
        try {
            if (!(Test-Path $fullPath)) {
                New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
            }
            
            $testFile = Join-Path $fullPath "test_$(Get-Random).tmp"
            "test" | Out-File $testFile
            Remove-Item $testFile
            
            Add-ValidationCheck -Name "Write Permission: $dir" -Category "Security" -Status "Pass" -Message "Write access confirmed"
        }
        catch {
            Add-ValidationCheck -Name "Write Permission: $dir" -Category "Security" -Status "Fail" -Message "No write access" -Recommendation "Grant write permissions to $fullPath"
        }
    }
    
    # Check if running as administrator (for production deployments)
    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
    if ($Environment -eq "Production") {
        if ($isAdmin) {
            Add-ValidationCheck -Name "Administrator Rights" -Category "Security" -Status "Pass" -Message "Running as administrator"
        } else {
            Add-ValidationCheck -Name "Administrator Rights" -Category "Security" -Status "Warning" -Message "Not running as administrator" -Recommendation "Consider running as administrator for production deployments"
        }
    } else {
        if ($isAdmin) {
            Add-ValidationCheck -Name "Administrator Rights" -Category "Security" -Status "Pass" -Message "Running as administrator"
        } else {
            Add-ValidationCheck -Name "Administrator Rights" -Category "Security" -Status "Pass" -Message "Running as regular user (appropriate for development)"
        }
    }
}

function Generate-Summary {
    Write-Host "`n=== VALIDATION SUMMARY ===" -ForegroundColor Blue
    
    $totalChecks = $ValidationResults.Checks.Count
    $passedChecks = ($ValidationResults.Checks | Where-Object { $_.Status -eq "Pass" }).Count
    $failedChecks = ($ValidationResults.Checks | Where-Object { $_.Status -eq "Fail" }).Count
    $warningChecks = ($ValidationResults.Checks | Where-Object { $_.Status -eq "Warning" }).Count
    $skippedChecks = ($ValidationResults.Checks | Where-Object { $_.Status -eq "Skip" }).Count
    
    Write-Host "Total Checks: $totalChecks" -ForegroundColor White
    Write-Host "Passed: $passedChecks" -ForegroundColor Green
    Write-Host "Failed: $failedChecks" -ForegroundColor Red
    Write-Host "Warnings: $warningChecks" -ForegroundColor Yellow
    Write-Host "Skipped: $skippedChecks" -ForegroundColor Gray
    
    # Determine overall status
    if ($failedChecks -eq 0 -and $warningChecks -eq 0) {
        $ValidationResults.OverallStatus = "Ready"
        Write-Host "`nOVERALL STATUS: READY FOR MIGRATION" -ForegroundColor Green
    } elseif ($failedChecks -eq 0) {
        $ValidationResults.OverallStatus = "Ready with Warnings"
        Write-Host "`nOVERALL STATUS: READY WITH WARNINGS" -ForegroundColor Yellow
    } else {
        $ValidationResults.OverallStatus = "Not Ready"
        Write-Host "`nOVERALL STATUS: NOT READY FOR MIGRATION" -ForegroundColor Red
    }
    
    # Show recommendations
    if ($ValidationResults.Recommendations.Count -gt 0) {
        Write-Host "`n=== RECOMMENDATIONS ===" -ForegroundColor Cyan
        foreach ($rec in $ValidationResults.Recommendations) {
            Write-Host "• $($rec.Name): $($rec.Recommendation)" -ForegroundColor Cyan
        }
    }
}

function Export-Results {
    if ($OutputPath) {
        switch ($OutputFormat.ToUpper()) {
            "JSON" {
                $ValidationResults | ConvertTo-Json -Depth 10 | Out-File $OutputPath
                Write-Host "`nResults exported to: $OutputPath" -ForegroundColor Green
            }
            "CSV" {
                $ValidationResults.Checks | Export-Csv $OutputPath -NoTypeInformation
                Write-Host "`nResults exported to: $OutputPath" -ForegroundColor Green
            }
            default {
                Write-Host "`nUnsupported output format: $OutputFormat" -ForegroundColor Red
            }
        }
    }
}

# Main execution
Write-Host "Migration Environment Validation" -ForegroundColor Blue
Write-Host "Environment: $Environment" -ForegroundColor Blue
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Blue

Test-SystemRequirements
Test-DatabaseConnectivity
Test-ProjectStructure
Test-MigrationSystem
Test-SecurityAndPermissions
Generate-Summary
Export-Results

# Exit with appropriate code
if ($ValidationResults.OverallStatus -eq "Not Ready") {
    exit 1
} else {
    exit 0
}