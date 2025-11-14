#!/bin/bash

# =============================================
# Database Verification Script
# Verifies that all database objects were created successfully
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

# Function to execute SQL query and return result
execute_sql_query() {
    local query="$1"
    local description="$2"
    
    print_color $YELLOW "Checking: $description"
    
    local result=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "$query" -h -1 2>/dev/null | grep -o "[0-9]*" | head -1)
    
    if [[ $? -eq 0 && -n "$result" ]]; then
        print_color $GREEN "✓ $description: $result"
        return 0
    else
        print_color $RED "✗ Failed to get $description"
        return 1
    fi
}

# Function to list database objects
list_database_objects() {
    local object_type="$1"
    local query="$2"
    local description="$3"
    
    print_color $YELLOW "Listing $description..."
    
    local results=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "$query" -h -1 2>/dev/null | grep -v "^$")
    
    if [[ $? -eq 0 && -n "$results" ]]; then
        echo "$results" | head -20  # Show first 20 items
        local count=$(echo "$results" | wc -l)
        if [[ $count -gt 20 ]]; then
            print_color $BLUE "... and $((count - 20)) more"
        fi
        return 0
    else
        print_color $RED "✗ No $description found"
        return 1
    fi
}

# Function to check specific key tables
check_key_tables() {
    print_header "Checking Key Tables"
    
    local key_tables=("UsedLubeSamples" "Test" "TestReadings" "FileUploads" "EmSpectro" "Ferrogram" "FTIR" "ParticleCount")
    local found_count=0
    
    for table in "${key_tables[@]}"; do
        local exists=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$table') THEN 1 ELSE 0 END" -h -1 2>/dev/null | grep -o "[01]")
        
        if [[ "$exists" == "1" ]]; then
            print_color $GREEN "✓ Table exists: $table"
            ((found_count++))
        else
            print_color $RED "✗ Table missing: $table"
        fi
    done
    
    print_color $BLUE "Key tables found: $found_count/${#key_tables[@]}"
}

# Function to check table structures
check_table_structures() {
    print_header "Checking Table Structures"
    
    # Check UsedLubeSamples structure
    print_color $YELLOW "Checking UsedLubeSamples columns..."
    local uls_columns=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UsedLubeSamples' ORDER BY ORDINAL_POSITION" -h -1 2>/dev/null | grep -v "^$")
    
    if [[ -n "$uls_columns" ]]; then
        print_color $GREEN "✓ UsedLubeSamples columns:"
        echo "$uls_columns" | head -10
    else
        print_color $RED "✗ Could not retrieve UsedLubeSamples columns"
    fi
    
    # Check Test table structure
    print_color $YELLOW "Checking Test table columns..."
    local test_columns=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Test' ORDER BY ORDINAL_POSITION" -h -1 2>/dev/null | grep -v "^$")
    
    if [[ -n "$test_columns" ]]; then
        print_color $GREEN "✓ Test table columns:"
        echo "$test_columns"
    else
        print_color $RED "✗ Could not retrieve Test table columns"
    fi
}

# Function to check for test data
check_test_data() {
    print_header "Checking Test Data"
    
    # Check if Test table has any data
    local test_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM Test" -h -1 2>/dev/null | grep -o "[0-9]*")
    
    if [[ -n "$test_count" ]]; then
        print_color $GREEN "✓ Test table record count: $test_count"
        
        if [[ $test_count -gt 0 ]]; then
            print_color $YELLOW "Sample test records:"
            sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT TOP 5 ID, name, groupname FROM Test ORDER BY ID" -h -1 2>/dev/null | grep -v "^$"
        else
            print_color $YELLOW "⚠ Test table is empty - you may need to seed test data"
        fi
    else
        print_color $RED "✗ Could not check Test table data"
    fi
    
    # Check UsedLubeSamples data
    local sample_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM UsedLubeSamples" -h -1 2>/dev/null | grep -o "[0-9]*")
    
    if [[ -n "$sample_count" ]]; then
        print_color $GREEN "✓ UsedLubeSamples record count: $sample_count"
    else
        print_color $RED "✗ Could not check UsedLubeSamples data"
    fi
}

# Main verification
print_header "Database Verification Report"
print_color $BLUE "Database: LabResultsDb"
print_color $BLUE "Server: localhost"
print_color $BLUE "User: sa"
echo

# Test connection first
print_color $YELLOW "Testing database connection..."
if sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT @@VERSION" > /dev/null 2>&1; then
    print_color $GREEN "✓ Database connection successful"
else
    print_color $RED "✗ Database connection failed"
    exit 1
fi

# Count all database objects
print_header "Database Object Counts"

execute_sql_query "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'" "Total Tables"
execute_sql_query "SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION'" "Total Functions"
execute_sql_query "SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'" "Total Stored Procedures"
execute_sql_query "SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS" "Total Views"

# List all tables
print_header "All Tables"
list_database_objects "TABLES" "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME" "tables"

# List all functions
print_header "All Functions"
list_database_objects "FUNCTIONS" "SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION' ORDER BY ROUTINE_NAME" "functions"

# List all stored procedures
print_header "All Stored Procedures"
list_database_objects "PROCEDURES" "SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE' ORDER BY ROUTINE_NAME" "stored procedures"

# List all views
print_header "All Views"
list_database_objects "VIEWS" "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS ORDER BY TABLE_NAME" "views"

# Check key tables
check_key_tables

# Check table structures
check_table_structures

# Check for data
check_test_data

# Final summary
print_header "Verification Summary"

# Get final counts
table_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'" -h -1 2>/dev/null | grep -o "[0-9]*")
function_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION'" -h -1 2>/dev/null | grep -o "[0-9]*")
procedure_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'" -h -1 2>/dev/null | grep -o "[0-9]*")
view_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS" -h -1 2>/dev/null | grep -o "[0-9]*")

print_color $GREEN "✓ Database verification completed!"
echo
print_color $BLUE "Final Object Counts:"
print_color $BLUE "  Tables: $table_count"
print_color $BLUE "  Functions: $function_count"
print_color $BLUE "  Stored Procedures: $procedure_count"
print_color $BLUE "  Views: $view_count"
echo

# Expected counts based on file counts
expected_tables=56
expected_functions=8
expected_procedures=18
expected_views=93

print_color $CYAN "Expected vs Actual:"
if [[ $table_count -ge $expected_tables ]]; then
    print_color $GREEN "  Tables: $table_count/$expected_tables ✓"
else
    print_color $YELLOW "  Tables: $table_count/$expected_tables (some may not have been created)"
fi

if [[ $function_count -ge $expected_functions ]]; then
    print_color $GREEN "  Functions: $function_count/$expected_functions ✓"
else
    print_color $YELLOW "  Functions: $function_count/$expected_functions (some may not have been created)"
fi

if [[ $procedure_count -ge $expected_procedures ]]; then
    print_color $GREEN "  Procedures: $procedure_count/$expected_procedures ✓"
else
    print_color $YELLOW "  Procedures: $procedure_count/$expected_procedures (some may not have been created)"
fi

if [[ $view_count -ge $expected_views ]]; then
    print_color $GREEN "  Views: $view_count/$expected_views ✓"
else
    print_color $YELLOW "  Views: $view_count/$expected_views (some may not have been created)"
fi

echo
print_color $CYAN "Next Steps:"
print_color $BLUE "1. If object counts are lower than expected, check the update script output for errors"
print_color $BLUE "2. Test your API to ensure it connects and works properly"
print_color $BLUE "3. Check the frontend test selection for correct test types"
print_color $BLUE "4. Consider seeding reference data from the db-seeding/ directory"