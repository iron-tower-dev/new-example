#!/usr/bin/env python3
"""
Seed UsedLubeSamples table from CSV file
"""

import csv
import pyodbc
from datetime import datetime

# Database connection string
conn_str = (
    'DRIVER={ODBC Driver 18 for SQL Server};'
    'SERVER=localhost,1433;'
    'DATABASE=LabResultsDb;'
    'UID=sa;'
    'PWD=LabResults123!;'
    'TrustServerCertificate=yes;'
)

# CSV file path
csv_file = 'db-seeding/UsedLubeSamples.csv'

def parse_datetime(date_str):
    """Parse datetime string, return None if NULL or empty"""
    if not date_str or date_str.upper() == 'NULL':
        return None
    try:
        return datetime.strptime(date_str, '%Y-%m-%d %H:%M:%S.%f')
    except:
        try:
            return datetime.strptime(date_str, '%Y-%m-%d %H:%M:%S')
        except:
            return None

def parse_int(value):
    """Parse integer, return None if NULL or empty"""
    if not value or value.upper() == 'NULL':
        return None
    try:
        return int(value)
    except:
        return None

def parse_str(value):
    """Parse string, return None if NULL or empty"""
    if not value or value.upper() == 'NULL':
        return None
    return value

def seed_used_lube_samples():
    """Seed the UsedLubeSamples table from CSV"""
    
    print("Connecting to database...")
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    
    print("Clearing existing data...")
    cursor.execute("DELETE FROM UsedLubeSamples")
    conn.commit()
    
    print(f"Reading CSV file: {csv_file}")
    with open(csv_file, 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        
        insert_sql = """
        INSERT INTO UsedLubeSamples (
            ID, tagNumber, component, location, lubeType, woNumber, trackingNumber,
            warehouseId, batchNumber, classItem, sampleDate, receivedOn, sampledBy,
            status, cmptSelectFlag, newUsedFlag, entryId, validateId, testPricesId,
            pricingPackageId, evaluation, siteId, results_review_date, results_avail_date,
            results_reviewId, storeSource, schedule, returnedDate
        ) VALUES (
            ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?
        )
        """
        
        count = 0
        batch = []
        batch_size = 100
        
        for row in reader:
            values = (
                parse_int(row['ID']),
                parse_str(row['tagNumber']),
                parse_str(row['component']),
                parse_str(row['location']),
                parse_str(row['lubeType']),
                parse_str(row['woNumber']),
                parse_str(row['trackingNumber']),
                parse_str(row['warehouseId']),
                parse_str(row['batchNumber']),
                parse_str(row['classItem']),
                parse_datetime(row['sampleDate']),
                parse_datetime(row['receivedOn']),
                parse_str(row['sampledBy']),
                parse_int(row['status']),
                parse_int(row['cmptSelectFlag']),
                parse_int(row['newUsedFlag']),
                parse_str(row['entryId']),
                parse_str(row['validateId']),
                parse_int(row['testPricesId']),
                parse_int(row['pricingPackageId']),
                parse_int(row['evaluation']),
                parse_int(row['siteId']),
                parse_datetime(row['results_review_date']),
                parse_datetime(row['results_avail_date']),
                parse_str(row['results_reviewId']),
                parse_str(row['storeSource']),
                parse_str(row['schedule']),
                parse_datetime(row['returnedDate'])
            )
            
            batch.append(values)
            count += 1
            
            if len(batch) >= batch_size:
                cursor.executemany(insert_sql, batch)
                conn.commit()
                print(f"Inserted {count} records...")
                batch = []
        
        # Insert remaining records
        if batch:
            cursor.executemany(insert_sql, batch)
            conn.commit()
        
        print(f"\nTotal records inserted: {count}")
    
    # Verify the data
    print("\nVerifying data...")
    cursor.execute("""
        SELECT 
            COUNT(*) as TotalRecords,
            MIN(sampleDate) as EarliestSample,
            MAX(sampleDate) as LatestSample,
            COUNT(DISTINCT tagNumber) as UniqueTagNumbers
        FROM UsedLubeSamples
    """)
    
    result = cursor.fetchone()
    print(f"Total Records: {result[0]}")
    print(f"Earliest Sample: {result[1]}")
    print(f"Latest Sample: {result[2]}")
    print(f"Unique Tag Numbers: {result[3]}")
    
    # Show sample records
    print("\nSample records:")
    cursor.execute("""
        SELECT TOP 5 
            ID, tagNumber, component, location, lubeType, sampleDate, status
        FROM UsedLubeSamples
        ORDER BY ID
    """)
    
    for row in cursor.fetchall():
        print(f"  ID: {row[0]}, Tag: {row[1]}, Component: {row[2]}, Location: {row[3]}, Type: {row[4]}")
    
    cursor.close()
    conn.close()
    
    print("\n✅ UsedLubeSamples seeding complete!")

if __name__ == '__main__':
    try:
        seed_used_lube_samples()
    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()
