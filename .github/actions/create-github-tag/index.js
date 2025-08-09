const core = require('@actions/core');
const github = require('@actions/github');
const { execSync } = require('child_process');

async function main() {
  try {
    const fullVersion = core.getInput('full-version', { required: true });
    const githubToken = core.getInput('github-token') || process.env.GITHUB_TOKEN;
    
    core.info(`Creating tag: ${fullVersion}`);
    
    // Configure git for GitHub Actions
    execSync('git config user.name "github-actions"', { stdio: 'inherit' });
    execSync('git config user.email "github-actions@github.com"', { stdio: 'inherit' });
    
    // Check if tag already exists
    try {
      execSync(`git rev-parse ${fullVersion}`, { stdio: 'pipe' });
      core.warning(`Tag ${fullVersion} already exists, skipping creation`);
      return;
    } catch (error) {
      // Tag doesn't exist, continue with creation
      core.info(`Tag ${fullVersion} does not exist, creating...`);
    }
    
    // Create the tag
    execSync(`git tag ${fullVersion}`, { stdio: 'inherit' });
    core.info(`Created tag: ${fullVersion}`);
    
    // Push the tag
    if (githubToken) {
      // Set up authenticated remote URL
      const context = github.context;
      const remoteUrl = `https://x-access-token:${githubToken}@github.com/${context.repo.owner}/${context.repo.repo}`;
      execSync(`git remote set-url origin ${remoteUrl}`, { stdio: 'pipe' });
    }
    
    execSync(`git push origin ${fullVersion}`, { stdio: 'inherit' });
    core.info(`Pushed tag: ${fullVersion}`);
    
    // Create GitHub release if we have a token
    if (githubToken) {
      try {
        const octokit = github.getOctokit(githubToken);
        const context = github.context;
        
        const release = await octokit.rest.repos.createRelease({
          owner: context.repo.owner,
          repo: context.repo.repo,
          tag_name: fullVersion,
          name: `Release ${fullVersion}`,
          draft: false,
          prerelease: false,
          generate_release_notes: true
        });
        
        core.info(`Created GitHub release: ${release.data.html_url}`);
      } catch (error) {
        core.warning(`Failed to create GitHub release: ${error.message}`);
      }
    }
    
    // Add summary
    await core.summary
      .addHeading('Tag Created')
      .addTable([
        [{data: 'Property', header: true}, {data: 'Value', header: true}],
        ['Tag Name', fullVersion],
        ['Status', 'Successfully created and pushed']
      ])
      .write();
      
  } catch (error) {
    core.setFailed(`Failed to create tag: ${error.message}`);
    core.error(error.stack || error.toString());
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };
