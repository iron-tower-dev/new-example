#!/bin/bash

# =============================================
# Complete CSV Seeding Script
# Seeds remaining important tables
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

# Function to import CSV with identity handling
import_csv_with_identity() {
    local csv_file="$1"
    local table_name="$2"
    local description="$3"
    local skip_identity="$4"  # "yes" to skip identity column
    
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
    
    # Skip if table already has data
    if [[ $before_count -gt 0 ]]; then
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
skip_identity = '$skip_identity' == 'yes'

try:
    with open(csv_file, 'r', encoding='utf-8-sig') as f:
        reader = csv.DictReader(f)
        
        with open(sql_file, 'w') as sql_f:
            sql_f.write('SET NOCOUNT ON;\\n')
            if skip_identity:
                sql_f.write(f'SET IDENTITY_INSERT [{table_name}] ON;\\n')
            sql_f.write('BEGIN TRANSACTION;\\n')
            
            row_count = 0
            for row_num, row in enumerate(reader):
                if row_num >= 2000:  # Limit to first 2000 rows
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
                            val_str = str(val).strip()
                            if val_str == '' or val_str.upper() == 'NULL':
                                values.append('NULL')
                            else:
                                val_escaped = val_str.replace(\"'\", \"''\")
                                values.append(f\"'{val_escaped}'\")
                
                if columns and values:
                    insert_sql = f\"INSERT INTO [{table_name}] ({', '.join(columns)}) VALUES ({', '.join(values)});\\n\"
                    sql_f.write(insert_sql)
                    row_count += 1
            
            sql_f.write('COMMIT TRANSACTION;\\n')
            if skip_identity:
                sql_f.write(f'SET IDENTITY_INSERT [{table_name}] OFF;\\n')
            sql_f.write(f'PRINT \\'{row_count} rows processed\\';\\n')
            
    print(f'SQL file created successfully with {row_count} rows')
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
    
    if echo "$result" | grep -q "rows processed"; then
        local after_count=$(get_row_count "$table_name")
        local imported=$((after_count - before_count))
        print_color $GREEN "✓ Imported $imported rows into $table_name (total: $after_count)"
        rm -f "$temp_sql"
        return 0
    else
        print_color $RED "✗ Failed to import into $table_name"
        echo "$result" | head -5
        rm -f "$temp_sql"
        return 1
    fi
}

# Main execution
print_header "Complete Database Seeding"
print_color $BLUE "This will import remaining CSV data"

# Test connection
if ! sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT 1" > /dev/null 2>&1; then
    print_color $RED "✗ Database connection failed"
    exit 1
fi

print_color $GREEN "✓ Database connection successful"

# Show current status
print_header "Current Table Status"
all_tables=("Location" "LookupList" "Test" "eq_lubrication_pt_t" "limits" "Lubricant" "LubeTechList" "LubeTechQualification" "Lube_Sampling_Point" "AllResults" "EmSpectro" "Ferrogram" "FTIR" "Comments")

for table in "${all_tables[@]}"; do
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
read -p "Proceed with seeding remaining tables? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    print_color $YELLOW "Cancelled by user"
    exit 0
fi

print_header "Starting Complete Import Process"

# Import remaining tables
print_color $CYAN "Step 1: Lookup data with identity"
import_csv_with_identity "db-seeding/LookupList.csv" "LookupList" "Lookup list data" "yes"

print_color $CYAN "Step 2: Personnel qualifications"
import_csv_with_identity "db-seeding/LubeTechQualification.csv" "LubeTechQualification" "Lube technician qualifications" "no"

print_color $CYAN "Step 3: Sampling points"
import_csv_with_identity "db-seeding/Lube_Sampling_Point.csv" "Lube_Sampling_Point" "Lubrication sampling points" "no"

print_color $CYAN "Step 4: Analytical data"
import_csv_with_identity "db-seeding/EmSpectro.csv" "EmSpectro" "Emission spectroscopy data" "no"
import_csv_with_identity "db-seeding/Ferrogram.csv" "Ferrogram" "Ferrogram data" "no"
import_csv_with_identity "db-seeding/FTIR.csv" "FTIR" "FTIR data" "no"

print_color $CYAN "Step 5: Results and comments"
import_csv_with_identity "db-seeding/AllResults.csv" "AllResults" "All results data" "no"
import_csv_with_identity "db-seeding/Comments.csv" "Comments" "Comments data" "no"

print_color $CYAN "Step 6: Additional data"
import_csv_with_identity "db-seeding/Control_Data.csv" "Control_Data" "Control data" "no"
import_csv_with_identity "db-seeding/limits_xref.csv" "limits_xref" "Limits cross-reference" "no"
import_csv_with_identity "db-seeding/lcde_limits.csv" "lcde_limits" "LCDE limits" "no"
import_csv_with_identity "db-seeding/lcde_t.csv" "lcde_t" "LCDE data" "no"

print_header "Complete Import Finished"

# Show final status
print_color $CYAN "Final Table Status:"
for table in "${all_tables[@]}"; do
    count=$(get_row_count "$table")
    if [[ -n "$count" && "$count" =~ ^[0-9]+$ && $count -gt 0 ]]; then
        print_color $GREEN "✓ $table: $count records"
    else
        print_color $YELLOW "⚠ $table: $count records"
    fi
done

print_color $GREEN "Database seeding completed!"
print_color $BLUE "Your database now has comprehensive test data."
print_color $CYAN "Next steps:"
print_color $BLUE "1. Restart your API: cd LabResultsApi && dotnet run"
print_color $BLUE "2. Start your frontend: cd lab-results-frontend && npm start"
print_color $BLUE "3. Test the application with the new data"