#!/bin/bash

# =============================================
# Fix Database Names in SQL Files
# Updates all SQL files to use LabResultsDb instead of lubelab_dev
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

print_color $CYAN "=============================================================="
print_color $CYAN "Fixing Database Names in SQL Files"
print_color $CYAN "=============================================================="
print_color $BLUE "Changing 'lubelab_dev' to 'LabResultsDb' in all SQL files"
echo

# Counters
files_processed=0
files_changed=0

# Function to process files in a directory
process_directory() {
    local dir="$1"
    local description="$2"
    
    if [[ ! -d "$dir" ]]; then
        print_color $YELLOW "⚠ Directory not found: $dir"
        return
    fi
    
    print_color $YELLOW "Processing $description..."
    
    for file in "$dir"/*.sql; do
        if [[ -f "$file" ]]; then
            ((files_processed++))
            
            # Check if file contains the old database name
            if grep -q "lubelab_dev" "$file"; then
                print_color $BLUE "  Updating: $(basename "$file")"
                
                # Create backup
                cp "$file" "$file.backup"
                
                # Replace the database name
                sed -i 's/lubelab_dev/LabResultsDb/g' "$file"
                
                ((files_changed++))
            fi
        fi
    done
}

# Process all directories
process_directory "db-tables" "table creation scripts"
process_directory "db-functions" "function creation scripts"
process_directory "db-sp" "stored procedure scripts"
process_directory "db-views" "view creation scripts"

# Summary
echo
print_color $CYAN "=============================================================="
print_color $CYAN "Database Name Fix Summary"
print_color $CYAN "=============================================================="
print_color $GREEN "Files processed: $files_processed"
print_color $GREEN "Files changed: $files_changed"

if [[ $files_changed -gt 0 ]]; then
    print_color $YELLOW "Backup files created with .backup extension"
    print_color $BLUE "You can now re-run the database update script:"
    print_color $BLUE "  ./update-database.sh"
    print_color $BLUE "  ./update-database.fish"
else
    print_color $GREEN "No files needed to be changed"
fi