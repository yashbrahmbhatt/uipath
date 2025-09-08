const core = require('@actions/core');
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

async function main() {
  try {
    const packagePath = core.getInput('package-path', { required: true });
    const orchestratorUrl = core.getInput('orchestrator-url', { required: true });
    const orchestratorTenant = core.getInput('orchestrator-tenant', { required: true });
    const orchestratorFolder = core.getInput('orchestrator-folder');
    const orchestratorUsername = core.getInput('orchestrator-username');
    const orchestratorPassword = core.getInput('orchestrator-password');
    const authToken = core.getInput('auth-token');
    const accountName = core.getInput('account-name');
    const applicationId = core.getInput('application-id');
    const applicationSecret = core.getInput('application-secret');
    const applicationScope = core.getInput('application-scope');
    const identityUrl = core.getInput('identity-url');
    const createProcess = core.getInput('create-process');
    const entryPointsPath = core.getInput('entry-points-path');
    const environments = core.getInput('environments');
    const ignoreLibraryDeployConflict = core.getInput('ignore-library-deploy-conflict');
    const processName = core.getInput('process-name');
    const traceLevel = core.getInput('trace-level');
    
    core.info(`Publishing package to UiPath Orchestrator: ${packagePath}`);
    
    // Verify package exists
    if (!fs.existsSync(packagePath)) {
      throw new Error(`Package file or folder not found: ${packagePath}`);
    }
    
    // Validate authentication parameters
    const hasUsernamePassword = orchestratorUsername && orchestratorPassword;
    const hasTokenAuth = authToken && accountName;
    const hasAppAuth = applicationId && applicationSecret;
    
    if (!hasUsernamePassword && !hasTokenAuth && !hasAppAuth) {
      throw new Error('Authentication required: provide either username/password, token/account, or application credentials');
    }
    
    // Build the uipcli command
    const command = ['uipcli', 'package', 'deploy'];
    
    // Add positional arguments
    command.push(`"${packagePath}"`);
    command.push(`"${orchestratorUrl}"`);
    command.push(`"${orchestratorTenant}"`);
    
    // Add authentication parameters
    if (hasUsernamePassword) {
      command.push('-u', `"${orchestratorUsername}"`);
      command.push('-p', `"${orchestratorPassword}"`);
    } else if (hasTokenAuth) {
      command.push('-t', `"${authToken}"`);
      command.push('-a', `"${accountName}"`);
    } else if (hasAppAuth) {
      command.push('-I', `"${applicationId}"`);
      command.push('-S', `"${applicationSecret}"`);
      command.push('-A', `"${accountName}"`);
      if (applicationScope) {
        command.push('--applicationScope', `"${applicationScope}"`);
      }
    }
    
    // Add optional parameters
    if (orchestratorFolder) {
      command.push('-o', `"${orchestratorFolder}"`);
    }
    
    if (identityUrl) {
      command.push('--identityUrl', `"${identityUrl}"`);
    }
    
    if (createProcess && createProcess.toLowerCase() !== 'true') {
      command.push('-c', createProcess);
    }
    
    if (entryPointsPath) {
      command.push('--entryPointsPath', `"${entryPointsPath}"`);
    }
    
    if (environments) {
      command.push('-e', `"${environments}"`);
    }
    
    if (ignoreLibraryDeployConflict && ignoreLibraryDeployConflict.toLowerCase() === 'true') {
      command.push('--ignoreLibraryDeployConflict');
    }
    
    if (processName) {
      command.push('--processName', `"${processName}"`);
    }
    
    if (traceLevel) {
      command.push('--traceLevel', traceLevel);
    }
    
    const deployCommand = command.join(' ');
    
    core.info('Publishing to UiPath Orchestrator...');
    core.info(`Command: ${deployCommand}`);
    
    try {
      execSync(deployCommand, { 
        stdio: 'inherit',
        env: {
          ...process.env
        }
      });
      core.info('Package published successfully to UiPath Orchestrator');
    } catch (error) {
      // Check for specific error patterns that might be expected
      if (error.message.includes('already exists') && ignoreLibraryDeployConflict === 'true') {
        core.warning('Package already exists in Orchestrator, but ignoring due to ignoreLibraryDeployConflict flag');
      } else {
        throw error;
      }
    }
    
    // Add summary
    const packageName = path.basename(packagePath);
    await core.summary
      .addHeading('Package Published to UiPath Orchestrator')
      .addTable([
        [{data: 'Property', header: true}, {data: 'Value', header: true}],
        ['Package', packageName],
        ['Orchestrator URL', orchestratorUrl],
        ['Tenant', orchestratorTenant],
        ['Folder', orchestratorFolder || 'Default'],
        ['Create Process', createProcess],
        ['Entry Points', entryPointsPath || 'Main.xaml'],
        ['Status', 'Successfully published']
      ])
      .write();
      
  } catch (error) {
    core.setFailed(`Failed to publish to UiPath Orchestrator: ${error.message}`);
    core.error(error.stack || error.toString());
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };
