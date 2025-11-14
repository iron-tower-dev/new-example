#!/bin/bash

# Simple database update script
# Executes all SQL files without complex error handling

set -e  # Exit on error

echo "Starting simple database update..."
echo "Database: LabResultsDb"
echo "Server: localhost"
echo

# Function to execute SQL file
run_sql() {
    local file="$1"
    echo "Executing: $(basename "$file")"
    sqlcmd -S localhost -d LabResultsDb -U sa -P "LabResults123!" -i "$file" > /dev/null
    if [[ $? -eq 0 ]]; then
        echo "✓ Success: $(basename "$file")"
    else
        echo "✗ Failed: $(basename "$file")"
    fi
}

# Execute preparation script
if [[ -f "update-database-structure.sql" ]]; then
    run_sql "update-database-structure.sql"
fi

# Execute table scripts in order
echo
echo "Creating tables..."
for file in db-tables/site.sql \
           db-tables/Component.sql \
           db-tables/Location.sql \
           db-tables/MeasurementType.sql \
           db-tables/TestStand.sql \
           db-tables/Test.sql \
           db-tables/UsedLubeSamples.sql \
           db-tables/TestReadings.sql \
           db-tables/FileUploads.sql; do
    if [[ -f "$file" ]]; then
        run_sql "$file"
    fi
done

# Execute remaining table files
echo
echo "Creating remaining tables..."
for file in db-tables/*.sql; do
    filename=$(basename "$file")
    # Skip files already processed
    if [[ "$filename" != "site.sql" && "$filename" != "Component.sql" && "$filename" != "Location.sql" && 
          "$filename" != "MeasurementType.sql" && "$filename" != "TestStand.sql" && "$filename" != "Test.sql" && 
          "$filename" != "UsedLubeSamples.sql" && "$filename" != "TestReadings.sql" && "$filename" != "FileUploads.sql" ]]; then
        run_sql "$file"
    fi
done

# Execute functions
echo
echo "Creating functions..."
for file in db-functions/*.sql; do
    if [[ -f "$file" ]]; then
        run_sql "$file"
    fi
done

# Execute stored procedures
echo
echo "Creating stored procedures..."
for file in db-sp/*.sql; do
    if [[ -f "$file" ]]; then
        run_sql "$file"
    fi
done

# Execute views
echo
echo "Creating views..."
for file in db-views/*.sql; do
    if [[ -f "$file" ]]; then
        run_sql "$file"
    fi
done

echo
echo "Database update completed!"
echo "Run ./verify-database.sh to check results"