# PowerShell script for deploying Lab Results Frontend to IIS
param(
    [Parameter(Mandatory=$true)]
    [string]$SiteName = "LabResultsFrontend",
    
    [Parameter(Mandatory=$true)]
    [string]$SitePath = "C:\inetpub\wwwroot\LabResultsFrontend",
    
    [Parameter(Mandatory=$true)]
    [string]$AppPoolName = "LabResultsFrontendPool",
    
    [Parameter(Mandatory=$false)]
    [string]$Port = "80",
    
    [Parameter(Mandatory=$false)]
    [string]$SourcePath = "..\lab-results-frontend\dist\lab-results-frontend"
)

Write-Host "Starting Lab Results Frontend deployment..." -ForegroundColor Green

# Import WebAdministration module
Import-Module WebAdministration -ErrorAction Stop

try {
    # Stop existing application pool if it exists
    if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Write-Host "Stopping existing application pool: $AppPoolName" -ForegroundColor Yellow
        Stop-WebAppPool -Name $AppPoolName
        Start-Sleep -Seconds 5
    }

    # Create application pool for static content
    Write-Host "Creating/updating application pool: $AppPoolName" -ForegroundColor Blue
    if (!(Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue)) {
        New-WebAppPool -Name $AppPoolName
    }
    
    # Configure application pool for static content
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""
    Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "enable32BitAppOnWin64" -Value $false

    # Create site directory if it doesn't exist
    if (!(Test-Path $SitePath)) {
        Write-Host "Creating site directory: $SitePath" -ForegroundColor Blue
        New-Item -ItemType Directory -Path $SitePath -Force
    }

    # Copy built Angular application
    if (Test-Path $SourcePath) {
        Write-Host "Copying Angular application from: $SourcePath" -ForegroundColor Blue
        Copy-Item -Path "$SourcePath\*" -Destination $SitePath -Recurse -Force
    } else {
        Write-Host "Warning: Source path not found: $SourcePath" -ForegroundColor Yellow
        Write-Host "Please build the Angular application first using: ng build --configuration production" -ForegroundColor Yellow
    }

    # Remove existing site if it exists
    if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
        Write-Host "Removing existing website: $SiteName" -ForegroundColor Yellow
        Remove-Website -Name $SiteName
    }

    # Create new website
    Write-Host "Creating website: $SiteName on port $Port" -ForegroundColor Blue
    New-Website -Name $SiteName -Port $Port -PhysicalPath $SitePath -ApplicationPool $AppPoolName

    # Configure URL Rewrite for Angular routing
    $webConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Angular Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
            <add input="{REQUEST_URI}" pattern="^/(api)" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
    
    <!-- Security Headers -->
    <httpProtocol>
      <customHeaders>
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="X-Frame-Options" value="DENY" />
        <add name="X-XSS-Protection" value="1; mode=block" />
        <add name="Referrer-Policy" value="strict-origin-when-cross-origin" />
      </customHeaders>
    </httpProtocol>
    
    <!-- Compression -->
    <urlCompression doStaticCompression="true" doDynamicCompression="true" />
    
    <!-- Static Content Caching -->
    <staticContent>
      <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="7.00:00:00" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <mimeMap fileExtension=".woff" mimeType="application/font-woff" />
      <mimeMap fileExtension=".woff2" mimeType="application/font-woff2" />
    </staticContent>
    
    <!-- Default Document -->
    <defaultDocument>
      <files>
        <clear />
        <add value="index.html" />
      </files>
    </defaultDocument>
    
  </system.webServer>
</configuration>
"@

    # Write web.config for Angular routing
    $webConfigPath = Join-Path $SitePath "web.config"
    $webConfigContent | Out-File -FilePath $webConfigPath -Encoding UTF8

    # Set permissions
    Write-Host "Setting permissions for application pool identity..." -ForegroundColor Blue
    $acl = Get-Acl $SitePath
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\$AppPoolName", "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule)
    Set-Acl -Path $SitePath -AclObject $acl

    # Start application pool
    Write-Host "Starting application pool: $AppPoolName" -ForegroundColor Green
    Start-WebAppPool -Name $AppPoolName

    Write-Host "Lab Results Frontend deployment completed successfully!" -ForegroundColor Green
    Write-Host "Site URL: http://localhost:$Port" -ForegroundColor Cyan

} catch {
    Write-Host "Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}