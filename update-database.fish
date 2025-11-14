#!/usr/bin/env fish

# =============================================
# Fish Shell Script to Update Database Structure
# This is a wrapper around the bash script for Fish shell users
# =============================================

# Set colors
set -l RED (set_color red)
set -l GREEN (set_color green)
set -l YELLOW (set_color yellow)
set -l BLUE (set_color blue)
set -l CYAN (set_color cyan)
set -l NORMAL (set_color normal)

# Default configuration
set -l server_name "localhost"
set -l database_name "LabResultsDb"  # Updated to match API configuration
set -l use_windows_auth 0  # Changed to 0 for Linux - Windows Auth doesn't work on Linux
set -l username "sa"  # Default from API configuration
set -l password "LabResults123!"  # Default from API configuration

# Function to show usage
function show_usage
    echo "Usage: $argv[0] [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -s, --server SERVER     SQL Server instance name (default: localhost)"
    echo "  -d, --database DATABASE Database name (default: lubelab_dev)"
    echo "  -u, --username USERNAME SQL Server username (for SQL auth)"
    echo "  -p, --password PASSWORD SQL Server password (for SQL auth)"
    echo "  -w, --windows-auth      Use Windows authentication (default)"
    echo "  -q, --sql-auth          Use SQL Server authentication"
    echo "  -h, --help              Show this help message"
    echo ""
    echo "Examples:"
    echo "  $argv[0]                                          # Use defaults with Windows auth"
    echo "  $argv[0] -s myserver -d mydb                     # Custom server and database"
    echo "  $argv[0] -s myserver -d mydb -q -u sa -p mypass  # SQL Server authentication"
end

# Parse arguments
set -l i 1
while test $i -le (count $argv)
    switch $argv[$i]
        case -s --server
            set i (math $i + 1)
            if test $i -le (count $argv)
                set server_name $argv[$i]
            else
                echo $RED"Error: --server requires a value"$NORMAL
                exit 1
            end
        case -d --database
            set i (math $i + 1)
            if test $i -le (count $argv)
                set database_name $argv[$i]
            else
                echo $RED"Error: --database requires a value"$NORMAL
                exit 1
            end
        case -u --username
            set i (math $i + 1)
            if test $i -le (count $argv)
                set username $argv[$i]
            else
                echo $RED"Error: --username requires a value"$NORMAL
                exit 1
            end
        case -p --password
            set i (math $i + 1)
            if test $i -le (count $argv)
                set password $argv[$i]
            else
                echo $RED"Error: --password requires a value"$NORMAL
                exit 1
            end
        case -w --windows-auth
            set use_windows_auth 1
        case -q --sql-auth
            set use_windows_auth 0
        case -h --help
            show_usage
            exit 0
        case '*'
            echo $RED"Unknown option: $argv[$i]"$NORMAL
            show_usage
            exit 1
    end
    set i (math $i + 1)
end

# Print header
echo
echo $CYAN"=============================================================="$NORMAL
echo $CYAN"Database Structure Update Script (Fish Shell Wrapper)"$NORMAL
echo $CYAN"=============================================================="$NORMAL
echo $BLUE"Server: $server_name"$NORMAL
echo $BLUE"Database: $database_name"$NORMAL
if test $use_windows_auth -eq 1
    echo $BLUE"Authentication: Windows"$NORMAL
else
    echo $BLUE"Authentication: SQL Server"$NORMAL
end
echo

# Check if bash script exists
if not test -f update-database.sh
    echo $RED"Error: update-database.sh not found in current directory"$NORMAL
    exit 1
end

# Check if bash script is executable
if not test -x update-database.sh
    echo $YELLOW"Making update-database.sh executable..."$NORMAL
    chmod +x update-database.sh
end

# Build arguments for bash script
set -l bash_args
set bash_args $bash_args --server $server_name
set bash_args $bash_args --database $database_name

if test $use_windows_auth -eq 1
    set bash_args $bash_args --windows-auth
else
    set bash_args $bash_args --sql-auth
    if test -n "$username"
        set bash_args $bash_args --username $username
    end
    if test -n "$password"
        set bash_args $bash_args --password $password
    end
end

# Execute the bash script
echo $CYAN"Executing bash script with arguments: $bash_args"$NORMAL
echo

./update-database.sh $bash_args

# Capture exit code
set -l exit_code $status

if test $exit_code -eq 0
    echo
    echo $GREEN"✓ Fish wrapper completed successfully!"$NORMAL
else
    echo
    echo $RED"✗ Fish wrapper completed with errors (exit code: $exit_code)"$NORMAL
end

exit $exit_code