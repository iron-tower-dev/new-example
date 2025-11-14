@echo off
REM Full deployment script for Lab Results System
REM This script deploys both API and Frontend to IIS

echo Starting full Lab Results System deployment...
echo.

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script must be run as Administrator
    echo Please right-click and select "Run as administrator"
    pause
    exit /b 1
)

REM Set deployment parameters
set ENVIRONMENT=%1
if "%ENVIRONMENT%"=="" set ENVIRONMENT=production

echo Deploying to environment: %ENVIRONMENT%
echo.

REM Build .NET API
echo Building .NET API...
cd ..\LabResultsApi
dotnet publish -c Release -o ..\deployment\api-build
if %errorLevel% neq 0 (
    echo ERROR: API build failed
    pause
    exit /b 1
)
echo API build completed successfully
echo.

REM Build Angular Frontend
echo Building Angular Frontend...
cd ..\lab-results-frontend
call npm run build:prod
if %errorLevel% neq 0 (
    echo ERROR: Frontend build failed
    pause
    exit /b 1
)
echo Frontend build completed successfully
echo.

REM Deploy API
echo Deploying API to IIS...
cd ..\deployment
powershell -ExecutionPolicy Bypass -File "deploy-api.ps1" -SiteName "LabResultsApi" -SitePath "C:\inetpub\wwwroot\LabResultsApi" -AppPoolName "LabResultsApiPool" -Port "8080"
if %errorLevel% neq 0 (
    echo ERROR: API deployment failed
    pause
    exit /b 1
)

REM Copy API files
echo Copying API files...
xcopy /E /I /Y api-build\* C:\inetpub\wwwroot\LabResultsApi\
if %errorLevel% neq 0 (
    echo ERROR: API file copy failed
    pause
    exit /b 1
)

REM Deploy Frontend
echo Deploying Frontend to IIS...
powershell -ExecutionPolicy Bypass -File "deploy-frontend.ps1" -SiteName "LabResultsFrontend" -SitePath "C:\inetpub\wwwroot\LabResultsFrontend" -AppPoolName "LabResultsFrontendPool" -Port "80" -SourcePath "..\lab-results-frontend\dist\lab-results-frontend"
if %errorLevel% neq 0 (
    echo ERROR: Frontend deployment failed
    pause
    exit /b 1
)

REM Validate deployment
echo.
echo Running comprehensive deployment validation...
timeout /t 10 /nobreak >nul
powershell -ExecutionPolicy Bypass -File "validate-deployment.ps1" -ApiUrl "http://localhost:8080" -FrontendUrl "http://localhost"
if %errorLevel% neq 0 (
    echo ERROR: Deployment validation failed
    echo Please check the validation results above
    pause
    exit /b 1
) else (
    echo Deployment validation passed successfully
)

echo.
echo ========================================
echo Deployment completed successfully!
echo ========================================
echo.
echo API URL: http://localhost:8080
echo API Health: http://localhost:8080/health
echo API Swagger: http://localhost:8080/swagger
echo Frontend URL: http://localhost
echo.
echo Please verify the applications are working correctly.
echo Check the IIS logs if there are any issues.
echo.
pause