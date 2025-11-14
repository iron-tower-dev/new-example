# PowerShell script to validate Lab Results System deployment
param(
    [Parameter(Mandatory=$false)]
    [string]$ApiUrl = "http://localhost:8080",
    
    [Parameter(Mandatory=$false)]
    [string]$FrontendUrl = "http://localhost",
    
    [Parameter(Mandatory=$false)]
    [int]$TimeoutSeconds = 30
)

Write-Host "Starting Lab Results System deployment validation..." -ForegroundColor Green
Write-Host "API URL: $ApiUrl" -ForegroundColor Cyan
Write-Host "Frontend URL: $FrontendUrl" -ForegroundColor Cyan
Write-Host ""

$validationResults = @()
$overallSuccess = $true

function Test-Endpoint {
    param(
        [string]$Url,
        [string]$Description,
        [int]$ExpectedStatusCode = 200,
        [int]$TimeoutSeconds = 30
    )
    
    try {
        Write-Host "Testing: $Description" -NoNewline
        
        $response = Invoke-WebRequest -Uri $Url -TimeoutSec $TimeoutSeconds -UseBasicParsing
        
        if ($response.StatusCode -eq $ExpectedStatusCode) {
            Write-Host " ✓ PASS" -ForegroundColor Green
            return @{ Success = $true; Message = "OK"; StatusCode = $response.StatusCode }
        } else {
            Write-Host " ✗ FAIL (Status: $($response.StatusCode))" -ForegroundColor Red
            return @{ Success = $false; Message = "Unexpected status code: $($response.StatusCode)"; StatusCode = $response.StatusCode }
        }
    }
    catch {
        Write-Host " ✗ FAIL" -ForegroundColor Red
        return @{ Success = $false; Message = $_.Exception.Message; StatusCode = $null }
    }
}

function Test-JsonEndpoint {
    param(
        [string]$Url,
        [string]$Description,
        [string[]]$RequiredFields = @(),
        [int]$TimeoutSeconds = 30
    )
    
    try {
        Write-Host "Testing: $Description" -NoNewline
        
        $response = Invoke-RestMethod -Uri $Url -TimeoutSec $TimeoutSeconds
        
        $missingFields = @()
        foreach ($field in $RequiredFields) {
            if (-not $response.PSObject.Properties.Name.Contains($field)) {
                $missingFields += $field
            }
        }
        
        if ($missingFields.Count -eq 0) {
            Write-Host " ✓ PASS" -ForegroundColor Green
            return @{ Success = $true; Message = "OK"; Data = $response }
        } else {
            Write-Host " ✗ FAIL (Missing fields: $($missingFields -join ', '))" -ForegroundColor Red
            return @{ Success = $false; Message = "Missing required fields: $($missingFields -join ', ')"; Data = $response }
        }
    }
    catch {
        Write-Host " ✗ FAIL" -ForegroundColor Red
        return @{ Success = $false; Message = $_.Exception.Message; Data = $null }
    }
}

# Test API Health Endpoints
Write-Host "=== API Health Checks ===" -ForegroundColor Yellow

$result = Test-JsonEndpoint -Url "$ApiUrl/health" -Description "Basic Health Check" -RequiredFields @("Status", "Timestamp")
$validationResults += @{ Test = "API Basic Health"; Result = $result }
if (-not $result.Success) { $overallSuccess = $false }

$result = Test-JsonEndpoint -Url "$ApiUrl/health/database" -Description "Database Health Check" -RequiredFields @("Status", "Timestamp")
$validationResults += @{ Test = "API Database Health"; Result = $result }
if (-not $result.Success) { $overallSuccess = $false }

$result = Test-JsonEndpoint -Url "$ApiUrl/health/memory" -Description "Memory Health Check" -RequiredFields @("Status", "Timestamp")
$validationResults += @{ Test = "API Memory Health"; Result = $result }
if (-not $result.Success) { $overallSuccess = $false }

$result = Test-JsonEndpoint -Url "$ApiUrl/health/filesystem" -Description "File System Health Check" -RequiredFields @("Status", "Timestamp")
$validationResults += @{ Test = "API File System Health"; Result = $result }
if (-not $result.Success) { $overallSuccess = $false }

$result = Test-JsonEndpoint -Url "$ApiUrl/health/detailed" -Description "Detailed Health Check" -RequiredFields @("Status", "Timestamp", "Checks")
$validationResults += @{ Test = "API Detailed Health"; Result = $result }
if (-not $result.Success) { $overallSuccess = $false }

# Test API Endpoints
Write-Host ""
Write-Host "=== API Endpoint Tests ===" -ForegroundColor Yellow

$result = Test-Endpoint -Url "$ApiUrl/swagger" -Description "Swagger Documentation"
$validationResults += @{ Test = "API Swagger"; Result = $result }
if (-not $result.Success) { $overallSuccess = $false }

# Test Frontend
Write-Host ""
Write-Host "=== Frontend Tests ===" -ForegroundColor Yellow

$result = Test-Endpoint -Url "$FrontendUrl" -Description "Frontend Home Page"
$validationResults += @{ Test = "Frontend Home"; Result = $result }
if (-not $result.Success) { $overallSuccess = $false }

$result = Test-Endpoint -Url "$FrontendUrl/index.html" -Description "Frontend Index"
$validationResults += @{ Test = "Frontend Index"; Result = $result }
if (-not $result.Success) { $overallSuccess = $false }

# Test IIS Configuration
Write-Host ""
Write-Host "=== IIS Configuration Tests ===" -ForegroundColor Yellow

try {
    Import-Module WebAdministration -ErrorAction Stop
    
    # Check API App Pool
    $apiPool = Get-WebAppPool -Name "LabResultsApiPool" -ErrorAction SilentlyContinue
    if ($apiPool -and $apiPool.State -eq "Started") {
        Write-Host "API Application Pool Status: ✓ RUNNING" -ForegroundColor Green
        $validationResults += @{ Test = "API App Pool"; Result = @{ Success = $true; Message = "Running" } }
    } else {
        Write-Host "API Application Pool Status: ✗ NOT RUNNING" -ForegroundColor Red
        $validationResults += @{ Test = "API App Pool"; Result = @{ Success = $false; Message = "Not running or not found" } }
        $overallSuccess = $false
    }
    
    # Check Frontend App Pool
    $frontendPool = Get-WebAppPool -Name "LabResultsFrontendPool" -ErrorAction SilentlyContinue
    if ($frontendPool -and $frontendPool.State -eq "Started") {
        Write-Host "Frontend Application Pool Status: ✓ RUNNING" -ForegroundColor Green
        $validationResults += @{ Test = "Frontend App Pool"; Result = @{ Success = $true; Message = "Running" } }
    } else {
        Write-Host "Frontend Application Pool Status: ✗ NOT RUNNING" -ForegroundColor Red
        $validationResults += @{ Test = "Frontend App Pool"; Result = @{ Success = $false; Message = "Not running or not found" } }
        $overallSuccess = $false
    }
    
    # Check API Site
    $apiSite = Get-Website -Name "LabResultsApi" -ErrorAction SilentlyContinue
    if ($apiSite -and $apiSite.State -eq "Started") {
        Write-Host "API Website Status: ✓ RUNNING" -ForegroundColor Green
        $validationResults += @{ Test = "API Website"; Result = @{ Success = $true; Message = "Running" } }
    } else {
        Write-Host "API Website Status: ✗ NOT RUNNING" -ForegroundColor Red
        $validationResults += @{ Test = "API Website"; Result = @{ Success = $false; Message = "Not running or not found" } }
        $overallSuccess = $false
    }
    
    # Check Frontend Site
    $frontendSite = Get-Website -Name "LabResultsFrontend" -ErrorAction SilentlyContinue
    if ($frontendSite -and $frontendSite.State -eq "Started") {
        Write-Host "Frontend Website Status: ✓ RUNNING" -ForegroundColor Green
        $validationResults += @{ Test = "Frontend Website"; Result = @{ Success = $true; Message = "Running" } }
    } else {
        Write-Host "Frontend Website Status: ✗ NOT RUNNING" -ForegroundColor Red
        $validationResults += @{ Test = "Frontend Website"; Result = @{ Success = $false; Message = "Not running or not found" } }
        $overallSuccess = $false
    }
    
} catch {
    Write-Host "IIS Module Error: $($_.Exception.Message)" -ForegroundColor Red
    $validationResults += @{ Test = "IIS Configuration"; Result = @{ Success = $false; Message = $_.Exception.Message } }
    $overallSuccess = $false
}

# Test File System Permissions
Write-Host ""
Write-Host "=== File System Tests ===" -ForegroundColor Yellow

$testPaths = @(
    "C:\inetpub\wwwroot\LabResultsApi",
    "C:\inetpub\wwwroot\LabResultsFrontend",
    "C:\LabResults\Uploads"
)

foreach ($path in $testPaths) {
    if (Test-Path $path) {
        Write-Host "Directory exists: $path ✓" -ForegroundColor Green
        $validationResults += @{ Test = "Directory: $path"; Result = @{ Success = $true; Message = "Exists" } }
    } else {
        Write-Host "Directory missing: $path ✗" -ForegroundColor Red
        $validationResults += @{ Test = "Directory: $path"; Result = @{ Success = $false; Message = "Missing" } }
        $overallSuccess = $false
    }
}

# Summary
Write-Host ""
Write-Host "=== Validation Summary ===" -ForegroundColor Yellow

$passCount = ($validationResults | Where-Object { $_.Result.Success }).Count
$failCount = ($validationResults | Where-Object { -not $_.Result.Success }).Count
$totalCount = $validationResults.Count

Write-Host "Total Tests: $totalCount" -ForegroundColor Cyan
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red

if ($overallSuccess) {
    Write-Host ""
    Write-Host "🎉 DEPLOYMENT VALIDATION SUCCESSFUL!" -ForegroundColor Green
    Write-Host "All critical components are working correctly." -ForegroundColor Green
    exit 0
} else {
    Write-Host ""
    Write-Host "❌ DEPLOYMENT VALIDATION FAILED!" -ForegroundColor Red
    Write-Host "Please review the failed tests above and fix the issues." -ForegroundColor Red
    
    Write-Host ""
    Write-Host "Failed Tests:" -ForegroundColor Red
    foreach ($validation in $validationResults) {
        if (-not $validation.Result.Success) {
            Write-Host "  - $($validation.Test): $($validation.Result.Message)" -ForegroundColor Red
        }
    }
    
    exit 1
}