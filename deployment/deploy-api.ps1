# PowerShell script for deploying Lab Results API to IIS
param(
    [Parameter(Mandatory=$true)]
    [string]$SiteName = "LabResultsApi",
    
    [Parameter(Mandatory=$true)]
    [string]$SitePath = "C:\inetpub\wwwroot\LabResultsApi",
    
    [Parameter(Mandatory=$true)]
    [string]$AppPoolName = "LabResultsApiPool",
    
    [Parameter(Mandatory=$false)]
    [string]$Port = "8080",
    
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Production"
)

Write-Host "Starting Lab Results API deployment..." -ForegroundColor Green

# Import WebAdministration module
Import-Module WebAdministration -ErrorAction Stop

try {
    # Stop existing application pool if it exists
    if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Write-Host "Stopping existing application pool: $AppPoolName" -ForegroundColor Yellow
        Stop-WebAppPool -Name $AppPoolName
        Start-Sleep -Seconds 5
    }

    # Create application pool
    Write-Host "Creating/updating application pool: $AppPoolName" -ForegroundColor Blue
    if (!(Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue)) {
        New-WebAppPool -Name $AppPoolName
    }
    
    # Configure application pool
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "enable32BitAppOnWin64" -Value $false
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.loadUserProfile" -Value $true
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.idleTimeout" -Value "00:00:00"
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "recycling.periodicRestart.time" -Value "00:00:00"

    # Create site directory if it doesn't exist
    if (!(Test-Path $SitePath)) {
        Write-Host "Creating site directory: $SitePath" -ForegroundColor Blue
        New-Item -ItemType Directory -Path $SitePath -Force
    }

    # Remove existing site if it exists
    if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
        Write-Host "Removing existing website: $SiteName" -ForegroundColor Yellow
        Remove-Website -Name $SiteName
    }

    # Create new website
    Write-Host "Creating website: $SiteName on port $Port" -ForegroundColor Blue
    New-Website -Name $SiteName -Port $Port -PhysicalPath $SitePath -ApplicationPool $AppPoolName

    # Set permissions for application pool identity
    Write-Host "Setting permissions for application pool identity..." -ForegroundColor Blue
    $acl = Get-Acl $SitePath
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\$AppPoolName", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule)
    Set-Acl -Path $SitePath -AclObject $acl

    # Create logs directory
    $logsPath = Join-Path $SitePath "logs"
    if (!(Test-Path $logsPath)) {
        New-Item -ItemType Directory -Path $logsPath -Force
        $acl = Get-Acl $logsPath
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\$AppPoolName", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
        $acl.SetAccessRule($accessRule)
        Set-Acl -Path $logsPath -AclObject $acl
    }

    # Create uploads directory
    $uploadsPath = Join-Path $SitePath "uploads"
    if (!(Test-Path $uploadsPath)) {
        New-Item -ItemType Directory -Path $uploadsPath -Force
        $acl = Get-Acl $uploadsPath
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\$AppPoolName", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
        $acl.SetAccessRule($accessRule)
        Set-Acl -Path $uploadsPath -AclObject $acl
    }

    # Start application pool
    Write-Host "Starting application pool: $AppPoolName" -ForegroundColor Green
    Start-WebAppPool -Name $AppPoolName

    Write-Host "Lab Results API deployment completed successfully!" -ForegroundColor Green
    Write-Host "Site URL: http://localhost:$Port" -ForegroundColor Cyan
    Write-Host "Health Check: http://localhost:$Port/health" -ForegroundColor Cyan

} catch {
    Write-Host "Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}