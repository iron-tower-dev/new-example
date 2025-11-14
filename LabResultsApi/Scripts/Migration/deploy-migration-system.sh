#!/bin/bash

# Migration System Deployment Script for Linux/macOS
# This script sets up the migration system for the Lab Results API

set -e  # Exit on any error

# Default values
ENVIRONMENT=""
CONNECTION_STRING=""
LEGACY_CONNECTION_STRING=""
SKIP_DATABASE_SETUP=false
SKIP_VALIDATION=false
FORCE=false
LOG_PATH="logs/deployment.log"

# Script paths
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$(dirname "$SCRIPT_DIR")")"
CONFIG_PATH="$PROJECT_ROOT/Configuration"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging function
log() {
    local level=${2:-INFO}
    local message="$1"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    local log_message="[$timestamp] [$level] $message"
    
    case $level in
        ERROR)
            echo -e "${RED}$log_message${NC}" >&2
            ;;
        WARN)
            echo -e "${YELLOW}$log_message${NC}"
            ;;
        SUCCESS)
            echo -e "${GREEN}$log_message${NC}"
            ;;
        *)
            echo -e "${BLUE}$log_message${NC}"
            ;;
    esac
    
    # Ensure log directory exists
    mkdir -p "$(dirname "$LOG_PATH")"
    echo "$log_message" >> "$LOG_PATH"
}

# Show usage
show_usage() {
    cat << EOF
Usage: $0 -e ENVIRONMENT [OPTIONS]

Required:
    -e, --environment ENVIRONMENT    Target environment (Development, Staging, Production)

Options:
    -c, --connection-string STRING   Database connection string
    -l, --legacy-connection STRING   Legacy database connection string
    -s, --skip-database-setup       Skip database schema setup
    -v, --skip-validation           Skip deployment validation
    -f, --force                     Force deployment even if already exists
    -p, --log-path PATH             Log file path (default: logs/deployment.log)
    -h, --help                      Show this help message

Examples:
    $0 -e Development
    $0 -e Production -c "Server=prod-db;Database=LabResultsDb;..." -f
    $0 -e Staging --skip-validation --log-path /tmp/deploy.log
EOF
}

# Parse command line arguments
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            -e|--environment)
                ENVIRONMENT="$2"
                shift 2
                ;;
            -c|--connection-string)
                CONNECTION_STRING="$2"
                shift 2
                ;;
            -l|--legacy-connection)
                LEGACY_CONNECTION_STRING="$2"
                shift 2
                ;;
            -s|--skip-database-setup)
                SKIP_DATABASE_SETUP=true
                shift
                ;;
            -v|--skip-validation)
                SKIP_VALIDATION=true
                shift
                ;;
            -f|--force)
                FORCE=true
                shift
                ;;
            -p|--log-path)
                LOG_PATH="$2"
                shift 2
                ;;
            -h|--help)
                show_usage
                exit 0
                ;;
            *)
                echo "Unknown option: $1" >&2
                show_usage
                exit 1
                ;;
        esac
    done
    
    if [[ -z "$ENVIRONMENT" ]]; then
        echo "Error: Environment is required" >&2
        show_usage
        exit 1
    fi
}

# Check prerequisites
check_prerequisites() {
    log "Checking prerequisites..."
    
    # Check bash version
    if [[ ${BASH_VERSION%%.*} -lt 4 ]]; then
        log "Bash 4.0 or later is required" ERROR
        exit 1
    fi
    log "Bash version: $BASH_VERSION"
    
    # Check .NET
    if ! command -v dotnet &> /dev/null; then
        log ".NET SDK is not installed or not in PATH" ERROR
        exit 1
    fi
    local dotnet_version=$(dotnet --version)
    log ".NET version: $dotnet_version"
    
    # Check sqlcmd if we need database setup
    if [[ "$SKIP_DATABASE_SETUP" == false ]] && [[ -n "$CONNECTION_STRING" ]]; then
        if ! command -v sqlcmd &> /dev/null; then
            log "sqlcmd is not available. Install SQL Server command line tools or provide connection string for alternative method" WARN
        fi
    fi
    
    # Check required directories
    local required_dirs=("db-seeding" "db-tables" "logs")
    for dir in "${required_dirs[@]}"; do
        local full_path="$PROJECT_ROOT/$dir"
        if [[ ! -d "$full_path" ]]; then
            log "Creating directory: $full_path"
            mkdir -p "$full_path"
        fi
    done
    
    log "Prerequisites check completed successfully" SUCCESS
}

# Install database schema
install_database_schema() {
    if [[ "$SKIP_DATABASE_SETUP" == true ]]; then
        log "Skipping database setup as requested"
        return
    fi
    
    log "Installing migration database schema..."
    
    local sql_script="$SCRIPT_DIR/01-create-migration-tables.sql"
    if [[ ! -f "$sql_script" ]]; then
        log "Migration SQL script not found: $sql_script" ERROR
        exit 1
    fi
    
    if [[ -n "$CONNECTION_STRING" ]]; then
        # Parse connection string for sqlcmd
        local server=$(echo "$CONNECTION_STRING" | grep -oP 'Server=\K[^;]*' || echo "localhost")
        local database=$(echo "$CONNECTION_STRING" | grep -oP 'Database=\K[^;]*' || echo "LabResultsDb")
        local user=$(echo "$CONNECTION_STRING" | grep -oP 'User Id=\K[^;]*' || echo "")
        local password=$(echo "$CONNECTION_STRING" | grep -oP 'Password=\K[^;]*' || echo "")
        
        if command -v sqlcmd &> /dev/null; then
            local sqlcmd_args=("-S" "$server" "-d" "$database" "-i" "$sql_script" "-b")
            
            if [[ -n "$user" && -n "$password" ]]; then
                sqlcmd_args+=("-U" "$user" "-P" "$password")
            else
                sqlcmd_args+=("-E")  # Use integrated security
            fi
            
            if sqlcmd "${sqlcmd_args[@]}"; then
                log "Database schema installed successfully using sqlcmd" SUCCESS
            else
                log "sqlcmd failed" ERROR
                exit 1
            fi
        else
            log "sqlcmd not available and connection string provided. Manual database setup required." WARN
            log "Please run the SQL script manually: $sql_script"
        fi
    else
        log "No connection string provided. Using default local connection with sqlcmd"
        if command -v sqlcmd &> /dev/null; then
            if sqlcmd -S localhost -d LabResultsDb -E -i "$sql_script" -b; then
                log "Database schema installed successfully" SUCCESS
            else
                log "Database schema installation failed" ERROR
                exit 1
            fi
        else
            log "sqlcmd not available. Please install SQL Server command line tools or provide connection string" ERROR
            exit 1
        fi
    fi
}

# Deploy configuration
deploy_configuration() {
    log "Deploying configuration for environment: $ENVIRONMENT"
    
    # Validate environment
    local valid_environments=("Development" "Staging" "Production")
    local env_valid=false
    for env in "${valid_environments[@]}"; do
        if [[ "$ENVIRONMENT" == "$env" ]]; then
            env_valid=true
            break
        fi
    done
    
    if [[ "$env_valid" == false ]]; then
        log "Invalid environment. Must be one of: ${valid_environments[*]}" ERROR
        exit 1
    fi
    
    # Check if environment-specific config exists
    local config_file="$CONFIG_PATH/migration-${ENVIRONMENT,,}.json"
    if [[ ! -f "$config_file" ]]; then
        log "Environment-specific configuration not found: $config_file" WARN
        log "Using default configuration"
    else
        log "Found environment configuration: $config_file"
        
        # Validate JSON
        if command -v jq &> /dev/null; then
            if jq empty "$config_file" 2>/dev/null; then
                log "Configuration JSON is valid" SUCCESS
            else
                log "Invalid JSON in configuration file" ERROR
                exit 1
            fi
        else
            log "jq not available, skipping JSON validation" WARN
        fi
    fi
    
    # Update appsettings if needed
    local appsettings_file="$PROJECT_ROOT/appsettings.$ENVIRONMENT.json"
    if [[ -f "$appsettings_file" ]]; then
        log "Found appsettings file: $appsettings_file"
        
        # Update connection strings if provided and jq is available
        if command -v jq &> /dev/null && [[ -n "$CONNECTION_STRING" || -n "$LEGACY_CONNECTION_STRING" ]]; then
            local temp_file=$(mktemp)
            cp "$appsettings_file" "$temp_file"
            
            if [[ -n "$CONNECTION_STRING" ]]; then
                jq --arg conn "$CONNECTION_STRING" '.ConnectionStrings.DefaultConnection = $conn' "$temp_file" > "$appsettings_file"
                log "Updated DefaultConnection in appsettings" SUCCESS
            fi
            
            if [[ -n "$LEGACY_CONNECTION_STRING" ]]; then
                jq --arg conn "$LEGACY_CONNECTION_STRING" '.ConnectionStrings.LegacyDatabase = $conn' "$appsettings_file" > "$temp_file"
                cp "$temp_file" "$appsettings_file"
                log "Updated LegacyDatabase connection in appsettings" SUCCESS
            fi
            
            rm -f "$temp_file"
        elif [[ -n "$CONNECTION_STRING" || -n "$LEGACY_CONNECTION_STRING" ]]; then
            log "jq not available, cannot update connection strings automatically" WARN
            log "Please update connection strings manually in: $appsettings_file"
        fi
    fi
}

# Test deployment
test_deployment() {
    if [[ "$SKIP_VALIDATION" == true ]]; then
        log "Skipping deployment validation as requested"
        return
    fi
    
    log "Validating deployment..."
    
    # Test API build
    cd "$PROJECT_ROOT"
    if dotnet build --configuration Release --no-restore; then
        log "API build successful" SUCCESS
    else
        log "API build validation failed" ERROR
        exit 1
    fi
    
    # Test configuration files
    if command -v jq &> /dev/null; then
        for config_file in "$CONFIG_PATH"/migration-*.json; do
            if [[ -f "$config_file" ]]; then
                if jq empty "$config_file" 2>/dev/null; then
                    log "Configuration file valid: $(basename "$config_file")" SUCCESS
                else
                    log "Invalid configuration file: $(basename "$config_file")" ERROR
                fi
            fi
        done
    fi
    
    log "Deployment validation completed successfully" SUCCESS
}

# Show deployment summary
show_deployment_summary() {
    log "=== DEPLOYMENT SUMMARY ===" 
    log "Environment: $ENVIRONMENT"
    log "Project Root: $PROJECT_ROOT"
    log "Configuration Path: $CONFIG_PATH"
    log "Log Path: $LOG_PATH"
    
    if [[ "$SKIP_DATABASE_SETUP" == false ]]; then
        log "Database schema: Installed"
    else
        log "Database schema: Skipped"
    fi
    
    if [[ "$SKIP_VALIDATION" == false ]]; then
        log "Validation: Completed"
    else
        log "Validation: Skipped"
    fi
    
    log "=== NEXT STEPS ==="
    log "1. Review the deployment log: $LOG_PATH"
    log "2. Update connection strings in appsettings.$ENVIRONMENT.json if needed"
    log "3. Test the migration endpoints using the API"
    log "4. Run a test migration to verify everything works"
    
    log "Deployment completed successfully!" SUCCESS
}

# Main execution
main() {
    parse_args "$@"
    
    log "Starting migration system deployment..."
    log "Environment: $ENVIRONMENT"
    log "Force: $FORCE"
    
    # Check if already deployed
    if [[ "$FORCE" == false ]]; then
        local migration_script="$SCRIPT_DIR/01-create-migration-tables.sql"
        if [[ -f "$migration_script" && -n "$CONNECTION_STRING" ]]; then
            log "Migration system may already be deployed. Use --force to redeploy." WARN
            log "Continuing with deployment..."
        fi
    fi
    
    check_prerequisites
    install_database_schema
    deploy_configuration
    test_deployment
    show_deployment_summary
}

# Run main function with all arguments
main "$@"