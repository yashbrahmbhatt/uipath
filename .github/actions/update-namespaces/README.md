# Update C# Namespaces Action

This GitHub Action synchronizes C# namespaces with directory structure in UiPath CodedWorkflows projects.

## Description

The action reads the root namespace from `project.json` and updates all C# files to have namespaces that match their directory structure. This ensures consistency between file organization and namespace hierarchy.

## Usage

```yaml
- name: Update C# Namespaces
  uses: ./.github/actions/update-namespaces
  with:
    project-path: './path/to/uipath/project'  # Optional, defaults to '.'
    dry-run: 'false'                          # Optional, defaults to 'false'
    verbose: 'true'                           # Optional, defaults to 'false'
    create-backup: 'true'                     # Optional, defaults to 'false'
```

## Inputs

| Input | Description | Required | Default |
|-------|-------------|----------|---------|
| `project-path` | Path to the project folder containing project.json | No | `.` |
| `dry-run` | Show what would be changed without making changes | No | `false` |
| `verbose` | Show detailed processing information | No | `false` |
| `create-backup` | Create backup files before making changes | No | `false` |

## Outputs

| Output | Description |
|--------|-------------|
| `files-processed` | Number of C# files processed |
| `files-updated` | Number of C# files updated |
| `root-namespace` | The root namespace extracted from project.json |

## Example

```yaml
name: Synchronize Namespaces

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  sync-namespaces:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Update C# Namespaces
      uses: ./.github/actions/update-namespaces
      with:
        project-path: './uipath-libraries/MyProject'
        verbose: 'true'
        create-backup: 'true'
      
    - name: Check results
      run: |
        echo "Files processed: ${{ steps.sync-namespaces.outputs.files-processed }}"
        echo "Files updated: ${{ steps.sync-namespaces.outputs.files-updated }}"
        echo "Root namespace: ${{ steps.sync-namespaces.outputs.root-namespace }}"
```

## How it works

1. **Reads root namespace** from the `name` field in `project.json`
2. **Scans directory structure** to determine expected namespaces
3. **Processes C# files** that contain namespace declarations
4. **Updates namespaces** to match directory structure:
   - Files in root directory get the root namespace
   - Files in subdirectories get `RootNamespace.SubDirectory`
   - Directories starting with numbers get underscore prefix (e.g., `01_Dispatcher` → `_01_Dispatcher`)
5. **Skips hidden directories** (those starting with `.`)

## Directory Mapping Examples

Given root namespace `MyProject`:

| File Path | Expected Namespace |
|-----------|-------------------|
| `CodedWorkflow.cs` | `MyProject` |
| `01_Dispatcher/Dispatcher.cs` | `MyProject._01_Dispatcher` |
| `CodedWorkflows/Helper.cs` | `MyProject.CodedWorkflows` |
| `Framework/Utilities/Logger.cs` | `MyProject.Framework.Utilities` |

## Error Handling

The action will fail if:
- `project.json` is not found
- `project.json` contains invalid JSON
- Root namespace cannot be extracted from `project.json`
- File system operations fail

## Backup Behavior

When `create-backup` is enabled, the action creates `.bak` files before modifying any C# files. These backup files should be cleaned up manually or excluded from version control.