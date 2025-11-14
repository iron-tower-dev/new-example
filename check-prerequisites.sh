#!/bin/bash

# =============================================
# Prerequisites Check Script for Linux
# Checks if all required tools are installed
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
print_color $CYAN "Prerequisites Check for Database Update"
print_color $CYAN "=============================================================="
echo

# Check operating system
print_color $BLUE "Operating System Information:"
if command -v lsb_release &> /dev/null; then
    lsb_release -d | sed 's/Description:/  /'
elif [[ -f /etc/os-release ]]; then
    . /etc/os-release
    echo "  $PRETTY_NAME"
elif [[ -f /etc/arch-release ]]; then
    echo "  Arch Linux"
else
    echo "  $(uname -s) $(uname -r)"
fi
echo

# Check for sqlcmd
print_color $BLUE "Checking for sqlcmd..."
if command -v sqlcmd &> /dev/null; then
    print_color $GREEN "✓ sqlcmd is installed"
    sqlcmd_version=$(sqlcmd -? 2>&1 | head -1 | grep -o "Version [0-9.]*" || echo "Version unknown")
    echo "  $sqlcmd_version"
else
    print_color $RED "✗ sqlcmd is not installed"
    echo
    print_color $YELLOW "Installation instructions:"
    
    # Detect package manager and provide instructions
    if command -v pacman &> /dev/null; then
        print_color $YELLOW "  For Arch Linux:"
        print_color $YELLOW "    yay -S mssql-tools"
        print_color $YELLOW "    # or"
        print_color $YELLOW "    sudo pacman -S mssql-tools"
    elif command -v apt-get &> /dev/null; then
        print_color $YELLOW "  For Ubuntu/Debian:"
        print_color $YELLOW "    curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -"
        print_color $YELLOW "    curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list | sudo tee /etc/apt/sources.list.d/msprod.list"
        print_color $YELLOW "    sudo apt-get update"
        print_color $YELLOW "    sudo apt-get install mssql-tools unixodbc-dev"
    elif command -v yum &> /dev/null; then
        print_color $YELLOW "  For RHEL/CentOS/Fedora:"
        print_color $YELLOW "    sudo curl -o /etc/yum.repos.d/msprod.repo https://packages.microsoft.com/config/rhel/8/prod.repo"
        print_color $YELLOW "    sudo yum install mssql-tools unixODBC-devel"
    else
        print_color $YELLOW "  Please visit: https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup-tools"
    fi
fi
echo

# Check for bash
print_color $BLUE "Checking for bash..."
if command -v bash &> /dev/null; then
    print_color $GREEN "✓ bash is available"
    bash_version=$(bash --version | head -1)
    echo "  $bash_version"
else
    print_color $RED "✗ bash is not available"
fi
echo

# Check for fish (if user mentioned they use it)
print_color $BLUE "Checking for fish shell..."
if command -v fish &> /dev/null; then
    print_color $GREEN "✓ fish shell is available"
    fish_version=$(fish --version)
    echo "  $fish_version"
else
    print_color $YELLOW "⚠ fish shell is not available (optional)"
fi
echo

# Check if required directories exist
print_color $BLUE "Checking for required directories..."
required_dirs=("db-tables" "db-functions" "db-sp" "db-views")
all_dirs_exist=true

for dir in "${required_dirs[@]}"; do
    if [[ -d "$dir" ]]; then
        file_count=$(find "$dir" -name "*.sql" | wc -l)
        print_color $GREEN "✓ Directory exists: $dir ($file_count SQL files)"
    else
        print_color $RED "✗ Directory missing: $dir"
        all_dirs_exist=false
    fi
done
echo

# Check if update scripts exist
print_color $BLUE "Checking for update scripts..."
scripts=("update-database.sh" "update-database.fish" "update-database-structure.sql")

for script in "${scripts[@]}"; do
    if [[ -f "$script" ]]; then
        if [[ -x "$script" ]]; then
            print_color $GREEN "✓ Script exists and is executable: $script"
        else
            print_color $YELLOW "⚠ Script exists but is not executable: $script"
            print_color $YELLOW "  Run: chmod +x $script"
        fi
    else
        print_color $RED "✗ Script missing: $script"
    fi
done
echo

# Summary
print_color $CYAN "=============================================================="
print_color $CYAN "Prerequisites Summary"
print_color $CYAN "=============================================================="

if command -v sqlcmd &> /dev/null && [[ "$all_dirs_exist" == true ]]; then
    print_color $GREEN "✓ All prerequisites are met!"
    print_color $GREEN "You can proceed with the database update."
    echo
    print_color $BLUE "To run the update:"
    print_color $BLUE "  ./update-database.sh                    # For bash users"
    print_color $BLUE "  ./update-database.fish                  # For fish users"
else
    print_color $RED "✗ Some prerequisites are missing."
    print_color $YELLOW "Please install the missing components before proceeding."
fi
echo