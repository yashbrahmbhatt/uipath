# Publish to UiPath Orchestrator Action

This GitHub Action publishes UiPath packages (`.nupkg` files) to a UiPath Orchestrator instance using the UiPath CLI (`uipcli`).

## Usage

### Basic Example with Username/Password Authentication

```yaml
- name: Publish to UiPath Orchestrator
  uses: ./.github/actions/publish-orchestrator
  with:
    package-path: './dist/MyPackage.1.0.0.nupkg'
    orchestrator-url: 'https://your-orchestrator.com'
    orchestrator-tenant: 'YourTenant'
    orchestrator-username: '${{ secrets.ORCHESTRATOR_USERNAME }}'
    orchestrator-password: '${{ secrets.ORCHESTRATOR_PASSWORD }}'
    orchestrator-folder: 'Production'
```

### Token Authentication

```yaml
- name: Publish to UiPath Orchestrator
  uses: ./.github/actions/publish-orchestrator
  with:
    package-path: './dist/MyPackage.1.0.0.nupkg'
    orchestrator-url: 'https://your-orchestrator.com'
    orchestrator-tenant: 'YourTenant'
    auth-token: '${{ secrets.ORCHESTRATOR_TOKEN }}'
    account-name: 'YourAccountName'
```

### Application Authentication

```yaml
- name: Publish to UiPath Orchestrator
  uses: ./.github/actions/publish-orchestrator
  with:
    package-path: './dist/MyPackage.1.0.0.nupkg'
    orchestrator-url: 'https://your-orchestrator.com'
    orchestrator-tenant: 'YourTenant'
    account-name: 'YourAccountName'
    application-id: '${{ secrets.APPLICATION_ID }}'
    application-secret: '${{ secrets.APPLICATION_SECRET }}'
```

## Inputs

| Input | Required | Description |
|-------|----------|-------------|
| `package-path` | Yes | Path to the .nupkg file or folder containing packages to publish |
| `orchestrator-url` | Yes | The URL of the Orchestrator instance |
| `orchestrator-tenant` | Yes | The tenant of the Orchestrator instance |
| `orchestrator-folder` | No | The Orchestrator folder name |
| `orchestrator-username` | No | The Orchestrator username (for username/password auth) |
| `orchestrator-password` | No | The Orchestrator password (for username/password auth) |
| `auth-token` | No | The Orchestrator refresh token (for token auth) |
| `account-name` | No | The Orchestrator organization name |
| `application-id` | No | The external application id (for app auth) |
| `application-secret` | No | The external application secret (for app auth) |
| `application-scope` | No | The space-separated list of application scopes (default: standard scopes) |
| `identity-url` | No | URL of your identity server (required for PaaS deployments) |
| `create-process` | No | Create a process in the respective folder (default: true) |
| `entry-points-path` | No | Define the specific entry points (default: Main.xaml) |
| `environments` | No | Comma-separated list of environments to deploy to |
| `ignore-library-deploy-conflict` | No | Ignore conflicts when deploying library with same version (default: false) |
| `process-name` | No | Custom name for the process to be updated/created |
| `trace-level` | No | Log message level (default: Information) |

## Authentication

The action supports three authentication methods:

1. **Username/Password**: Use `orchestrator-username` and `orchestrator-password`
2. **Token**: Use `auth-token` and `account-name`
3. **Application**: Use `application-id`, `application-secret`, and `account-name`

## Prerequisites

- UiPath CLI (`uipcli`) must be installed and available in the runner's PATH
- Valid UiPath Orchestrator credentials
- The package file must exist at the specified path

## Error Handling

The action will fail if:
- Required inputs are missing
- Package file doesn't exist
- Authentication credentials are invalid
- UiPath CLI command fails

The action will create a deployment summary showing the deployment details upon success.
