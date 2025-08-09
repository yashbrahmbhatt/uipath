const core = require('@actions/core');
const github = require('@actions/github');
const { execSync } = require('child_process');
const fs = require('fs');

async function main() {
  try {
    const packagePath = core.getInput('package-path', { required: true });
    const githubToken = core.getInput('github-token') || process.env.GITHUB_TOKEN;
    const repositoryOwner = core.getInput('repository-owner') || github.context.repo.owner;
    
    core.info(`Publishing package: ${packagePath}`);
    core.info(`Repository owner: ${repositoryOwner}`);
    
    // Verify package exists
    if (!fs.existsSync(packagePath)) {
      throw new Error(`Package file not found: ${packagePath}`);
    }
    
    if (!githubToken) {
      throw new Error('GitHub token is required for publishing to GitHub Packages');
    }
    
    // Construct the GitHub Packages NuGet source URL
    const sourceUrl = `https://nuget.pkg.github.com/${repositoryOwner}/index.json`;
    
    core.info(`Publishing to GitHub Packages: ${sourceUrl}`);
    
    // Prepare dotnet nuget push command
    const pushCommand = [
      'dotnet', 'nuget', 'push',
      `"${packagePath}"`,
      '--api-key', githubToken,
      '--source', sourceUrl,
      '--skip-duplicate'
    ].join(' ');
    
    core.info('Executing push command...');
    
    try {
      execSync(pushCommand, { 
        stdio: 'inherit',
        env: {
          ...process.env,
          DOTNET_CLI_TELEMETRY_OPTOUT: '1'
        }
      });
      core.info('Package published successfully to GitHub Packages');
    } catch (error) {
      // Check if it's a duplicate package error (which we can ignore due to --skip-duplicate)
      if (error.message.includes('already exists') || error.message.includes('409')) {
        core.warning('Package already exists in GitHub Packages, skipping');
      } else {
        throw error;
      }
    }
    
    // Add summary
    await core.summary
      .addHeading('Package Published to GitHub Packages')
      .addTable([
        [{data: 'Property', header: true}, {data: 'Value', header: true}],
        ['Package Path', packagePath],
        ['Repository Owner', repositoryOwner],
        ['Source URL', sourceUrl],
        ['Status', 'Successfully published']
      ])
      .write();
      
  } catch (error) {
    core.setFailed(`Failed to publish to GitHub Packages: ${error.message}`);
    core.error(error.stack || error.toString());
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };
