#!/bin/bash

# =============================================================================
# Namespace Synchronization Script for UiPath CodedWorkflows
# =============================================================================
# This script synchronizes C# namespaces with the directory structure,
# reading the root namespace from project.json
#
# Usage: ./update_namespaces.sh [--dry-run] [--verbose]
#   --dry-run: Show what would be changed without making changes
#   --verbose: Show detailed processing information
#
# Author: Yash Team
# Created: September 2025
# =============================================================================

set -uo pipefail

# Configuration
PROJECT_JSON="project.json"
DRY_RUN=false
VERBOSE=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        --verbose)
            VERBOSE=true
            shift
            ;;
        --help|-h)
            echo "Usage: $0 [--dry-run] [--verbose]"
            echo "  --dry-run: Show what would be changed without making changes"
            echo "  --verbose: Show detailed processing information"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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
        echo -e "${BLUE}[VERBOSE]${NC} $1"
    fi
}

# Function to extract root namespace from project.json
get_root_namespace() {
    if [[ ! -f "$PROJECT_JSON" ]]; then
        log_error "project.json not found in current directory"
        exit 1
    fi
    
    # Extract the name field from project.json
    local root_namespace
    root_namespace=$(grep '"name"' "$PROJECT_JSON" | sed 's/.*"name"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/')
    
    if [[ -z "$root_namespace" ]]; then
        log_error "Could not extract root namespace from $PROJECT_JSON"
        exit 1
    fi
    
    echo "$root_namespace"
}

# Function to convert directory path to namespace component
dir_to_namespace() {
    local dir_path="$1"
    local namespace_part=""
    
    # Skip certain directories that shouldn't have namespaces
    case "$dir_path" in
        .codedworkflows*|.entities*|.local*|.objects*|.project*|.settings*|.templates*|.tmh*)
            return 1
            ;;
    esac
    
    # Split path by '/' and process each component
    IFS='/' read -ra PATH_PARTS <<< "$dir_path"
    
    for part in "${PATH_PARTS[@]}"; do
        if [[ -z "$part" ]]; then
            continue
        fi
        
        # Skip hidden directories
        if [[ "$part" =~ ^\. ]]; then
            return 1
        fi
        
        # Add underscore prefix to parts that start with numbers
        if [[ "$part" =~ ^[0-9] ]]; then
            part="_$part"
        fi
        
        # Append to namespace (dot-separated)
        if [[ -z "$namespace_part" ]]; then
            namespace_part="$part"
        else
            namespace_part="$namespace_part.$part"
        fi
    done
    
    echo "$namespace_part"
    return 0
}

# Function to generate expected namespace for a file
get_expected_namespace() {
    local file_path="$1"
    local root_namespace="$2"
    
    # Get directory path relative to project root
    local dir_path
    dir_path=$(dirname "$file_path")
    
    # Convert to Windows path separators and then to namespace format
    dir_path=$(echo "$dir_path" | tr '\\' '/')
    
    # Skip if in root directory
    if [[ "$dir_path" == "." ]]; then
        echo "$root_namespace"
        return
    fi
    
    # Convert directory path to namespace component
    local namespace_suffix
    if ! namespace_suffix=$(dir_to_namespace "$dir_path"); then
        # Return empty if directory should be skipped
        return 1
    fi
    
    # Combine root namespace with directory-based suffix
    if [[ -z "$namespace_suffix" ]]; then
        echo "$root_namespace"
    else
        echo "$root_namespace.$namespace_suffix"
    fi
}

# Function to extract current namespace from a C# file
get_current_namespace() {
    local file_path="$1"
    
    # Look for namespace declaration
    local current_namespace
    current_namespace=$(grep -E "^[[:space:]]*namespace[[:space:]]+" "$file_path" | head -1 | sed 's/^[[:space:]]*namespace[[:space:]]\+\([^[:space:]]*\).*/\1/')
    
    echo "$current_namespace"
}

# Function to update namespace in a C# file
update_namespace() {
    local file_path="$1"
    local new_namespace="$2"
    
    if [[ "$DRY_RUN" == true ]]; then
        log_info "DRY RUN: Would update namespace in $file_path to: $new_namespace"
        return
    fi
    
    # Create backup
    cp "$file_path" "$file_path.bak"
    
    # Update the namespace line
    sed -i "s/^[[:space:]]*namespace[[:space:]]\+[^[:space:]]*/namespace $new_namespace/" "$file_path"
    
    # Remove backup if successful
    rm "$file_path.bak"
    
    log_success "Updated namespace in $file_path to: $new_namespace"
}

# Function to process a single C# file
process_cs_file() {
    local file_path="$1"
    local root_namespace="$2"
    
    log_verbose "Processing: $file_path"
    
    # Get current and expected namespaces
    local current_namespace
    current_namespace=$(get_current_namespace "$file_path")
    
    local expected_namespace
    if ! expected_namespace=$(get_expected_namespace "$file_path" "$root_namespace"); then
        log_verbose "  Skipping (directory should be ignored)"
        return
    fi
    
    log_verbose "  Current namespace: $current_namespace"
    log_verbose "  Expected namespace: $expected_namespace"
    
    # Check if namespace needs updating
    if [[ "$current_namespace" != "$expected_namespace" ]]; then
        if [[ -z "$current_namespace" ]]; then
            log_warning "No namespace found in $file_path - skipping"
            return
        fi
        
        log_info "Namespace mismatch in $file_path:"
        log_info "  Current:  $current_namespace"
        log_info "  Expected: $expected_namespace"
        
        update_namespace "$file_path" "$expected_namespace"
    else
        log_verbose "  Namespace is correct"
    fi
}

# Main execution function
main() {
    log_info "🚀 Starting namespace synchronization..."
    
    if [[ "$DRY_RUN" == true ]]; then
        log_warning "Running in DRY RUN mode - no changes will be made"
    fi
    
    # Get root namespace from project.json
    local root_namespace
    root_namespace=$(get_root_namespace)
    log_info "Root namespace: $root_namespace"
    
    # Find all C# files
    local cs_files
    mapfile -t cs_files < <(find . -name "*.cs" -type f | grep -v "\.bak$" | grep -v "^\\./\\." | sort)
    
    if [[ ${#cs_files[@]} -eq 0 ]]; then
        log_warning "No C# files found"
        exit 0
    fi
    
    log_info "Found ${#cs_files[@]} C# files to process"
    
    # Process each file
    local files_processed=0
    local files_updated=0
    
    for file in "${cs_files[@]}"; do
        # Remove leading ./
        file="${file#./}"
        
        # Skip if file doesn't exist (shouldn't happen, but safety check)
        if [[ ! -f "$file" ]]; then
            log_warning "File not found: $file"
            continue
        fi
        
        # Skip files in directories that should be ignored
        if [[ "$file" =~ ^\..*/ ]]; then
            log_verbose "Skipping $file (hidden directory)"
            continue
        fi
        
        # Check if file contains namespace declaration
        if ! grep -q "^[[:space:]]*namespace[[:space:]]" "$file"; then
            log_verbose "Skipping $file (no namespace declaration)"
            continue
        fi
        
        local current_namespace
        current_namespace=$(get_current_namespace "$file")
        
        local expected_namespace
        if ! expected_namespace=$(get_expected_namespace "$file" "$root_namespace"); then
            log_verbose "Skipping $file (directory should be ignored)"
            continue
        fi
        
        process_cs_file "$file" "$root_namespace"
        
        ((files_processed++))
        
        if [[ "$current_namespace" != "$expected_namespace" && -n "$current_namespace" ]]; then
            ((files_updated++)) || true
        fi
    done
    
    # Summary
    log_info "📊 Summary:"
    log_info "  Files processed: $files_processed"
    
    if [[ "$DRY_RUN" == true ]]; then
        log_info "  Files that would be updated: $files_updated"
        log_warning "No actual changes were made (dry run mode)"
    else
        log_info "  Files updated: $files_updated"
        if [[ $files_updated -gt 0 ]]; then
            log_success "Namespace synchronization completed successfully!"
        else
            log_success "All namespaces were already correct!"
        fi
    fi
}

# Run main function
main "$@"