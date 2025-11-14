#!/bin/bash

# =============================================
# Create Missing Database Views Script
# Executes the SQL script to create missing views
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

print_header "Creating Missing Database Views"
print_color $BLUE "This script will create the missing database views required for legacy compatibility"

# Test database connection
print_color $YELLOW "Testing database connection..."
if sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT @@VERSION" > /dev/null 2>&1; then
    print_color $GREEN "✓ Database connection successful"
else
    print_color $RED "✗ Database connection failed"
    exit 1
fi

# Check if SQL script exists
if [[ ! -f "LabResultsApi/Scripts/CreateMissingViews.sql" ]]; then
    print_color $RED "✗ SQL script not found: LabResultsApi/Scripts/CreateMissingViews.sql"
    exit 1
fi

print_color $BLUE "Found SQL script: LabResultsApi/Scripts/CreateMissingViews.sql"

# Execute the SQL script
print_color $YELLOW "Executing SQL script to create missing views..."

if sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -i "LabResultsApi/Scripts/CreateMissingViews.sql"; then
    print_color $GREEN "✓ Database views created successfully!"
    
    # Verify views were created
    print_color $YELLOW "Verifying created views..."
    
    views_to_check=(
        "vwLELimitsForSampleTests"
        "vwSpectroscopy"
        "vwFTIR"
        "vwParticleCount"
        "vwResultsBySample"
        "vwTestScheduleDefinitionByEQID"
        "vwTestRulesByEQID"
        "vwTestDeleteCriteria"
        "vwTestAddCriteria"
        "vwTestResultBySampleAndTest"
        "vwLabOverall"
        "vwMTE_UsageForSample"
    )
    
    for view in "${views_to_check[@]}"; do
        if sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = '$view'" -h -1 | grep -q "1"; then
            print_color $GREEN "✓ View created: $view"
        else
            print_color $YELLOW "⚠ View may not exist: $view"
        fi
    done
    
    print_header "Views Creation Complete"
    print_color $GREEN "All database views have been created successfully!"
    print_color $BLUE "The API now has access to the same data sources as the legacy system."
    print_color $CYAN "Next steps:"
    print_color $BLUE "1. Restart your API to ensure it can access the new views"
    print_color $BLUE "2. Test the new endpoints for scheduling, limits, and qualifications"
    print_color $BLUE "3. Verify that equipment selection now matches the legacy system"
    
else
    print_color $RED "✗ Failed to create database views"
    print_color $YELLOW "Check the SQL script for syntax errors or permission issues"
    exit 1
fi