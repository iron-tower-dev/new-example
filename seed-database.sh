#!/bin/bash

# =============================================
# Database CSV Seeding Script
# Seeds the database with data from CSV files
# =============================================

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

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

# Database connection parameters
SERVER="localhost"
DATABASE="LabResultsDb"
USERNAME="sa"
PASSWORD="LabResults123!"

# Counters
SUCCESS_COUNT=0
ERROR_COUNT=0
SKIP_COUNT=0

# Function to check if table exists
table_exists() {
    local table_name="$1"
    local result=$(sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$table_name') THEN 1 ELSE 0 END" -h -1 2>/dev/null | tr -d ' \n\r' | grep -o "[01]")
    [[ "$result" == "1" ]]
}

# Function to get table row count
get_row_count() {
    local table_name="$1"
    sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT COUNT(*) FROM [$table_name]" -h -1 2>/dev/null | grep -o "[0-9]*" | head -1
}

# Function to import CSV file using BULK INSERT
import_csv_bulk() {
    local csv_file="$1"
    local table_name="$2"
    local description="$3"
    
    print_color $YELLOW "Importing: $description"
    
    # Check if CSV file exists
    if [[ ! -f "$csv_file" ]]; then
        print_color $RED "✗ CSV file not found: $csv_file"
        ((ERROR_COUNT++))
        return 1
    fi
    
    # Check if table exists
    if ! table_exists "$table_name"; then
        print_color $YELLOW "⚠ Table does not exist: $table_name (skipping)"
        ((SKIP_COUNT++))
        return 1
    fi
    
    # Get current row count
    local before_count=$(get_row_count "$table_name")
    
    # Skip if table already has data (unless it's a critical lookup table)
    if [[ $before_count -gt 0 ]] && [[ "$table_name" != "LookupList" ]] && [[ "$table_name" != "Location" ]]; then
        print_color $BLUE "ℹ Table $table_name already has $before_count records, skipping"
        ((SKIP_COUNT++))
        return 0
    fi
    
    # Get absolute path for CSV file
    local abs_csv_path=$(realpath "$csv_file")
    
    # Create BULK INSERT SQL with error handling
    local sql="
    BEGIN TRY
        BULK INSERT [$table_name] 
        FROM '$abs_csv_path' 
        WITH (
            FIELDTERMINATOR = ',', 
            ROWTERMINATOR = '\n', 
            FIRSTROW = 2,
            KEEPIDENTITY,
            TABLOCK
        )
        PRINT 'SUCCESS: Imported data into $table_name'
    END TRY
    BEGIN CATCH
        PRINT 'ERROR: ' + ERROR_MESSAGE()
    END CATCH
    "
    
    # Execute BULK INSERT
    local output=$(sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "$sql" 2>&1)
    
    if echo "$output" | grep -q "SUCCESS:"; then
        local after_count=$(get_row_count "$table_name")
        local imported=$((after_count - before_count))
        print_color $GREEN "✓ Imported $imported rows into $table_name"
        ((SUCCESS_COUNT++))
        return 0
    else
        print_color $RED "✗ Failed to import: $csv_file"
        echo "$output" | head -3
        ((ERROR_COUNT++))
        return 1
    fi
}

# Function to import with custom handling for specific tables
import_csv_custom() {
    local csv_file="$1"
    local table_name="$2"
    local description="$3"
    
    print_color $YELLOW "Custom import: $description"
    
    case "$table_name" in
        "Test")
            # Test table might have identity columns, handle specially
            local current_count=$(get_row_count "Test")
            if [[ $current_count -gt 50 ]]; then
                print_color $BLUE "ℹ Test table already has $current_count records, skipping"
                ((SKIP_COUNT++))
                return 0
            fi
            ;;
        "Location")
            # Always try to import location data
            ;;
    esac
    
    # Fall back to regular bulk import
    import_csv_bulk "$csv_file" "$table_name" "$description"
}

# Main seeding function
seed_database() {
    print_header "Database CSV Seeding Started"
    print_color $BLUE "Database: $DATABASE"
    print_color $BLUE "Server: $SERVER"
    print_color $BLUE "CSV Directory: db-seeding/"
    echo
    
    # Test database connection
    print_color $YELLOW "Testing database connection..."
    if sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT @@VERSION" > /dev/null 2>&1; then
        print_color $GREEN "✓ Database connection successful"
    else
        print_color $RED "✗ Database connection failed"
        exit 1
    fi
    
    # Seed in dependency order
    print_header "Step 1: Seeding Core Lookup Tables"
    
    # Core lookup tables first (these are essential)
    import_csv_bulk "db-seeding/LookupList.csv" "LookupList" "Lookup List data"
    import_csv_bulk "db-seeding/Location.csv" "Location" "Location data"
    import_csv_custom "db-seeding/test.csv" "Test" "Test definitions"
    
    print_header "Step 2: Seeding Equipment and Lubrication Data"
    
    # Equipment and lubrication data
    import_csv_bulk "db-seeding/eq_lubrication_pt_t.csv" "eq_lubrication_pt_t" "Equipment lubrication points"
    import_csv_bulk "db-seeding/Lube_Sampling_Point.csv" "Lube_Sampling_Point" "Lubrication sampling points"
    import_csv_bulk "db-seeding/Lubricant.csv" "Lubricant" "Lubricant data"
    import_csv_bulk "db-seeding/test-stand.csv" "TestStand" "Test stand data"
    
    print_header "Step 3: Seeding Personnel and Qualification Data"
    
    # Personnel data
    import_csv_bulk "db-seeding/LubeTechList.csv" "LubeTechList" "Lube technician list"
    import_csv_bulk "db-seeding/LubeTechQualification.csv" "LubeTechQualification" "Lube technician qualifications"
    
    print_header "Step 4: Seeding Particle Analysis Data"
    
    # Particle definitions
    import_csv_bulk "db-seeding/particle-type-definition.csv" "ParticleTypeDefinition" "Particle type definitions"
    import_csv_bulk "db-seeding/particle-sub-type-definition.csv" "ParticleSubTypeDefinition" "Particle sub-type definitions"
    import_csv_bulk "db-seeding/particle-sub-type-category-definition.csv" "ParticleSubTypeCategoryDefinition" "Particle sub-type category definitions"
    import_csv_bulk "db-seeding/particle-type.csv" "ParticleType" "Particle type data"
    import_csv_bulk "db-seeding/particle-sub-type.csv" "ParticleSubType" "Particle sub-type data"
    
    print_header "Step 5: Seeding Limits and Control Data"
    
    # Limits and control data
    import_csv_bulk "db-seeding/limits.csv" "limits" "Test limits"
    import_csv_bulk "db-seeding/limits_xref.csv" "limits_xref" "Limits cross-reference"
    import_csv_bulk "db-seeding/lcde_limits.csv" "lcde_limits" "LCDE limits"
    import_csv_bulk "db-seeding/lcde_t.csv" "lcde_t" "LCDE data"
    import_csv_bulk "db-seeding/Control_Data.csv" "Control_Data" "Control data"
    
    print_header "Step 6: Seeding Schedule Data"
    
    # Test scheduling data
    import_csv_bulk "db-seeding/test-schedule.csv" "TestSchedule" "Test schedules"
    import_csv_bulk "db-seeding/test-schedule-rule.csv" "TestScheduleRule" "Test schedule rules"
    import_csv_bulk "db-seeding/test-schedule-test.csv" "TestScheduleTest" "Test schedule tests"
    
    print_header "Step 7: Seeding Test Results and Analytical Data"
    
    # Test results data
    import_csv_bulk "db-seeding/test-readings.csv" "TestReadings" "Test readings"
    import_csv_bulk "db-seeding/AllResults.csv" "AllResults" "All results data"
    import_csv_bulk "db-seeding/ExportTestData.csv" "ExportTestData" "Export test data"
    
    # Analytical test data
    import_csv_bulk "db-seeding/EmSpectro.csv" "EmSpectro" "Emission spectroscopy data"
    import_csv_bulk "db-seeding/Ferrogram.csv" "Ferrogram" "Ferrogram data"
    import_csv_bulk "db-seeding/FTIR.csv" "FTIR" "FTIR data"
    import_csv_bulk "db-seeding/InspectFilter.csv" "InspectFilter" "Filter inspection data"
    import_csv_bulk "db-seeding/LNFData.csv" "LNFData" "LNF data"
    
    print_header "Step 8: Seeding Comments and Metadata"
    
    # Comments and metadata
    import_csv_bulk "db-seeding/Comments.csv" "Comments" "Comments data"
    import_csv_bulk "db-seeding/allsamplecomments.csv" "allsamplecomments" "Sample comments"
    
    # Additional files
    import_csv_bulk "db-seeding/testlist.csv" "TestList" "Test list data"
}

# Function to show summary
show_summary() {
    print_header "Seeding Summary"
    
    print_color $GREEN "✓ Successful imports: $SUCCESS_COUNT"
    print_color $YELLOW "⚠ Skipped (table not found or has data): $SKIP_COUNT"
    print_color $RED "✗ Failed imports: $ERROR_COUNT"
    
    local total=$((SUCCESS_COUNT + SKIP_COUNT + ERROR_COUNT))
    if [[ $total -gt 0 ]]; then
        local success_rate=$((SUCCESS_COUNT * 100 / total))
        print_color $BLUE "Success rate: $success_rate%"
    fi
    
    echo
    if [[ $ERROR_COUNT -eq 0 ]]; then
        print_color $GREEN "✓ Database seeding completed successfully!"
    else
        print_color $YELLOW "⚠ Database seeding completed with some errors"
    fi
    
    print_color $CYAN "Next steps:"
    print_color $BLUE "1. Restart your API to ensure it sees the new data"
    print_color $BLUE "2. Test the frontend to verify data is loading correctly"
    print_color $BLUE "3. Check test selection dropdowns for proper data"
}

# Function to show current table status
show_table_status() {
    print_header "Current Table Status"
    
    local key_tables=("Test" "Location" "LookupList" "eq_lubrication_pt_t" "limits" "Lubricant" "LubeTechList")
    
    for table in "${key_tables[@]}"; do
        local count=$(get_row_count "$table")
        if [[ -n "$count" && "$count" != "" ]]; then
            if [[ $count -gt 0 ]]; then
                print_color $GREEN "✓ $table: $count records"
            else
                print_color $YELLOW "⚠ $table: empty"
            fi
        else
            print_color $RED "✗ $table: table not found or query failed"
        fi
    done
}

# Main execution
print_header "Database CSV Seeding Script"
print_color $BLUE "This script will seed your database with data from CSV files"
echo

# Check if db-seeding directory exists
if [[ ! -d "db-seeding" ]]; then
    print_color $RED "✗ db-seeding directory not found"
    print_color $YELLOW "Please ensure you're running this script from the project root directory"
    exit 1
fi

# Count CSV files
csv_count=$(find db-seeding -name "*.csv" | wc -l)
print_color $BLUE "Found $csv_count CSV files to process"

# Show current status
show_table_status

echo
# Ask for confirmation
read -p "Do you want to proceed with seeding? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    print_color $YELLOW "Seeding cancelled by user"
    exit 0
fi

# Record start time
START_TIME=$(date +%s)

# Run the seeding
seed_database

# Calculate duration
END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))
DURATION_FORMATTED=$(printf '%02d:%02d:%02d' $((DURATION/3600)) $((DURATION%3600/60)) $((DURATION%60)))

# Show summary
show_summary

echo
print_color $BLUE "Seeding completed in: $DURATION_FORMATTED"
print_color $BLUE "Timestamp: $(date)"