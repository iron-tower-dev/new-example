#!/bin/bash

# =============================================
# Direct CSV Seeding Script
# Simple approach to seed database with CSV files
# =============================================

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

print_color() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

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

# Function to get table row count
get_row_count() {
    local table_name="$1"
    sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT COUNT(*) FROM [$table_name]" -h -1 2>/dev/null | tr -d ' \n\r' | grep -o "[0-9]*"
}

# Function to import CSV directly
import_csv() {
    local csv_file="$1"
    local table_name="$2"
    local description="$3"
    
    print_color $YELLOW "Processing: $description"
    
    # Check if CSV file exists
    if [[ ! -f "$csv_file" ]]; then
        print_color $RED "✗ CSV file not found: $csv_file"
        return 1
    fi
    
    # Get current row count
    local before_count=$(get_row_count "$table_name")
    
    if [[ -z "$before_count" ]]; then
        print_color $RED "✗ Cannot access table: $table_name"
        return 1
    fi
    
    # Skip if table already has data (except for critical tables)
    if [[ $before_count -gt 0 ]] && [[ "$table_name" != "Location" ]] && [[ "$table_name" != "LookupList" ]]; then
        print_color $BLUE "ℹ Table $table_name already has $before_count records, skipping"
        return 0
    fi
    
    # Get absolute path for CSV file
    local abs_csv_path=$(realpath "$csv_file")
    
    # Try BULK INSERT
    print_color $BLUE "  Importing from: $abs_csv_path"
    
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
        SELECT 'SUCCESS: Data imported successfully'
    END TRY
    BEGIN CATCH
        SELECT 'ERROR: ' + ERROR_MESSAGE()
    END CATCH
    "
    
    local result=$(sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "$sql" -h -1 2>&1)
    
    if echo "$result" | grep -q "SUCCESS:"; then
        local after_count=$(get_row_count "$table_name")
        local imported=$((after_count - before_count))
        print_color $GREEN "✓ Imported $imported rows into $table_name (total: $after_count)"
        return 0
    else
        print_color $RED "✗ Failed to import into $table_name"
        echo "$result" | head -2
        return 1
    fi
}

# Main execution
print_header "Direct Database Seeding"
print_color $BLUE "This will import CSV data into the database"

# Test connection
if ! sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT 1" > /dev/null 2>&1; then
    print_color $RED "✗ Database connection failed"
    exit 1
fi

print_color $GREEN "✓ Database connection successful"

# Show current status of key tables
print_header "Current Table Status"
key_tables=("Location" "LookupList" "Test" "eq_lubrication_pt_t" "limits" "Lubricant" "LubeTechList")

for table in "${key_tables[@]}"; do
    count=$(get_row_count "$table")
    if [[ -n "$count" ]]; then
        if [[ $count -gt 0 ]]; then
            print_color $GREEN "✓ $table: $count records"
        else
            print_color $YELLOW "⚠ $table: empty"
        fi
    else
        print_color $RED "✗ $table: not accessible"
    fi
done

echo
read -p "Proceed with seeding? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    print_color $YELLOW "Cancelled by user"
    exit 0
fi

print_header "Starting Import Process"

# Import in logical order
print_color $CYAN "Step 1: Core lookup data"
import_csv "db-seeding/Location.csv" "Location" "Location data"
import_csv "db-seeding/LookupList.csv" "LookupList" "Lookup list data"

print_color $CYAN "Step 2: Equipment and lubrication data"
import_csv "db-seeding/eq_lubrication_pt_t.csv" "eq_lubrication_pt_t" "Equipment lubrication points"
import_csv "db-seeding/Lube_Sampling_Point.csv" "Lube_Sampling_Point" "Lubrication sampling points"
import_csv "db-seeding/Lubricant.csv" "Lubricant" "Lubricant data"

print_color $CYAN "Step 3: Personnel data"
import_csv "db-seeding/LubeTechList.csv" "LubeTechList" "Lube technician list"
import_csv "db-seeding/LubeTechQualification.csv" "LubeTechQualification" "Lube technician qualifications"

print_color $CYAN "Step 4: Test limits and control data"
import_csv "db-seeding/limits.csv" "limits" "Test limits"
import_csv "db-seeding/limits_xref.csv" "limits_xref" "Limits cross-reference"
import_csv "db-seeding/lcde_limits.csv" "lcde_limits" "LCDE limits"
import_csv "db-seeding/lcde_t.csv" "lcde_t" "LCDE data"
import_csv "db-seeding/Control_Data.csv" "Control_Data" "Control data"

print_color $CYAN "Step 5: Particle analysis data"
import_csv "db-seeding/ParticleTypeDefinition.csv" "ParticleTypeDefinition" "Particle type definitions" 2>/dev/null || import_csv "db-seeding/particle-type-definition.csv" "ParticleTypeDefinition" "Particle type definitions"
import_csv "db-seeding/ParticleSubTypeDefinition.csv" "ParticleSubTypeDefinition" "Particle sub-type definitions" 2>/dev/null || import_csv "db-seeding/particle-sub-type-definition.csv" "ParticleSubTypeDefinition" "Particle sub-type definitions"
import_csv "db-seeding/ParticleSubTypeCategoryDefinition.csv" "ParticleSubTypeCategoryDefinition" "Particle sub-type category definitions" 2>/dev/null || import_csv "db-seeding/particle-sub-type-category-definition.csv" "ParticleSubTypeCategoryDefinition" "Particle sub-type category definitions"
import_csv "db-seeding/ParticleType.csv" "ParticleType" "Particle type data" 2>/dev/null || import_csv "db-seeding/particle-type.csv" "ParticleType" "Particle type data"
import_csv "db-seeding/ParticleSubType.csv" "ParticleSubType" "Particle sub-type data" 2>/dev/null || import_csv "db-seeding/particle-sub-type.csv" "ParticleSubType" "Particle sub-type data"

print_color $CYAN "Step 6: Test scheduling data"
import_csv "db-seeding/TestSchedule.csv" "TestSchedule" "Test schedules" 2>/dev/null || import_csv "db-seeding/test-schedule.csv" "TestSchedule" "Test schedules"
import_csv "db-seeding/TestScheduleRule.csv" "TestScheduleRule" "Test schedule rules" 2>/dev/null || import_csv "db-seeding/test-schedule-rule.csv" "TestScheduleRule" "Test schedule rules"
import_csv "db-seeding/TestScheduleTest.csv" "TestScheduleTest" "Test schedule tests" 2>/dev/null || import_csv "db-seeding/test-schedule-test.csv" "TestScheduleTest" "Test schedule tests"

print_color $CYAN "Step 7: Analytical test data"
import_csv "db-seeding/EmSpectro.csv" "EmSpectro" "Emission spectroscopy data"
import_csv "db-seeding/Ferrogram.csv" "Ferrogram" "Ferrogram data"
import_csv "db-seeding/FTIR.csv" "FTIR" "FTIR data"
import_csv "db-seeding/InspectFilter.csv" "InspectFilter" "Filter inspection data"
import_csv "db-seeding/LNFData.csv" "LNFData" "LNF data"

print_color $CYAN "Step 8: Test results data"
import_csv "db-seeding/TestReadings.csv" "TestReadings" "Test readings" 2>/dev/null || import_csv "db-seeding/test-readings.csv" "TestReadings" "Test readings"
import_csv "db-seeding/AllResults.csv" "AllResults" "All results data"
import_csv "db-seeding/ExportTestData.csv" "ExportTestData" "Export test data"

print_color $CYAN "Step 9: Comments and metadata"
import_csv "db-seeding/Comments.csv" "Comments" "Comments data"
import_csv "db-seeding/allsamplecomments.csv" "allsamplecomments" "Sample comments"
import_csv "db-seeding/TestList.csv" "TestList" "Test list data" 2>/dev/null || import_csv "db-seeding/testlist.csv" "TestList" "Test list data"

print_header "Import Complete"

# Show final status
print_color $CYAN "Final Table Status:"
for table in "${key_tables[@]}"; do
    count=$(get_row_count "$table")
    if [[ -n "$count" && $count -gt 0 ]]; then
        print_color $GREEN "✓ $table: $count records"
    else
        print_color $YELLOW "⚠ $table: $count records"
    fi
done

print_color $GREEN "Database seeding completed!"
print_color $BLUE "You can now restart your API and test the frontend."