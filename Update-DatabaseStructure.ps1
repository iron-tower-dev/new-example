# =============================================
# PowerShell Script to Update Database Structure
# This script executes all SQL files in the correct order
# =============================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,
    
    [Parameter(Mandatory=$true)]
    [string]$DatabaseName,
    
    [Parameter(Mandatory=$false)]
    [string]$Username,
    
    [Parameter(Mandatory=$false)]
    [string]$Password,
    
    [Parameter(Mandatory=$false)]
    [switch]$UseWindowsAuth = $true
)

# Import SQL Server module if available
try {
    Import-Module SqlServer -ErrorAction Stop
    Write-Host "SQL Server module loaded successfully." -ForegroundColor Green
} catch {
    Write-Warning "SQL Server module not available. Using sqlcmd instead."
}

# Function to execute SQL file
function Execute-SqlFile {
    param(
        [string]$FilePath,
        [string]$Server,
        [string]$Database,
        [string]$User,
        [string]$Pass,
        [bool]$WindowsAuth
    )
    
    Write-Host "Executing: $FilePath" -ForegroundColor Yellow
    
    try {
        if ($WindowsAuth) {
            if (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue) {
                Invoke-Sqlcmd -ServerInstance $Server -Database $Database -InputFile $FilePath -ErrorAction Stop
            } else {
                sqlcmd -S $Server -d $Database -E -i $FilePath
            }
        } else {
            if (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue) {
                Invoke-Sqlcmd -ServerInstance $Server -Database $Database -Username $User -Password $Pass -InputFile $FilePath -ErrorAction Stop
            } else {
                sqlcmd -S $Server -d $Database -U $User -P $Pass -i $FilePath
            }
        }
        Write-Host "✓ Successfully executed: $FilePath" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "✗ Error executing: $FilePath" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Function to get files in dependency order
function Get-TableCreationOrder {
    # Core tables that other tables depend on
    $coreFiles = @(
        "site.sql",
        "Component.sql",
        "Location.sql",
        "MeasurementType.sql",
        "TestStand.sql",
        "Test.sql",
        "UsedLubeSamples.sql",
        "TestReadings.sql"
    )
    
    # Lookup tables
    $lookupFiles = @(
        "LookupList.sql",
        "NAS_lookup.sql",
        "NLGILookup.sql",
        "ParticleTypeDefinition.sql",
        "ParticleSubTypeDefinition.sql",
        "ParticleSubTypeCategoryDefinition.sql"
    )
    
    # Data tables
    $dataFiles = @(
        "AllResults.sql",
        "Comments.sql",
        "allsamplecomments.sql",
        "Control_Data.sql",
        "EmSpectro.sql",
        "ExportTestData.sql",
        "Ferrogram.sql",
        "FileUploads.sql",
        "FTIR.sql",
        "InspectFilter.sql",
        "LNFData.sql",
        "Lubricant.sql",
        "LubeTechList.sql",
        "LubeTechQualification.sql",
        "ParticleCount.sql",
        "ParticleType.sql",
        "ParticleSubType.sql",
        "rheometer.sql",
        "RheometerCalcs.sql",
        "System_Log.sql",
        "TestList.sql"
    )
    
    # Schedule and limit tables
    $scheduleFiles = @(
        "TestSchedule.sql",
        "TestScheduleRule.sql",
        "TestScheduleTest.sql",
        "limits.sql",
        "limits_xref.sql",
        "lcde_limits.sql",
        "lcde_t.sql"
    )
    
    # Equipment and work management
    $equipmentFiles = @(
        "eq_lubrication_pt_t.sql",
        "Lube_Sampling_Point.sql",
        "lubpipoints.sql",
        "lubpipointsNEW.sql",
        "M_And_T_Equip.sql",
        "piEngUnit.sql",
        "workmgmt.sql",
        "SWMSRecords.sql",
        "ScheduleDeletions.sql"
    )
    
    # Special data tables
    $specialFiles = @(
        "ManualSIMCAData.sql",
        "ReviewerList.sql",
        "enterResults.sql",
        "enterResultsFunctions.sql",
        "saveResultsFunctions.sql"
    )
    
    return $coreFiles + $lookupFiles + $dataFiles + $scheduleFiles + $equipmentFiles + $specialFiles
}

# Main execution
Write-Host "==============================================================" -ForegroundColor Cyan
Write-Host "Database Structure Update Script" -ForegroundColor Cyan
Write-Host "==============================================================" -ForegroundColor Cyan
Write-Host "Server: $ServerName" -ForegroundColor White
Write-Host "Database: $DatabaseName" -ForegroundColor White
Write-Host "Authentication: $(if($UseWindowsAuth) {'Windows'} else {'SQL Server'})" -ForegroundColor White
Write-Host ""

$startTime = Get-Date
$successCount = 0
$errorCount = 0

# Step 1: Execute preparation script
Write-Host "Step 1: Executing preparation script..." -ForegroundColor Cyan
if (Test-Path "update-database-structure.sql") {
    $result = Execute-SqlFile -FilePath "update-database-structure.sql" -Server $ServerName -Database $DatabaseName -User $Username -Pass $Password -WindowsAuth $UseWindowsAuth
    if ($result) { $successCount++ } else { $errorCount++ }
} else {
    Write-Warning "Preparation script not found: update-database-structure.sql"
}

# Step 2: Execute table creation scripts
Write-Host "`nStep 2: Creating tables..." -ForegroundColor Cyan
$tableOrder = Get-TableCreationOrder
foreach ($tableFile in $tableOrder) {
    $filePath = Join-Path "db-tables" $tableFile
    if (Test-Path $filePath) {
        $result = Execute-SqlFile -FilePath $filePath -Server $ServerName -Database $DatabaseName -User $Username -Pass $Password -WindowsAuth $UseWindowsAuth
        if ($result) { $successCount++ } else { $errorCount++ }
    } else {
        Write-Warning "Table file not found: $filePath"
    }
}

# Execute any remaining table files not in the ordered list
$allTableFiles = Get-ChildItem "db-tables" -Filter "*.sql" | Where-Object { $_.Name -notin $tableOrder }
foreach ($file in $allTableFiles) {
    $result = Execute-SqlFile -FilePath $file.FullName -Server $ServerName -Database $DatabaseName -User $Username -Pass $Password -WindowsAuth $UseWindowsAuth
    if ($result) { $successCount++ } else { $errorCount++ }
}

# Step 3: Execute function creation scripts
Write-Host "`nStep 3: Creating functions..." -ForegroundColor Cyan
$functionFiles = Get-ChildItem "db-functions" -Filter "*.sql"
foreach ($file in $functionFiles) {
    $result = Execute-SqlFile -FilePath $file.FullName -Server $ServerName -Database $DatabaseName -User $Username -Pass $Password -WindowsAuth $UseWindowsAuth
    if ($result) { $successCount++ } else { $errorCount++ }
}

# Step 4: Execute stored procedure creation scripts
Write-Host "`nStep 4: Creating stored procedures..." -ForegroundColor Cyan
$spFiles = Get-ChildItem "db-sp" -Filter "*.sql"
foreach ($file in $spFiles) {
    $result = Execute-SqlFile -FilePath $file.FullName -Server $ServerName -Database $DatabaseName -User $Username -Pass $Password -WindowsAuth $UseWindowsAuth
    if ($result) { $successCount++ } else { $errorCount++ }
}

# Step 5: Execute view creation scripts
Write-Host "`nStep 5: Creating views..." -ForegroundColor Cyan
$viewFiles = Get-ChildItem "db-views" -Filter "*.sql"
foreach ($file in $viewFiles) {
    $result = Execute-SqlFile -FilePath $file.FullName -Server $ServerName -Database $DatabaseName -User $Username -Pass $Password -WindowsAuth $UseWindowsAuth
    if ($result) { $successCount++ } else { $errorCount++ }
}

# Summary
$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "`n==============================================================" -ForegroundColor Cyan
Write-Host "Database Update Summary" -ForegroundColor Cyan
Write-Host "==============================================================" -ForegroundColor Cyan
Write-Host "Start Time: $($startTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor White
Write-Host "End Time: $($endTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor White
Write-Host "Duration: $($duration.ToString('hh\:mm\:ss'))" -ForegroundColor White
Write-Host "Successful: $successCount" -ForegroundColor Green
Write-Host "Errors: $errorCount" -ForegroundColor $(if($errorCount -gt 0) {'Red'} else {'Green'})

if ($errorCount -eq 0) {
    Write-Host "`n✓ Database structure update completed successfully!" -ForegroundColor Green
} else {
    Write-Host "`n⚠ Database structure update completed with errors. Please review the error messages above." -ForegroundColor Yellow
}

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Verify all tables, functions, stored procedures, and views were created" -ForegroundColor White
Write-Host "2. Test the application to ensure everything works correctly" -ForegroundColor White
Write-Host "3. Consider running any necessary data migration scripts" -ForegroundColor White