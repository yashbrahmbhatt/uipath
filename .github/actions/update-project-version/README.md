# Update Project Version Action

This GitHub Action updates project version and package dependencies in UiPath `project.json` files.

## Description

The action provides comprehensive version management for UiPath projects, supporting semantic versioning increments and package dependency updates. It can update project versions using semantic versioning rules or set specific versions, and update package dependencies with version constraints.

## Usage

```yaml
- name: Update Project Version
  uses: ./.github/actions/update-project-version
  with:
    project-path: './path/to/uipath/project'  # Optional, defaults to '.'
    minor: 'true'                             # Increment minor version
    packages: |                               # Update packages (newline-separated)
      UiPath.CodedWorkflows:24.11.0
      UiPath.Excel.Activities:3.3.0
    verbose: 'true'                           # Show detailed information
    create-backup: 'true'                     # Create backup before changes
```

## Inputs

| Input | Description | Required | Default |
|-------|-------------|----------|---------|
| `project-path` | Path to the project folder containing project.json | No | `.` |
| `version` | Set specific project version (e.g., "25.10.0") | No | - |
| `major` | Increment major version (X.0.0) | No | `false` |
| `minor` | Increment minor version (X.Y.0) | No | `false` |
| `patch` | Increment patch version (X.Y.Z) | No | `false` |
| `packages` | Update packages (JSON array or newline-separated NAME:VERSION pairs) | No | - |
| `dry-run` | Show what would be changed without making changes | No | `false` |
| `verbose` | Show detailed processing information | No | `false` |
| `create-backup` | Create backup file before making changes | No | `false` |

## Outputs

| Output | Description |
|--------|-------------|
| `old-version` | The original project version |
| `new-version` | The updated project version |
| `packages-updated` | Number of packages updated |
| `backup-file` | Path to backup file (if created) |

## Package Input Formats

### Newline-separated (recommended for workflow files)
```yaml
packages: |
  UiPath.CodedWorkflows:24.11.0
  UiPath.Excel.Activities:3.3.0
  Yash.Config:1.0.255.4200
```

### JSON Array
```yaml
packages: '["UiPath.CodedWorkflows:24.11.0", "UiPath.Excel.Activities:3.3.0"]'
```

### JSON Object Array
```yaml
packages: '[{"name": "UiPath.CodedWorkflows", "version": "24.11.0"}]'
```

## Examples

### Increment Minor Version
```yaml
- name: Bump Minor Version
  uses: ./.github/actions/update-project-version
  with:
    minor: 'true'
    verbose: 'true'
```

### Set Specific Version
```yaml
- name: Set Version to 26.0.0
  uses: ./.github/actions/update-project-version
  with:
    version: '26.0.0'
    create-backup: 'true'
```

### Update Packages Only
```yaml
- name: Update Dependencies
  uses: ./.github/actions/update-project-version
  with:
    packages: |
      UiPath.CodedWorkflows:24.11.0
      UiPath.Excel.Activities:3.3.0
    verbose: 'true'
```

### Combined Version and Package Update
```yaml
- name: Release Update
  uses: ./.github/actions/update-project-version
  with:
    patch: 'true'
    packages: |
      UiPath.CodedWorkflows:24.11.0
      Yash.Config:1.0.255.4200
    create-backup: 'true'
    verbose: 'true'
```

### Dry Run Mode
```yaml
- name: Preview Changes
  uses: ./.github/actions/update-project-version
  with:
    minor: 'true'
    packages: |
      UiPath.CodedWorkflows:24.11.0
    dry-run: 'true'
    verbose: 'true'
```

## Complete Workflow Example

```yaml
name: Update Project Version

on:
  workflow_dispatch:
    inputs:
      version_type:
        description: 'Version increment type'
        required: true
        default: 'patch'
        type: choice
        options:
        - major
        - minor
        - patch
      update_packages:
        description: 'Update packages to latest versions'
        required: false
        default: false
        type: boolean

jobs:
  update-version:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Update Project Version
      id: update
      uses: ./.github/actions/update-project-version
      with:
        ${{ github.event.inputs.version_type }}: 'true'
        packages: |
          UiPath.CodedWorkflows:24.11.0
          UiPath.Excel.Activities:3.3.0
        verbose: 'true'
        create-backup: 'true'
    
    - name: Commit Changes
      if: steps.update.outputs.new-version != steps.update.outputs.old-version
      run: |
        git config --local user.email "action@github.com"
        git config --local user.name "GitHub Action"
        git add project.json
        git commit -m "chore: update version to ${{ steps.update.outputs.new-version }}"
        git push
    
    - name: Create Release
      if: steps.update.outputs.new-version != steps.update.outputs.old-version
      uses: actions/create-release@v1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        tag_name: v${{ steps.update.outputs.new-version }}
        release_name: Release ${{ steps.update.outputs.new-version }}
        body: |
          Version updated from ${{ steps.update.outputs.old-version }} to ${{ steps.update.outputs.new-version }}
          Packages updated: ${{ steps.update.outputs.packages-updated }}
```

## Version Increment Rules

| Action | Example Transformation |
|--------|----------------------|
| `major: true` | `1.2.3` → `2.0.0` |
| `minor: true` | `1.2.3` → `1.3.0` |
| `patch: true` | `1.2.3` → `1.2.4` |
| `version: "2.5.0"` | `1.2.3` → `2.5.0` |

## Package Version Format

Packages in `project.json` are stored with bracket notation:
```json
{
  "dependencies": {
    "UiPath.CodedWorkflows": "[24.11.0]",
    "UiPath.Excel.Activities": "[3.3.0]"
  }
}
```

## Error Handling

The action will fail if:
- `project.json` is not found
- `project.json` contains invalid JSON
- Invalid version format is provided
- Multiple version increment actions are specified
- Both explicit version and increment action are specified
- No action is specified (no version update or package updates)
- Package format is invalid

## Backup Behavior

When `create-backup` is enabled, the action creates a timestamped backup file:
```
project.json.backup.2024-01-15T10-30-45
```

These backup files should be cleaned up manually or excluded from version control.