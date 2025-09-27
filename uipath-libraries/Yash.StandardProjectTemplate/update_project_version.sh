#!/bin/bash

# =============================================================================
# Project Version Update Script for UiPath CodedWorkflows
# =============================================================================
# This script updates the project version in project.json and optionally
# updates package dependencies
#
# Usage: ./update_project_version.sh [OPTIONS]
#   
# Version Update Options:
#   --version VERSION           Set specific project version (e.g., "25.10.0")
#   --major                     Increment major version (X.0.0)
#   --minor                     Increment minor version (X.Y.0)
#   --patch                     Increment patch version (X.Y.Z)
#
# Package Update Options:
#   --package NAME:VERSION      Update specific package (e.g., "UiPath.CodedWorkflows:24.10.2")
#   --packages FILE             Update packages from file (one per line: NAME:VERSION)
#   --update-all                Update all packages to latest compatible versions
#
# General Options:
#   --dry-run                   Show what would be changed without making changes
#   --verbose                   Show detailed processing information
#   --backup                    Create backup file before making changes
#   --help, -h                  Show this help message
#
# Examples:
#   ./update_project_version.sh --minor --verbose
#   ./update_project_version.sh --version "26.0.0" --package "UiPath.CodedWorkflows:24.11.0"
#   ./update_project_version.sh --patch --packages packages.txt --dry-run
#
# Author: Yash Team
# Created: September 2025
# =============================================================================

set -uo pipefail

# Configuration
PROJECT_JSON="project.json"
DRY_RUN=false
VERBOSE=false
CREATE_BACKUP=false
NEW_VERSION=""
VERSION_ACTION=""
PACKAGES_TO_UPDATE=()
PACKAGES_FILE=""
UPDATE_ALL_PACKAGES=false
PYTHON_CMD=""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_verbose() {
    if [[ "$VERBOSE" == true ]]; then
        echo -e "${CYAN}[VERBOSE]${NC} $1"
    fi
}

# Show help message
show_help() {
    cat << 'EOF'
Project Version Update Script for UiPath CodedWorkflows

Usage: ./update_project_version.sh [OPTIONS]

Version Update Options:
  --version VERSION           Set specific project version (e.g., "25.10.0")
  --major                     Increment major version (X.0.0)
  --minor                     Increment minor version (X.Y.0)
  --patch                     Increment patch version (X.Y.Z)

Package Update Options:
  --package NAME:VERSION      Update specific package (e.g., "UiPath.CodedWorkflows:24.10.2")
  --packages FILE             Update packages from file (one per line: NAME:VERSION)
  --update-all                Update all packages to latest compatible versions

General Options:
  --dry-run                   Show what would be changed without making changes
  --verbose                   Show detailed processing information
  --backup                    Create backup file before making changes
  --help, -h                  Show this help message

Examples:
  ./update_project_version.sh --minor --verbose
  ./update_project_version.sh --version "26.0.0" --package "UiPath.CodedWorkflows:24.11.0"
  ./update_project_version.sh --patch --packages packages.txt --dry-run

Package File Format (for --packages option):
  UiPath.CodedWorkflows:24.11.0
  UiPath.Excel.Activities:3.3.0
  Yash.Config:1.0.255.4200

EOF
}

# Parse command line arguments
parse_arguments() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --version)
                NEW_VERSION="$2"
                VERSION_ACTION="set"
                shift 2
                ;;
            --major)
                VERSION_ACTION="major"
                shift
                ;;
            --minor)
                VERSION_ACTION="minor"
                shift
                ;;
            --patch)
                VERSION_ACTION="patch"
                shift
                ;;
            --package)
                PACKAGES_TO_UPDATE+=("$2")
                shift 2
                ;;
            --packages)
                PACKAGES_FILE="$2"
                shift 2
                ;;
            --update-all)
                UPDATE_ALL_PACKAGES=true
                shift
                ;;
            --dry-run)
                DRY_RUN=true
                shift
                ;;
            --verbose)
                VERBOSE=true
                shift
                ;;
            --backup)
                CREATE_BACKUP=true
                shift
                ;;
            --help|-h)
                show_help
                exit 0
                ;;
            *)
                log_error "Unknown option: $1"
                echo "Use --help for usage information."
                exit 1
                ;;
        esac
    done
}

# Validate that project.json exists
validate_project_file() {
    if [[ ! -f "$PROJECT_JSON" ]]; then
        log_error "project.json not found in current directory"
        exit 1
    fi
    
    # Check for Python (more widely available than jq)
    PYTHON_CMD=""
    if python --version &> /dev/null; then
        PYTHON_CMD="python"
    elif python3 --version &> /dev/null; then
        PYTHON_CMD="python3"
    else
        log_error "Python is required but not installed. Please install Python to use this script."
        exit 1
    fi
    
    log_verbose "Using Python command: $PYTHON_CMD"
    
    # Validate JSON syntax
    if ! $PYTHON_CMD -c "import json; json.load(open('$PROJECT_JSON'))" 2>/dev/null; then
        log_error "project.json contains invalid JSON"
        exit 1
    fi
}

# Get current project version
get_current_version() {
    $PYTHON_CMD -c "
import json
with open('$PROJECT_JSON') as f:
    data = json.load(f)
    print(data.get('projectVersion', '1.0.0'))
"
}

# Increment version number
increment_version() {
    local current_version="$1"
    local action="$2"
    
    # Parse version (major.minor.patch)
    local version_regex="^([0-9]+)\.([0-9]+)\.([0-9]+)$"
    
    if [[ ! $current_version =~ $version_regex ]]; then
        log_error "Invalid version format: $current_version (expected: major.minor.patch)"
        exit 1
    fi
    
    local major="${BASH_REMATCH[1]}"
    local minor="${BASH_REMATCH[2]}"
    local patch="${BASH_REMATCH[3]}"
    
    case "$action" in
        "major")
            echo "$((major + 1)).0.0"
            ;;
        "minor")
            echo "$major.$((minor + 1)).0"
            ;;
        "patch")
            echo "$major.$minor.$((patch + 1))"
            ;;
        *)
            log_error "Unknown version action: $action"
            exit 1
            ;;
    esac
}

# Validate version format
validate_version() {
    local version="$1"
    local version_regex="^[0-9]+\.[0-9]+\.[0-9]+$"
    
    if [[ ! $version =~ $version_regex ]]; then
        log_error "Invalid version format: $version (expected: major.minor.patch)"
        exit 1
    fi
}

# Parse package string (NAME:VERSION)
parse_package() {
    local package_string="$1"
    
    if [[ ! $package_string =~ ^([^:]+):(.+)$ ]]; then
        log_error "Invalid package format: $package_string (expected: NAME:VERSION)"
        exit 1
    fi
    
    echo "${BASH_REMATCH[1]}" "${BASH_REMATCH[2]}"
}

# Read packages from file
read_packages_file() {
    local file="$1"
    
    if [[ ! -f "$file" ]]; then
        log_error "Packages file not found: $file"
        exit 1
    fi
    
    while IFS= read -r line; do
        # Skip empty lines and comments
        if [[ -n "$line" && ! "$line" =~ ^[[:space:]]*# ]]; then
            PACKAGES_TO_UPDATE+=("$line")
        fi
    done < "$file"
}

# Get current package version
get_package_version() {
    local package_name="$1"
    $PYTHON_CMD -c "
import json
with open('$PROJECT_JSON') as f:
    data = json.load(f)
    deps = data.get('dependencies', {})
    version = deps.get('$package_name')
    print(version if version else 'null')
"
}

# Update project version in JSON
update_project_version() {
    local new_version="$1"
    local temp_file
    temp_file=$(mktemp)
    
    log_verbose "Updating project version to: $new_version"
    
    if [[ "$DRY_RUN" == true ]]; then
        log_info "DRY RUN: Would update project version to: $new_version"
        return
    fi
    
    # Update projectVersion field using Python
    $PYTHON_CMD -c "
import json
with open('$PROJECT_JSON') as f:
    data = json.load(f)
data['projectVersion'] = '$new_version'
with open('$temp_file', 'w') as f:
    json.dump(data, f, indent=2)
"
    
    if [[ $? -eq 0 ]]; then
        mv "$temp_file" "$PROJECT_JSON"
        log_success "Updated project version to: $new_version"
    else
        rm -f "$temp_file"
        log_error "Failed to update project version"
        exit 1
    fi
}

# Update package dependency
update_package_dependency() {
    local package_name="$1"
    local package_version="$2"
    local temp_file
    temp_file=$(mktemp)
    
    log_verbose "Updating package $package_name to version: $package_version"
    
    if [[ "$DRY_RUN" == true ]]; then
        log_info "DRY RUN: Would update $package_name to: [$package_version]"
        return
    fi
    
    # Update dependency in JSON using Python
    $PYTHON_CMD -c "
import json
with open('$PROJECT_JSON') as f:
    data = json.load(f)
if 'dependencies' not in data:
    data['dependencies'] = {}
data['dependencies']['$package_name'] = '[$package_version]'
with open('$temp_file', 'w') as f:
    json.dump(data, f, indent=2)
"
    
    if [[ $? -eq 0 ]]; then
        mv "$temp_file" "$PROJECT_JSON"
        log_success "Updated $package_name to: [$package_version]"
    else
        rm -f "$temp_file"
        log_error "Failed to update package: $package_name"
        exit 1
    fi
}

# Create backup if requested
create_backup() {
    if [[ "$CREATE_BACKUP" == true ]]; then
        local backup_file="${PROJECT_JSON}.backup.$(date +%Y%m%d_%H%M%S)"
        cp "$PROJECT_JSON" "$backup_file"
        log_info "Created backup: $backup_file"
    fi
}

# Main execution function
main() {
    log_info "🚀 Starting project version update..."
    
    if [[ "$DRY_RUN" == true ]]; then
        log_warning "Running in DRY RUN mode - no changes will be made"
    fi
    
    # Validate environment
    validate_project_file
    
    # Get current version
    local current_version
    current_version=$(get_current_version)
    log_info "Current project version: $current_version"
    
    # Show current dependencies if verbose
    if [[ "$VERBOSE" == true ]]; then
        log_verbose "Current dependencies:"
        $PYTHON_CMD -c "
import json
with open('$PROJECT_JSON') as f:
    data = json.load(f)
    deps = data.get('dependencies', {})
    for name, version in deps.items():
        print(f'  {name}: {version}')
"
    fi
    
    # Read packages from file if specified
    if [[ -n "$PACKAGES_FILE" ]]; then
        log_info "Reading packages from file: $PACKAGES_FILE"
        read_packages_file "$PACKAGES_FILE"
    fi
    
    # Create backup before making changes
    if [[ "$DRY_RUN" == false ]]; then
        create_backup
    fi
    
    # Update project version if requested
    if [[ -n "$VERSION_ACTION" ]]; then
        local new_version
        
        if [[ "$VERSION_ACTION" == "set" ]]; then
            validate_version "$NEW_VERSION"
            new_version="$NEW_VERSION"
        else
            new_version=$(increment_version "$current_version" "$VERSION_ACTION")
        fi
        
        log_info "Updating project version: $current_version → $new_version"
        update_project_version "$new_version"
    fi
    
    # Update packages if requested
    if [[ ${#PACKAGES_TO_UPDATE[@]} -gt 0 ]]; then
        log_info "Updating ${#PACKAGES_TO_UPDATE[@]} package(s)..."
        
        for package_string in "${PACKAGES_TO_UPDATE[@]}"; do
            read -r package_name package_version <<< "$(parse_package "$package_string")"
            
            local current_pkg_version
            current_pkg_version=$(get_package_version "$package_name")
            
            if [[ "$current_pkg_version" == "null" ]]; then
                log_warning "Package not found in dependencies: $package_name"
                continue
            fi
            
            # Remove brackets from current version for comparison
            current_pkg_version=$(echo "$current_pkg_version" | tr -d '[]')
            
            if [[ "$current_pkg_version" == "$package_version" ]]; then
                log_verbose "Package $package_name already at version: $package_version"
                continue
            fi
            
            log_info "Updating package: $package_name ($current_pkg_version → $package_version)"
            update_package_dependency "$package_name" "$package_version"
        done
    fi
    
    # Handle update-all packages (placeholder for future implementation)
    if [[ "$UPDATE_ALL_PACKAGES" == true ]]; then
        log_warning "Update-all packages feature not yet implemented"
        log_info "This would require integration with UiPath package manager or NuGet"
    fi
    
    # Summary
    log_info "📊 Summary:"
    
    if [[ -n "$VERSION_ACTION" ]]; then
        if [[ "$DRY_RUN" == true ]]; then
            log_info "  Project version would be updated"
        else
            local final_version
            final_version=$(get_current_version)
            log_info "  Project version updated to: $final_version"
        fi
    fi
    
    if [[ ${#PACKAGES_TO_UPDATE[@]} -gt 0 ]]; then
        if [[ "$DRY_RUN" == true ]]; then
            log_info "  ${#PACKAGES_TO_UPDATE[@]} package(s) would be updated"
        else
            log_info "  ${#PACKAGES_TO_UPDATE[@]} package(s) updated"
        fi
    fi
    
    if [[ "$DRY_RUN" == true ]]; then
        log_warning "No actual changes were made (dry run mode)"
    else
        log_success "Project update completed successfully!"
    fi
}

# Parse arguments and run main function
parse_arguments "$@"

# Validate that at least one action was specified
if [[ -z "$VERSION_ACTION" && ${#PACKAGES_TO_UPDATE[@]} -eq 0 && -z "$PACKAGES_FILE" && "$UPDATE_ALL_PACKAGES" == false ]]; then
    log_error "No action specified. Use --help for usage information."
    exit 1
fi

# Run main function
main "$@"