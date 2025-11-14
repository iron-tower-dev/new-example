#!/usr/bin/env python3

import csv
import os
import sys
import pyodbc
from pathlib import Path

# Colors for output
class Colors:
    RED = '\033[0;31m'
    GREEN = '\033[0;32m'
    YELLOW = '\033[1;33m'
    BLUE = '\033[0;34m'
    CYAN = '\033[0;36m'
    NC = '\033[0m'  # No Color

def print_color(color, message):
    print(f"{color}{message}{Colors.NC}")

def print_header(title):
    print()
    print_color(Colors.CYAN, "=" * 60)
    print_color(Colors.CYAN, title)
    print_color(Colors.CYAN, "=" * 60)

# Database connection parameters
SERVER = "localhost"
DATABASE = "LabResultsDb"
USERNAME = "sa"
PASSWORD = "LabResults123!"

def get_connection():
    """Get database connection"""
    try:
        conn_str = f'DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={SERVER};DATABASE={DATABASE};UID={USERNAME};PWD={PASSWORD}'
        return pyodbc.connect(conn_str)
    except Exception as e:
        print_color(Colors.RED, f"✗ Database connection failed: {e}")
        return None

def get_row_count(conn, table_name):
    """Get row count for a table"""
    try:
        cursor = conn.cursor()
        cursor.execute(f"SELECT COUNT(*) FROM [{table_name}]")
        return cursor.fetchone()[0]
    except Exception as e:
        print_color(Colors.RED, f"✗ Error getting row count for {table_name}: {e}")
        return None

def table_exists(conn, table_name):
    """Check if table exists"""
    try:
        cursor = conn.cursor()
        cursor.execute(f"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table_name}'")
        return cursor.fetchone()[0] > 0
    except Exception as e:
        print_color(Colors.RED, f"✗ Error checking table existence for {table_name}: {e}")
        return False

def import_csv_to_table(conn, csv_file, table_name, description):
    """Import CSV file to database table"""
    print_color(Colors.YELLOW, f"Processing: {description}")
    
    # Check if CSV file exists
    if not os.path.exists(csv_file):
        print_color(Colors.RED, f"✗ CSV file not found: {csv_file}")
        return False
    
    # Check if table exists
    if not table_exists(conn, table_name):
        print_color(Colors.RED, f"✗ Table does not exist: {table_name}")
        return False
    
    # Get current row count
    before_count = get_row_count(conn, table_name)
    if before_count is None:
        return False
    
    # Skip if table already has data (except for critical tables)
    if before_count > 0 and table_name not in ['Location', 'LookupList']:
        print_color(Colors.BLUE, f"ℹ Table {table_name} already has {before_count} records, skipping")
        return True
    
    try:
        cursor = conn.cursor()
        
        # Get table columns
        cursor.execute(f"""
            SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = '{table_name}' 
            ORDER BY ORDINAL_POSITION
        """)
        table_columns = cursor.fetchall()
        
        # Read CSV file
        with open(csv_file, 'r', encoding='utf-8-sig') as file:
            csv_reader = csv.DictReader(file)
            csv_columns = csv_reader.fieldnames
            
            if not csv_columns:
                print_color(Colors.RED, f"✗ No columns found in CSV file: {csv_file}")
                return False
            
            # Map CSV columns to table columns (case-insensitive)
            column_mapping = {}
            table_col_names = [col[0].lower() for col in table_columns]
            
            for csv_col in csv_columns:
                csv_col_lower = csv_col.lower()
                if csv_col_lower in table_col_names:
                    # Find the actual table column name
                    for table_col in table_columns:
                        if table_col[0].lower() == csv_col_lower:
                            column_mapping[csv_col] = table_col[0]
                            break
            
            if not column_mapping:
                print_color(Colors.RED, f"✗ No matching columns found between CSV and table {table_name}")
                return False
            
            # Prepare INSERT statement
            mapped_columns = list(column_mapping.values())
            placeholders = ', '.join(['?' for _ in mapped_columns])
            insert_sql = f"INSERT INTO [{table_name}] ([{'], ['.join(mapped_columns)}]) VALUES ({placeholders})"
            
            # Import data row by row
            imported_count = 0
            error_count = 0
            
            for row_num, row in enumerate(csv_reader, start=2):  # Start at 2 because of header
                try:
                    # Extract values for mapped columns
                    values = []
                    for csv_col in column_mapping.keys():
                        value = row.get(csv_col, '').strip()
                        # Convert empty strings to None for nullable columns
                        if value == '':
                            value = None
                        values.append(value)
                    
                    cursor.execute(insert_sql, values)
                    imported_count += 1
                    
                    # Commit every 100 rows
                    if imported_count % 100 == 0:
                        conn.commit()
                        
                except Exception as e:
                    error_count += 1
                    if error_count <= 5:  # Only show first 5 errors
                        print_color(Colors.RED, f"  ✗ Error on row {row_num}: {str(e)[:100]}")
            
            # Final commit
            conn.commit()
            
            after_count = get_row_count(conn, table_name)
            actual_imported = after_count - before_count if after_count is not None else imported_count
            
            if error_count > 0:
                print_color(Colors.YELLOW, f"⚠ Imported {actual_imported} rows into {table_name} with {error_count} errors")
            else:
                print_color(Colors.GREEN, f"✓ Imported {actual_imported} rows into {table_name}")
            
            return True
            
    except Exception as e:
        print_color(Colors.RED, f"✗ Failed to import {csv_file}: {str(e)[:200]}")
        return False

def main():
    print_header("Database CSV Seeding Script")
    print_color(Colors.BLUE, "This script will seed your database with data from CSV files")
    
    # Check if db-seeding directory exists
    if not os.path.exists("db-seeding"):
        print_color(Colors.RED, "✗ db-seeding directory not found")
        print_color(Colors.YELLOW, "Please ensure you're running this script from the project root directory")
        sys.exit(1)
    
    # Count CSV files
    csv_files = list(Path("db-seeding").glob("*.csv"))
    print_color(Colors.BLUE, f"Found {len(csv_files)} CSV files to process")
    
    # Get database connection
    conn = get_connection()
    if not conn:
        sys.exit(1)
    
    print_color(Colors.GREEN, "✓ Database connection successful")
    
    # Show current status of key tables
    print_header("Current Table Status")
    key_tables = ["Location", "LookupList", "Test", "eq_lubrication_pt_t", "limits", "Lubricant", "LubeTechList"]
    
    for table in key_tables:
        count = get_row_count(conn, table)
        if count is not None:
            if count > 0:
                print_color(Colors.GREEN, f"✓ {table}: {count} records")
            else:
                print_color(Colors.YELLOW, f"⚠ {table}: empty")
        else:
            print_color(Colors.RED, f"✗ {table}: not accessible")
    
    # Ask for confirmation
    response = input("\nProceed with seeding? (y/N): ").strip().lower()
    if response != 'y':
        print_color(Colors.YELLOW, "Cancelled by user")
        sys.exit(0)
    
    print_header("Starting Import Process")
    
    # Define import order and mappings
    import_tasks = [
        # Core lookup data
        ("db-seeding/Location.csv", "Location", "Location data"),
        ("db-seeding/LookupList.csv", "LookupList", "Lookup list data"),
        
        # Equipment and lubrication data
        ("db-seeding/eq_lubrication_pt_t.csv", "eq_lubrication_pt_t", "Equipment lubrication points"),
        ("db-seeding/Lube_Sampling_Point.csv", "Lube_Sampling_Point", "Lubrication sampling points"),
        ("db-seeding/Lubricant.csv", "Lubricant", "Lubricant data"),
        
        # Personnel data
        ("db-seeding/LubeTechList.csv", "LubeTechList", "Lube technician list"),
        ("db-seeding/LubeTechQualification.csv", "LubeTechQualification", "Lube technician qualifications"),
        
        # Test limits and control data
        ("db-seeding/limits.csv", "limits", "Test limits"),
        ("db-seeding/limits_xref.csv", "limits_xref", "Limits cross-reference"),
        ("db-seeding/lcde_limits.csv", "lcde_limits", "LCDE limits"),
        ("db-seeding/lcde_t.csv", "lcde_t", "LCDE data"),
        ("db-seeding/Control_Data.csv", "Control_Data", "Control data"),
        
        # Particle analysis data
        ("db-seeding/particle-type-definition.csv", "ParticleTypeDefinition", "Particle type definitions"),
        ("db-seeding/particle-sub-type-definition.csv", "ParticleSubTypeDefinition", "Particle sub-type definitions"),
        ("db-seeding/particle-sub-type-category-definition.csv", "ParticleSubTypeCategoryDefinition", "Particle sub-type category definitions"),
        ("db-seeding/particle-type.csv", "ParticleType", "Particle type data"),
        ("db-seeding/particle-sub-type.csv", "ParticleSubType", "Particle sub-type data"),
        
        # Test scheduling data
        ("db-seeding/test-schedule.csv", "TestSchedule", "Test schedules"),
        ("db-seeding/test-schedule-rule.csv", "TestScheduleRule", "Test schedule rules"),
        ("db-seeding/test-schedule-test.csv", "TestScheduleTest", "Test schedule tests"),
        
        # Analytical test data
        ("db-seeding/EmSpectro.csv", "EmSpectro", "Emission spectroscopy data"),
        ("db-seeding/Ferrogram.csv", "Ferrogram", "Ferrogram data"),
        ("db-seeding/FTIR.csv", "FTIR", "FTIR data"),
        ("db-seeding/InspectFilter.csv", "InspectFilter", "Filter inspection data"),
        ("db-seeding/LNFData.csv", "LNFData", "LNF data"),
        
        # Test results data
        ("db-seeding/test-readings.csv", "TestReadings", "Test readings"),
        ("db-seeding/AllResults.csv", "AllResults", "All results data"),
        ("db-seeding/ExportTestData.csv", "ExportTestData", "Export test data"),
        
        # Comments and metadata
        ("db-seeding/Comments.csv", "Comments", "Comments data"),
        ("db-seeding/allsamplecomments.csv", "allsamplecomments", "Sample comments"),
        ("db-seeding/testlist.csv", "TestList", "Test list data"),
    ]
    
    # Execute imports
    success_count = 0
    skip_count = 0
    error_count = 0
    
    for csv_file, table_name, description in import_tasks:
        if os.path.exists(csv_file):
            if import_csv_to_table(conn, csv_file, table_name, description):
                success_count += 1
            else:
                error_count += 1
        else:
            print_color(Colors.YELLOW, f"⚠ CSV file not found: {csv_file}")
            skip_count += 1
    
    # Close connection
    conn.close()
    
    # Show summary
    print_header("Import Summary")
    print_color(Colors.GREEN, f"✓ Successful imports: {success_count}")
    print_color(Colors.YELLOW, f"⚠ Skipped (file not found): {skip_count}")
    print_color(Colors.RED, f"✗ Failed imports: {error_count}")
    
    total = success_count + skip_count + error_count
    if total > 0:
        success_rate = (success_count * 100) // total
        print_color(Colors.BLUE, f"Success rate: {success_rate}%")
    
    if error_count == 0:
        print_color(Colors.GREEN, "✓ Database seeding completed successfully!")
    else:
        print_color(Colors.YELLOW, "⚠ Database seeding completed with some errors")
    
    print_color(Colors.CYAN, "Next steps:")
    print_color(Colors.BLUE, "1. Restart your API to ensure it sees the new data")
    print_color(Colors.BLUE, "2. Test the frontend to verify data is loading correctly")
    print_color(Colors.BLUE, "3. Check test selection dropdowns for proper data")

if __name__ == "__main__":
    main()