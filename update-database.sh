#!/bin/bash

# =============================================
# Bash Script to Update Database Structure
# This script executes all SQL files using sqlcmd
# Compatible with Linux systems
# =============================================

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration - Update these values for your environment
SERVER_NAME="localhost"
DATABASE_NAME="LabResultsDb"  # Updated to match API configuration
USE_WINDOWS_AUTH=0  # Changed to 0 for Linux - Windows Auth doesn't work on Linux
USERNAME="sa"  # Default from API configuration
PASSWORD="LabResults123!"  # Default from API configuration

# Counters
SUCCESS_COUNT=0
ERROR_COUNT=0

# Function to print colored output
print_color() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

# Function to print header
print_header() {
    echo
    print_color $CYAN "=============================================================="
    print_color $CYAN "$1"
    print_color $CYAN "=============================================================="
}

# Function to execute SQL file
execute_sql_file() {
    local file_path="$1"
    local file_name=$(basename "$file_path")
    
    print_color $YELLOW "Executing: $file_path"
    
    if [[ ! -f "$file_path" ]]; then
        print_color $RED "✗ File not found: $file_path"
        ((ERROR_COUNT++))
        return 1
    fi
    
    # Build sqlcmd command
    local cmd_args="-S $SERVER_NAME -d $DATABASE_NAME -b"
    
    if [[ $USE_WINDOWS_AUTH -eq 1 ]]; then
        # Windows Authentication (only works on Windows)
        cmd_args="$cmd_args -E"
    else
        # SQL Server Authentication
        if [[ -z "$USERNAME" || -z "$PASSWORD" ]]; then
            print_color $RED "✗ Username and password required for SQL Server authentication"
            print_color $YELLOW "  Use: $0 -u username -p password"
            print_color $YELLOW "  Or set USERNAME and PASSWORD variables in the script"
            ((ERROR_COUNT++))
            return 1
        fi
        cmd_args="$cmd_args -U $USERNAME -P $PASSWORD"
    fi
    
    # Execute the SQL file
    if sqlcmd $cmd_args -i "$file_path" > /dev/null 2>&1; then
        print_color $GREEN "✓ Successfully executed: $file_name"
        ((SUCCESS_COUNT++))
        return 0
    else
        print_color $RED "✗ Error executing: $file_name"
        # Try to get more detailed error info
        sqlcmd $cmd_args -i "$file_path" 2>&1 | head -5
        ((ERROR_COUNT++))
        return 1
    fi
}

# Function to check prerequisites
check_prerequisites() {
    print_header "Checking Prerequisites"
    
    # Check if sqlcmd is available
    if ! command -v sqlcmd &> /dev/null; then
        print_color $RED "ERROR: sqlcmd is not available."
        print_color $YELLOW "On Arch Linux, install it with:"
        print_color $YELLOW "  yay -S mssql-tools"
        print_color $YELLOW "  # or"
        print_color $YELLOW "  sudo pacman -S mssql-tools"
        exit 1
    fi
    
    print_color $GREEN "✓ sqlcmd is available"
    
    # Check if required directories exist
    local dirs=("db-tables" "db-functions" "db-sp" "db-views")
    for dir in "${dirs[@]}"; do
        if [[ ! -d "$dir" ]]; then
            print_color $RED "✗ Required directory not found: $dir"
            exit 1
        fi
        print_color $GREEN "✓ Directory found: $dir"
    done
}

# Function to get table creation order
get_table_creation_order() {
    # Core tables that other tables depend on
    local core_files=(
        "site.sql"
        "Component.sql"
        "Location.sql"
        "MeasurementType.sql"
        "TestStand.sql"
        "Test.sql"
        "UsedLubeSamples.sql"
        "TestReadings.sql"
    )
    
    # Lookup tables
    local lookup_files=(
        "LookupList.sql"
        "NAS_lookup.sql"
        "NLGILookup.sql"
        "ParticleTypeDefinition.sql"
        "ParticleSubTypeDefinition.sql"
        "ParticleSubTypeCategoryDefinition.sql"
    )
    
    # Data tables
    local data_files=(
        "AllResults.sql"
        "Comments.sql"
        "allsamplecomments.sql"
        "Control_Data.sql"
        "EmSpectro.sql"
        "ExportTestData.sql"
        "Ferrogram.sql"
        "FileUploads.sql"
        "FTIR.sql"
        "InspectFilter.sql"
        "LNFData.sql"
        "Lubricant.sql"
        "LubeTechList.sql"
        "LubeTechQualification.sql"
        "ParticleCount.sql"
        "ParticleType.sql"
        "ParticleSubType.sql"
        "rheometer.sql"
        "RheometerCalcs.sql"
        "System_Log.sql"
        "TestList.sql"
    )
    
    # Schedule and limit tables
    local schedule_files=(
        "TestSchedule.sql"
        "TestScheduleRule.sql"
        "TestScheduleTest.sql"
        "limits.sql"
        "limits_xref.sql"
        "lcde_limits.sql"
        "lcde_t.sql"
    )
    
    # Equipment and work management
    local equipment_files=(
        "eq_lubrication_pt_t.sql"
        "Lube_Sampling_Point.sql"
        "lubpipoints.sql"
        "lubpipointsNEW.sql"
        "M_And_T_Equip.sql"
        "piEngUnit.sql"
        "workmgmt.sql"
        "SWMSRecords.sql"
        "ScheduleDeletions.sql"
    )
    
    # Special data tables
    local special_files=(
        "ManualSIMCAData.sql"
        "ReviewerList.sql"
        "enterResults.sql"
        "enterResultsFunctions.sql"
        "saveResultsFunctions.sql"
    )
    
    # Combine all arrays
    local all_files=("${core_files[@]}" "${lookup_files[@]}" "${data_files[@]}" "${schedule_files[@]}" "${equipment_files[@]}" "${special_files[@]}")
    
    # Return the array
    printf '%s\n' "${all_files[@]}"
}

# Function to execute files in directory
execute_directory_files() {
    local directory="$1"
    local description="$2"
    local ordered_files=("${@:3}")  # Get remaining arguments as array
    
    print_header "$description"
    
    # Execute ordered files first
    if [[ ${#ordered_files[@]} -gt 0 ]]; then
        for file in "${ordered_files[@]}"; do
            local file_path="$directory/$file"
            if [[ -f "$file_path" ]]; then
                execute_sql_file "$file_path"
            else
                print_color $YELLOW "⚠ Ordered file not found: $file_path"
            fi
        done
    fi
    
    # Execute remaining files in the directory
    if [[ -d "$directory" ]]; then
        for file_path in "$directory"/*.sql; do
            if [[ -f "$file_path" ]]; then
                local file_name=$(basename "$file_path")
                
                # Skip if already processed in ordered list
                local skip=false
                for ordered_file in "${ordered_files[@]}"; do
                    if [[ "$file_name" == "$ordered_file" ]]; then
                        skip=true
                        break
                    fi
                done
                
                if [[ "$skip" == false ]]; then
                    execute_sql_file "$file_path"
                fi
            fi
        done
    fi
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -s, --server SERVER     SQL Server instance name (default: localhost)"
    echo "  -d, --database DATABASE Database name (default: LabResultsDb)"
    echo "  -u, --username USERNAME SQL Server username (for SQL auth)"
    echo "  -p, --password PASSWORD SQL Server password (for SQL auth)"
    echo "  -w, --windows-auth      Use Windows authentication (default)"
    echo "  -q, --sql-auth          Use SQL Server authentication"
    echo "  -h, --help              Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                                          # Use defaults (SQL Server auth)"
    echo "  $0 -s myserver -d mydb                     # Custom server and database"
    echo "  $0 -s myserver -d mydb -u myuser -p mypass # Custom credentials"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -s|--server)
            SERVER_NAME="$2"
            shift 2
            ;;
        -d|--database)
            DATABASE_NAME="$2"
            shift 2
            ;;
        -u|--username)
            USERNAME="$2"
            shift 2
            ;;
        -p|--password)
            PASSWORD="$2"
            shift 2
            ;;
        -w|--windows-auth)
            USE_WINDOWS_AUTH=1
            shift
            ;;
        -q|--sql-auth)
            USE_WINDOWS_AUTH=0
            shift
            ;;
        -h|--help)
            show_usage
            exit 0
            ;;
        *)
            print_color $RED "Unknown option: $1"
            show_usage
            exit 1
            ;;
    esac
done

# Main execution
print_header "Database Structure Update Script"
print_color $BLUE "Server: $SERVER_NAME"
print_color $BLUE "Database: $DATABASE_NAME"
print_color $BLUE "Authentication: $(if [[ $USE_WINDOWS_AUTH -eq 1 ]]; then echo 'Windows'; else echo 'SQL Server'; fi)"
echo

# Record start time
START_TIME=$(date +%s)

# Check prerequisites
check_prerequisites

# Step 1: Execute preparation script
print_header "Step 1: Executing Preparation Script"
if [[ -f "update-database-structure.sql" ]]; then
    execute_sql_file "update-database-structure.sql"
else
    print_color $YELLOW "⚠ Preparation script not found: update-database-structure.sql"
fi

# Step 2: Execute table creation scripts
readarray -t table_order < <(get_table_creation_order)
execute_directory_files "db-tables" "Step 2: Creating Tables" "${table_order[@]}"

# Step 3: Execute function creation scripts
execute_directory_files "db-functions" "Step 3: Creating Functions"

# Step 4: Execute stored procedure creation scripts
execute_directory_files "db-sp" "Step 4: Creating Stored Procedures"

# Step 5: Execute view creation scripts
execute_directory_files "db-views" "Step 5: Creating Views"

# Calculate duration
END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))
DURATION_FORMATTED=$(printf '%02d:%02d:%02d' $((DURATION/3600)) $((DURATION%3600/60)) $((DURATION%60)))

# Summary
print_header "Database Update Summary"
print_color $BLUE "Start Time: $(date -d @$START_TIME '+%Y-%m-%d %H:%M:%S')"
print_color $BLUE "End Time: $(date -d @$END_TIME '+%Y-%m-%d %H:%M:%S')"
print_color $BLUE "Duration: $DURATION_FORMATTED"
print_color $GREEN "Successful: $SUCCESS_COUNT"
if [[ $ERROR_COUNT -gt 0 ]]; then
    print_color $RED "Errors: $ERROR_COUNT"
else
    print_color $GREEN "Errors: $ERROR_COUNT"
fi

echo
if [[ $ERROR_COUNT -eq 0 ]]; then
    print_color $GREEN "✓ Database structure update completed successfully!"
else
    print_color $YELLOW "⚠ Database structure update completed with errors. Please review the error messages above."
fi

echo
print_color $CYAN "Next steps:"
print_color $BLUE "1. Verify all tables, functions, stored procedures, and views were created"
print_color $BLUE "2. Test the application to ensure everything works correctly"
print_color $BLUE "3. Consider running any necessary data migration scripts"