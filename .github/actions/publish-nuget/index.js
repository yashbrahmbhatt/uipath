const core = require('@actions/core');
const { execSync } = require('child_process');
const fs = require('fs');

async function main() {
  try {
    const packagePath = core.getInput('package-path', { required: true });
    const nugetApiKey = core.getInput('nuget-api-key', { required: true });
    
    core.info(`Publishing package to NuGet.org: ${packagePath}`);
    
    // Verify package exists
    if (!fs.existsSync(packagePath)) {
      throw new Error(`Package file not found: ${packagePath}`);
    }
    
    if (!nugetApiKey || nugetApiKey.trim() === '') {
      throw new Error('NuGet API key is required for publishing to NuGet.org');
    }
    
    const sourceUrl = 'https://api.nuget.org/v3/index.json';
    
    // Prepare dotnet nuget push command
    const pushCommand = [
      'dotnet', 'nuget', 'push',
      `"${packagePath}"`,
      '--api-key', nugetApiKey,
      '--source', sourceUrl,
      '--skip-duplicate'
    ].join(' ');
    
    core.info('Publishing to NuGet.org...');
    
    try {
      execSync(pushCommand, { 
        stdio: 'inherit',
        env: {
          ...process.env,
          DOTNET_CLI_TELEMETRY_OPTOUT: '1'
        }
      });
      core.info('Package published successfully to NuGet.org');
    } catch (error) {
      // Check if it's a duplicate package error (which we can ignore due to --skip-duplicate)
      if (error.message.includes('already exists') || error.message.includes('409')) {
        core.warning('Package already exists on NuGet.org, skipping');
      } else {
        throw error;
      }
    }
    
    // Add summary
    await core.summary
      .addHeading('Package Published to NuGet.org')
      .addTable([
        [{data: 'Property', header: true}, {data: 'Value', header: true}],
        ['Package Path', packagePath],
        ['Source URL', sourceUrl],
        ['Status', 'Successfully published']
      ])
      .write();
      
  } catch (error) {
    core.setFailed(`Failed to publish to NuGet.org: ${error.message}`);
    core.error(error.stack || error.toString());
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };
