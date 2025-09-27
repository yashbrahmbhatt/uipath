# Project Management Scripts

This directory contains bash scripts to help manage your UiPath CodedWorkflows project.

## Scripts

### 1. update_namespaces.sh
Synchronizes C# namespaces with the directory structure based on the root namespace from project.json.

**Usage:**
```bash
# Show what would be changed
./update_namespaces.sh --dry-run

# Apply namespace corrections
./update_namespaces.sh

# Verbose output
./update_namespaces.sh --verbose
```

### 2. update_project_version.sh
Updates project version and package dependencies in project.json.

**Usage Examples:**

```bash
# Increment version numbers
./update_project_version.sh --major        # X.0.0
./update_project_version.sh --minor        # X.Y.0
./update_project_version.sh --patch        # X.Y.Z

# Set specific version
./update_project_version.sh --version "26.0.0"

# Update specific packages
./update_project_version.sh --package "UiPath.CodedWorkflows:24.11.0"

# Update multiple packages
./update_project_version.sh --package "UiPath.CodedWorkflows:24.11.0" --package "Yash.Config:1.0.255.4300"

# Update packages from file
./update_project_version.sh --packages packages.txt

# Combine version and package updates
./update_project_version.sh --minor --package "UiPath.CodedWorkflows:24.11.0" --verbose

# Dry run (show what would change)
./update_project_version.sh --minor --dry-run

# Create backup before changes
./update_project_version.sh --minor --backup
```

**Package File Format:**
```
# packages.txt
UiPath.CodedWorkflows:24.11.0
UiPath.Excel.Activities:3.3.0
Yash.Config:1.0.255.4300
```

## Requirements

- **Bash**: Available in Git Bash, WSL, or Linux/macOS
- **Python**: Required for update_project_version.sh (available in most environments)

## Features

### update_namespaces.sh
- ✅ Reads root namespace from project.json
- ✅ Maps directory structure to namespaces
- ✅ Handles numbered directories (01_Dispatcher → _01_Dispatcher)
- ✅ Skips hidden/generated directories
- ✅ Dry-run mode to preview changes
- ✅ Verbose logging

### update_project_version.sh
- ✅ Semantic version increment (major/minor/patch)
- ✅ Set specific version numbers
- ✅ Update individual packages
- ✅ Batch update from file
- ✅ JSON validation
- ✅ Backup creation
- ✅ Dry-run mode
- ✅ Verbose logging
- ✅ Cross-platform (Python-based JSON handling)

## Tips

1. **Always use --dry-run first** to see what changes will be made
2. **Use --verbose** for detailed logging and debugging
3. **Create backups** with --backup for important changes
4. **Test in development environments** before production use