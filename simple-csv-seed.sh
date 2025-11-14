#!/bin/bash

# =============================================
# Simple CSV Seeding using Docker Volume Mount
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
    local count=$(sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT COUNT(*) FROM [$table_name]" -h -1 2>/dev/null | tr -d ' \n\r' | grep -o '[0-9]*' | head -1)
    echo "$count"
}

# Function to import CSV using INSERT statements
import_csv_with_inserts() {
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
    
    # Create a temporary SQL file with INSERT statements
    local temp_sql="/tmp/import_${table_name}.sql"
    
    # Read the CSV and create INSERT statements
    python3 -c "
import csv
import sys

csv_file = '$csv_file'
table_name = '$table_name'
sql_file = '$temp_sql'

try:
    with open(csv_file, 'r', encoding='utf-8-sig') as f:
        reader = csv.DictReader(f)
        
        with open(sql_file, 'w') as sql_f:
            sql_f.write('SET NOCOUNT ON;\\n')
            sql_f.write('BEGIN TRANSACTION;\\n')
            
            for row_num, row in enumerate(reader):
                if row_num >= 1000:  # Limit to first 1000 rows for safety
                    break
                    
                # Clean and prepare values
                columns = []
                values = []
                
                for col, val in row.items():
                    if col and val is not None:
                        col_clean = col.strip().replace('[', '').replace(']', '')
                        if col_clean:
                            columns.append(f'[{col_clean}]')
                            # Escape single quotes and handle NULL values
                            if val.strip() == '' or val.strip().upper() == 'NULL':
                                values.append('NULL')
                            else:
                                val_escaped = val.replace(\"'\", \"''\")
                                values.append(f\"'{val_escaped}'\")
                
                if columns and values:
                    insert_sql = f\"INSERT INTO [{table_name}] ({', '.join(columns)}) VALUES ({', '.join(values)});\\n\"
                    sql_f.write(insert_sql)
            
            sql_f.write('COMMIT TRANSACTION;\\n')
            sql_f.write('PRINT \\'Import completed\\';\\n')
            
    print('SQL file created successfully')
except Exception as e:
    print(f'Error: {e}')
    sys.exit(1)
"
    
    if [[ $? -ne 0 ]]; then
        print_color $RED "✗ Failed to create SQL file for $table_name"
        return 1
    fi
    
    # Execute the SQL file
    local result=$(sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -i "$temp_sql" 2>&1)
    
    if echo "$result" | grep -q "Import completed"; then
        local after_count=$(get_row_count "$table_name")
        local imported=$((after_count - before_count))
        print_color $GREEN "✓ Imported $imported rows into $table_name (total: $after_count)"
        rm -f "$temp_sql"
        return 0
    else
        print_color $RED "✗ Failed to import into $table_name"
        echo "$result" | head -3
        rm -f "$temp_sql"
        return 1
    fi
}

# Main execution
print_header "Simple CSV Database Seeding"
print_color $BLUE "This will import CSV data using INSERT statements"

# Test connection
if ! sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT 1" > /dev/null 2>&1; then
    print_color $RED "✗ Database connection failed"
    exit 1
fi

print_color $GREEN "✓ Database connection successful"

# Check if Python3 is available
if ! command -v python3 &> /dev/null; then
    print_color $RED "✗ Python3 is required but not found"
    exit 1
fi

# Show current status of key tables
print_header "Current Table Status"
key_tables=("Location" "LookupList" "Test" "eq_lubrication_pt_t" "limits" "Lubricant" "LubeTechList")

for table in "${key_tables[@]}"; do
    count=$(get_row_count "$table")
    if [[ -n "$count" && "$count" =~ ^[0-9]+$ ]]; then
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

# Import key tables only (to avoid overwhelming the system)
print_color $CYAN "Step 1: Core lookup data"
import_csv_with_inserts "db-seeding/Location.csv" "Location" "Location data"
import_csv_with_inserts "db-seeding/LookupList.csv" "LookupList" "Lookup list data"

print_color $CYAN "Step 2: Equipment data"
import_csv_with_inserts "db-seeding/eq_lubrication_pt_t.csv" "eq_lubrication_pt_t" "Equipment lubrication points"
import_csv_with_inserts "db-seeding/Lubricant.csv" "Lubricant" "Lubricant data"

print_color $CYAN "Step 3: Test limits"
import_csv_with_inserts "db-seeding/limits.csv" "limits" "Test limits"

print_color $CYAN "Step 4: Personnel data"
import_csv_with_inserts "db-seeding/LubeTechList.csv" "LubeTechList" "Lube technician list"

print_header "Import Complete"

# Show final status
print_color $CYAN "Final Table Status:"
for table in "${key_tables[@]}"; do
    count=$(get_row_count "$table")
    if [[ -n "$count" && "$count" =~ ^[0-9]+$ && $count -gt 0 ]]; then
        print_color $GREEN "✓ $table: $count records"
    else
        print_color $YELLOW "⚠ $table: $count records"
    fi
done

print_color $GREEN "Key database tables seeded!"
print_color $BLUE "You can now restart your API and test the frontend."
print_color $YELLOW "Note: Only essential tables were seeded. You can run this script again"
print_color $YELLOW "or manually import additional CSV files as needed."