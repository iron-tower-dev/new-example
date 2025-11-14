#!/bin/bash

# =============================================
# Seed Test Table Script
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

# Database connection parameters
SERVER="localhost"
DATABASE="LabResultsDb"
USERNAME="sa"
PASSWORD="LabResults123!"

print_color $CYAN "Seeding Test table with correct data..."

# Check if CSV file exists
if [[ ! -f "db-seeding/test.csv" ]]; then
    print_color $RED "✗ CSV file not found: db-seeding/test.csv"
    exit 1
fi

# Test connection
if ! sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT 1" > /dev/null 2>&1; then
    print_color $RED "✗ Database connection failed"
    exit 1
fi

print_color $GREEN "✓ Database connection successful"

# Check current Test table count
current_count=$(sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT COUNT(*) FROM [Test]" -h -1 2>/dev/null | tr -d ' \n\r' | grep -o '[0-9]*' | head -1)

print_color $BLUE "Current Test table has $current_count records"

# Clear existing test data and reseed
print_color $YELLOW "Clearing existing Test data and reseeding..."

# Create SQL file to clear and reseed
cat > /tmp/seed_test.sql << 'EOF'
SET NOCOUNT ON;
BEGIN TRANSACTION;

-- Clear existing test data
DELETE FROM [Test];

-- Reset identity if needed
DBCC CHECKIDENT ('Test', RESEED, 0);

-- Insert correct test data
SET IDENTITY_INSERT [Test] ON;

INSERT INTO [Test] ([TestId], [TestName], [TestDescription], [Active]) VALUES
(10, 'TAN by Color Indication', 'Total Acid Number test using color indication method', 1),
(20, 'Water - KF', 'Water content determination by Karl Fischer method', 1),
(30, 'Emission Spectroscopy - Standard', 'Standard elemental analysis by emission spectroscopy', 1),
(40, 'Emission Spectroscopy - Large', 'Large sample elemental analysis by emission spectroscopy', 1),
(50, 'Viscosity @ 40°C', 'Kinematic viscosity measurement at 40 degrees Celsius', 1),
(60, 'Viscosity @ 100°C', 'Kinematic viscosity measurement at 100 degrees Celsius', 1),
(70, 'FT-IR', 'Fourier Transform Infrared Spectroscopy', 1),
(80, 'Flash Point', 'Flash point determination', 1),
(110, 'TBN by Auto Titration', 'Total Base Number by automatic titration', 1),
(120, 'Inspect Filter', 'Visual inspection of filter elements', 1),
(130, 'Grease Penetration Worked', 'Worked penetration test for grease consistency', 1),
(140, 'Grease Dropping Point', 'Temperature at which grease drops from test apparatus', 1),
(160, 'Particle Count', 'Particle contamination analysis', 1),
(170, 'RBOT', 'Rotating Bomb Oxidation Test', 1),
(180, 'Filter Residue', 'Analysis of filter residue content', 1),
(210, 'Ferrography', 'Microscopic analysis of wear particles', 1),
(220, 'Rust', 'Rust and corrosion analysis', 1),
(230, 'TFOUT', 'Thin Film Oxygen Uptake Test', 1),
(240, 'Debris Identification', 'Identification and classification of debris particles', 1),
(250, 'Deleterious', 'Analysis of deleterious particles', 1),
(270, 'Rheometer', 'Rheological properties measurement', 1),
(284, 'D-inch', 'D-inch particle analysis', 1),
(285, 'Oil Content', 'Oil content determination', 1),
(286, 'Varnish Potential Rating', 'Varnish potential assessment', 1);

SET IDENTITY_INSERT [Test] OFF;

COMMIT TRANSACTION;
PRINT 'Test table seeded successfully';
EOF

# Execute the SQL
result=$(sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -i "/tmp/seed_test.sql" 2>&1)

if echo "$result" | grep -q "Test table seeded successfully"; then
    new_count=$(sqlcmd -S "$SERVER" -d "$DATABASE" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT COUNT(*) FROM [Test]" -h -1 2>/dev/null | tr -d ' \n\r' | grep -o '[0-9]*' | head -1)
    print_color $GREEN "✓ Test table seeded successfully with $new_count records"
    rm -f /tmp/seed_test.sql
else
    print_color $RED "✗ Failed to seed Test table"
    echo "$result"
    rm -f /tmp/seed_test.sql
    exit 1
fi

print_color $CYAN "Test table seeding completed!"
print_color $BLUE "You can now restart your API to see the correct test data."