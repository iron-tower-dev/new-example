#!/bin/bash

# =============================================
# SQL Server Connection Test Script
# Tests connection before running database update
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

# Function to show usage
show_usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -s, --server SERVER     SQL Server instance name (default: localhost)"
    echo "  -d, --database DATABASE Database name (default: lubelab_dev)"
    echo "  -u, --username USERNAME SQL Server username"
    echo "  -p, --password PASSWORD SQL Server password"
    echo "  -h, --help              Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 -u sa -p mypassword"
    echo "  $0 -s myserver -d mydb -u myuser -p mypass"
}

# Default values
SERVER_NAME="localhost"
DATABASE_NAME="LabResultsDb"  # Updated to match API configuration
USERNAME="sa"  # Default from API configuration
PASSWORD="LabResults123!"  # Default from API configuration

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -s|--server)
            SERVER_NAME="$2"
            shift 2
            ;;
        -d|--database)
            DATABASE_NAME="$2"
            shift 2
            ;;
        -u|--username)
            USERNAME="$2"
            shift 2
            ;;
        -p|--password)
            PASSWORD="$2"
            shift 2
            ;;
        -h|--help)
            show_usage
            exit 0
            ;;
        *)
            print_color $RED "Unknown option: $1"
            show_usage
            exit 1
            ;;
    esac
done

print_color $CYAN "=============================================================="
print_color $CYAN "SQL Server Connection Test"
print_color $CYAN "=============================================================="
print_color $BLUE "Server: $SERVER_NAME"
print_color $BLUE "Database: $DATABASE_NAME"
print_color $BLUE "Username: $USERNAME"
echo

# Check if username and password are provided
if [[ -z "$USERNAME" || -z "$PASSWORD" ]]; then
    print_color $RED "✗ Username and password are required for Linux"
    print_color $YELLOW "Windows Authentication is not supported on Linux systems"
    echo
    show_usage
    exit 1
fi

# Test basic connection
print_color $YELLOW "Testing basic connection..."
if sqlcmd -S "$SERVER_NAME" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT @@VERSION" > /dev/null 2>&1; then
    print_color $GREEN "✓ Basic connection successful"
else
    print_color $RED "✗ Basic connection failed"
    print_color $YELLOW "Common issues:"
    print_color $YELLOW "  - SQL Server is not running"
    print_color $YELLOW "  - Incorrect server name/port"
    print_color $YELLOW "  - Firewall blocking connection"
    print_color $YELLOW "  - Invalid credentials"
    print_color $YELLOW "  - SQL Server not configured for SQL Authentication"
    exit 1
fi

# Test database access
print_color $YELLOW "Testing database access..."
if sqlcmd -S "$SERVER_NAME" -d "$DATABASE_NAME" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT DB_NAME()" > /dev/null 2>&1; then
    print_color $GREEN "✓ Database access successful"
else
    print_color $RED "✗ Database access failed"
    print_color $YELLOW "The database '$DATABASE_NAME' may not exist or you don't have access"
    
    # Try to list available databases
    print_color $YELLOW "Available databases:"
    sqlcmd -S "$SERVER_NAME" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT name FROM sys.databases WHERE database_id > 4" -h -1 2>/dev/null | grep -v "^$" | head -10
    exit 1
fi

# Test permissions
print_color $YELLOW "Testing database permissions..."
if sqlcmd -S "$SERVER_NAME" -d "$DATABASE_NAME" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT HAS_PERMS_BY_NAME(DB_NAME(), 'DATABASE', 'CREATE TABLE')" -h -1 2>/dev/null | grep -q "1"; then
    print_color $GREEN "✓ CREATE TABLE permission confirmed"
else
    print_color $YELLOW "⚠ CREATE TABLE permission may be limited"
    print_color $YELLOW "You may need db_ddladmin or db_owner permissions"
fi

# Show current database info
print_color $YELLOW "Getting database information..."
DB_INFO=$(sqlcmd -S "$SERVER_NAME" -d "$DATABASE_NAME" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT DB_NAME() as DatabaseName, USER_NAME() as CurrentUser, @@SERVERNAME as ServerName" -h -1 2>/dev/null)
if [[ $? -eq 0 ]]; then
    print_color $GREEN "✓ Database information:"
    echo "$DB_INFO" | grep -v "^$"
fi

# Count existing tables
print_color $YELLOW "Checking existing tables..."
TABLE_COUNT=$(sqlcmd -S "$SERVER_NAME" -d "$DATABASE_NAME" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'" -h -1 2>/dev/null | grep -o "[0-9]*" | head -1)
if [[ -n "$TABLE_COUNT" ]]; then
    print_color $GREEN "✓ Current table count: $TABLE_COUNT"
    if [[ $TABLE_COUNT -gt 0 ]]; then
        print_color $YELLOW "⚠ Database already contains tables. The update script will modify the structure."
    fi
else
    print_color $YELLOW "⚠ Could not determine table count"
fi

echo
print_color $GREEN "✓ Connection test completed successfully!"
print_color $BLUE "You can now run the database update with these credentials:"
print_color $BLUE "  ./update-database.sh -s '$SERVER_NAME' -d '$DATABASE_NAME' -u '$USERNAME' -p '$PASSWORD'"
print_color $BLUE "  ./update-database.fish -s '$SERVER_NAME' -d '$DATABASE_NAME' -u '$USERNAME' -p '$PASSWORD'"