const core = require('@actions/core');
const { execSync } = require('child_process');

async function main() {
  try {
    const projectId = core.getInput('project-id', { required: true });
    
    // Get current date for version base
    const now = new Date();
    const yy = now.getFullYear() % 100;
    const mm = now.getMonth() + 1;
    const base = `${yy}.${mm}`;
    
    core.info(`Computing version for project: ${projectId}`);
    core.info(`Version base: ${base}`);
    
    // Fetch all tags
    try {
      execSync('git fetch --tags', { stdio: 'pipe' });
    } catch (error) {
      core.warning(`Failed to fetch tags: ${error.message}`);
    }
    
    // Find existing tags for this project and version base
    const pattern = `${projectId}@${base}.*`;
    let tags = [];
    
    try {
      const output = execSync(`git tag --list "${pattern}"`, { encoding: 'utf8' });
      tags = output.split('\n').filter(tag => tag.trim());
    } catch (error) {
      core.warning(`Failed to list tags: ${error.message}`);
    }
    
    core.info(`Found ${tags.length} existing tags with pattern: ${pattern}`);
    
    // Calculate next patch version
    let patch = 0;
    if (tags.length > 0) {
      const patches = tags.map(tag => {
        const match = tag.match(new RegExp(`@${base.replace('.', '\\.')}\\.(\\d+)$`));
        return match ? parseInt(match[1], 10) : 0;
      });
      patch = Math.max(...patches) + 1;
    }
    
    const version = `${base}.${patch}`;
    const fullVersion = `${projectId}@${version}`;
    
    core.info(`Computed version: ${version}`);
    core.info(`Full version: ${fullVersion}`);
    
    // Set outputs
    core.setOutput('version', version);
    core.setOutput('full-version', fullVersion);
    
    // Also set as environment variables for compatibility
    core.exportVariable('VERSION', version);
    core.exportVariable('FULL_VERSION', fullVersion);
    
    // Add summary
    await core.summary
      .addHeading('Version Computed')
      .addTable([
        [{data: 'Property', header: true}, {data: 'Value', header: true}],
        ['Project ID', projectId],
        ['Version', version],
        ['Full Version', fullVersion],
        ['Existing Tags', tags.length.toString()]
      ])
      .write();
      
  } catch (error) {
    core.setFailed(`Failed to compute version: ${error.message}`);
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };
