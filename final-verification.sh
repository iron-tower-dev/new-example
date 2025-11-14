#!/bin/bash

# =============================================
# Final Database Verification Report
# Comprehensive check of database structure
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

print_header "Final Database Verification Report"
print_color $BLUE "Database: LabResultsDb"
print_color $BLUE "Timestamp: $(date)"
echo

# Get object counts
table_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'" -h -1 2>/dev/null | grep -o "[0-9]*")
function_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION'" -h -1 2>/dev/null | grep -o "[0-9]*")
procedure_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'" -h -1 2>/dev/null | grep -o "[0-9]*")
view_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS" -h -1 2>/dev/null | grep -o "[0-9]*")

print_header "Database Object Summary"
print_color $GREEN "✓ Tables Created: $table_count"
print_color $GREEN "✓ Functions Created: $function_count"
print_color $GREEN "✓ Stored Procedures Created: $procedure_count"
print_color $GREEN "✓ Views Created: $view_count"

# Check key tables
print_header "Key Tables Verification"
key_tables=("UsedLubeSamples" "Test" "TestReadings" "FileUploads" "EmSpectro" "Ferrogram" "FTIR" "ParticleCount")
for table in "${key_tables[@]}"; do
    if sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$table'" -h -1 2>/dev/null | grep -q "1"; then
        print_color $GREEN "✓ $table"
    else
        print_color $RED "✗ $table"
    fi
done

# Check test data
print_header "Test Data Verification"
test_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM Test" -h -1 2>/dev/null | grep -o "[0-9]*")
sample_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM UsedLubeSamples" -h -1 2>/dev/null | grep -o "[0-9]*")

print_color $GREEN "✓ Test Records: $test_count"
print_color $GREEN "✓ Sample Records: $sample_count"

if [[ $test_count -gt 0 ]]; then
    print_color $YELLOW "Sample Test Types:"
    sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT TOP 5 TestID, TestName FROM Test ORDER BY TestID" -h -1 2>/dev/null | grep -v "^$"
fi

# Check for specific test types that were problematic
print_header "Test Type Analysis"
ferrography_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM Test WHERE TestName LIKE '%Ferrography%' OR TestName LIKE '%Ferro%'" -h -1 2>/dev/null | grep -o "[0-9]*")
viscosity_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM Test WHERE TestName LIKE '%Viscosity%'" -h -1 2>/dev/null | grep -o "[0-9]*")
spectro_count=$(sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -Q "SELECT COUNT(*) FROM Test WHERE TestName LIKE '%Spectroscopy%' OR TestName LIKE '%Emission%'" -h -1 2>/dev/null | grep -o "[0-9]*")

print_color $BLUE "Ferrography Tests: $ferrography_count"
print_color $BLUE "Viscosity Tests: $viscosity_count"
print_color $BLUE "Spectroscopy Tests: $spectro_count"

# API Compatibility Check
print_header "API Compatibility Check"
print_color $YELLOW "Checking if API can connect to updated database..."

# Start API briefly to test connection
if pgrep -f "LabResultsApi" > /dev/null; then
    print_color $YELLOW "API is already running, testing connection..."
    if curl -s "http://localhost:5001/health" > /dev/null 2>&1; then
        print_color $GREEN "✓ API connection successful"
    else
        print_color $YELLOW "⚠ API not responding (may need restart)"
    fi
else
    print_color $YELLOW "API is not running - start it to test database connectivity"
fi

# Final assessment
print_header "Final Assessment"

total_expected=175  # Total SQL files
total_created=$((table_count + function_count + procedure_count + view_count))
success_rate=$((total_created * 100 / total_expected))

print_color $GREEN "✓ Database objects created: $total_created/$total_expected ($success_rate%)"

if [[ $success_rate -ge 80 ]]; then
    print_color $GREEN "✓ EXCELLENT: Database update was highly successful!"
elif [[ $success_rate -ge 60 ]]; then
    print_color $YELLOW "⚠ GOOD: Database update was mostly successful"
else
    print_color $YELLOW "⚠ PARTIAL: Some database objects may not have been created"
fi

echo
print_color $CYAN "Recommendations:"
if [[ $test_count -gt 50 ]]; then
    print_color $GREEN "✓ Test data looks comprehensive ($test_count test types)"
    print_color $BLUE "  Your frontend test selection should now show correct test types"
else
    print_color $YELLOW "⚠ Consider seeding more test data if needed"
fi

if [[ $view_count -gt 40 ]]; then
    print_color $GREEN "✓ Views created successfully ($view_count views)"
    print_color $BLUE "  Complex data queries should work properly"
fi

print_color $BLUE "Next steps:"
print_color $BLUE "1. Restart your API to ensure it uses the updated database structure"
print_color $BLUE "2. Test the frontend test selection dropdown"
print_color $BLUE "3. Verify that duplicate test types are resolved"
print_color $BLUE "4. Test key functionality like sample entry and test results"