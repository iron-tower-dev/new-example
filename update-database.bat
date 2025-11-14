@echo off
REM =============================================
REM Batch Script to Update Database Structure
REM This script executes all SQL files using sqlcmd
REM =============================================

setlocal enabledelayedexpansion

REM Configuration - Update these values for your environment
set SERVER_NAME=localhost
set DATABASE_NAME=lubelab_dev
set USE_WINDOWS_AUTH=1
set USERNAME=
set PASSWORD=

REM Check if sqlcmd is available
sqlcmd -? >nul 2>&1
if errorlevel 1 (
    echo ERROR: sqlcmd is not available. Please install SQL Server Command Line Utilities.
    pause
    exit /b 1
)

echo ==============================================================
echo Database Structure Update Script
echo ==============================================================
echo Server: %SERVER_NAME%
echo Database: %DATABASE_NAME%
echo Authentication: Windows Authentication
echo.

set SUCCESS_COUNT=0
set ERROR_COUNT=0

REM Function to execute SQL file
:execute_sql_file
set "FILE_PATH=%~1"
echo Executing: %FILE_PATH%

if %USE_WINDOWS_AUTH%==1 (
    sqlcmd -S %SERVER_NAME% -d %DATABASE_NAME% -E -i "%FILE_PATH%" -b
) else (
    sqlcmd -S %SERVER_NAME% -d %DATABASE_NAME% -U %USERNAME% -P %PASSWORD% -i "%FILE_PATH%" -b
)

if errorlevel 1 (
    echo ERROR: Failed to execute %FILE_PATH%
    set /a ERROR_COUNT+=1
) else (
    echo SUCCESS: %FILE_PATH%
    set /a SUCCESS_COUNT+=1
)
echo.
goto :eof

REM Step 1: Execute preparation script
echo Step 1: Executing preparation script...
if exist "update-database-structure.sql" (
    call :execute_sql_file "update-database-structure.sql"
) else (
    echo WARNING: Preparation script not found: update-database-structure.sql
)

REM Step 2: Execute core table creation scripts in order
echo Step 2: Creating core tables...
call :execute_sql_file "db-tables\site.sql"
call :execute_sql_file "db-tables\Component.sql"
call :execute_sql_file "db-tables\Location.sql"
call :execute_sql_file "db-tables\MeasurementType.sql"
call :execute_sql_file "db-tables\TestStand.sql"
call :execute_sql_file "db-tables\Test.sql"
call :execute_sql_file "db-tables\UsedLubeSamples.sql"
call :execute_sql_file "db-tables\TestReadings.sql"

REM Step 3: Execute lookup tables
echo Step 3: Creating lookup tables...
call :execute_sql_file "db-tables\LookupList.sql"
call :execute_sql_file "db-tables\NAS_lookup.sql"
call :execute_sql_file "db-tables\NLGILookup.sql"
call :execute_sql_file "db-tables\ParticleTypeDefinition.sql"
call :execute_sql_file "db-tables\ParticleSubTypeDefinition.sql"
call :execute_sql_file "db-tables\ParticleSubTypeCategoryDefinition.sql"

REM Step 4: Execute remaining table files
echo Step 4: Creating remaining tables...
for %%f in (db-tables\*.sql) do (
    REM Skip files already processed
    set "SKIP_FILE=0"
    if "%%~nf"=="site" set "SKIP_FILE=1"
    if "%%~nf"=="Component" set "SKIP_FILE=1"
    if "%%~nf"=="Location" set "SKIP_FILE=1"
    if "%%~nf"=="MeasurementType" set "SKIP_FILE=1"
    if "%%~nf"=="TestStand" set "SKIP_FILE=1"
    if "%%~nf"=="Test" set "SKIP_FILE=1"
    if "%%~nf"=="UsedLubeSamples" set "SKIP_FILE=1"
    if "%%~nf"=="TestReadings" set "SKIP_FILE=1"
    if "%%~nf"=="LookupList" set "SKIP_FILE=1"
    if "%%~nf"=="NAS_lookup" set "SKIP_FILE=1"
    if "%%~nf"=="NLGILookup" set "SKIP_FILE=1"
    if "%%~nf"=="ParticleTypeDefinition" set "SKIP_FILE=1"
    if "%%~nf"=="ParticleSubTypeDefinition" set "SKIP_FILE=1"
    if "%%~nf"=="ParticleSubTypeCategoryDefinition" set "SKIP_FILE=1"
    
    if "!SKIP_FILE!"=="0" (
        call :execute_sql_file "%%f"
    )
)

REM Step 5: Execute function creation scripts
echo Step 5: Creating functions...
for %%f in (db-functions\*.sql) do (
    call :execute_sql_file "%%f"
)

REM Step 6: Execute stored procedure creation scripts
echo Step 6: Creating stored procedures...
for %%f in (db-sp\*.sql) do (
    call :execute_sql_file "%%f"
)

REM Step 7: Execute view creation scripts
echo Step 7: Creating views...
for %%f in (db-views\*.sql) do (
    call :execute_sql_file "%%f"
)

REM Summary
echo ==============================================================
echo Database Update Summary
echo ==============================================================
echo Successful: %SUCCESS_COUNT%
echo Errors: %ERROR_COUNT%

if %ERROR_COUNT%==0 (
    echo.
    echo SUCCESS: Database structure update completed successfully!
) else (
    echo.
    echo WARNING: Database structure update completed with errors.
    echo Please review the error messages above.
)

echo.
echo Next steps:
echo 1. Verify all tables, functions, stored procedures, and views were created
echo 2. Test the application to ensure everything works correctly
echo 3. Consider running any necessary data migration scripts

pause