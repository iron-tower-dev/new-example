#!/usr/bin/env python3
"""
Generate SQL INSERT statements from UsedLubeSamples.csv
"""
import csv
from datetime import datetime

CSV_FILE = "db-seeding/UsedLubeSamples.csv"
OUTPUT_FILE = "LabResultsApi/Scripts/UsedLubeSamplesInserts.sql"

def format_value(value, field_name):
    """Format a value for SQL insertion"""
    if value == 'NULL' or value == '' or value is None:
        return 'NULL'
    
    # Numeric fields that should not be quoted
    numeric_fields = ['ID', 'status', 'cmptSelectFlag', 'newUsedFlag', 
                     'testPricesId', 'pricingPackageId', 'evaluation', 'siteId']
    
    if field_name in numeric_fields:
        return value
    
    # String fields - escape single quotes and wrap in quotes
    value = value.replace("'", "''")
    return f"'{value}'"

def main():
    with open(CSV_FILE, 'r', encoding='utf-8-sig') as csvfile:  # utf-8-sig handles BOM
        reader = csv.DictReader(csvfile)
        
        with open(OUTPUT_FILE, 'w', encoding='utf-8') as outfile:
            # Write header
            outfile.write("-- Generated INSERT statements for UsedLubeSamples\n")
            outfile.write(f"-- Generated from: {CSV_FILE}\n")
            outfile.write(f"-- Generated on: {datetime.now()}\n")
            outfile.write("\n")
            outfile.write("USE [LabResultsDb]\n")
            outfile.write("GO\n")
            outfile.write("\n")
            outfile.write("-- Clear existing data\n")
            outfile.write("DELETE FROM UsedLubeSamples;\n")
            outfile.write("GO\n")
            outfile.write("\n")
            outfile.write("-- Insert data\n")
            
            count = 0
            for row in reader:
                # Build the INSERT statement
                columns = list(row.keys())
                values = [format_value(row[col], col) for col in columns]
                
                columns_str = ', '.join(columns)
                values_str = ', '.join(values)
                
                outfile.write(f"INSERT INTO UsedLubeSamples ({columns_str}) VALUES ({values_str});\n")
                
                count += 1
                if count % 100 == 0:
                    outfile.write("GO\n")
                    print(f"Processed {count} records...")
            
            outfile.write("GO\n")
            outfile.write("\n")
            outfile.write("-- Verify data\n")
            outfile.write("SELECT COUNT(*) as TotalRecords FROM UsedLubeSamples;\n")
            outfile.write("GO\n")
    
    print(f"Generated {OUTPUT_FILE} with {count} INSERT statements")
    print(f"Run with: sqlcmd -S localhost,1433 -U sa -P 'LabResults123!' -d LabResultsDb -i {OUTPUT_FILE}")

if __name__ == "__main__":
    main()
