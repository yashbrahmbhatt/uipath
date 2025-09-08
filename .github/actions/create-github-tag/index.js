const core = require('@actions/core');
const github = require('@actions/github');
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

async function main() {
  try {
    const fullVersion = core.getInput('full-version', { required: true });
    const githubToken = core.getInput('github-token') || process.env.GITHUB_TOKEN;
    
    core.info(`Creating tag: ${fullVersion}`);
    
    // Debug environment variables
    core.info(`GITHUB_WORKSPACE: ${process.env.GITHUB_WORKSPACE}`);
    core.info(`GITHUB_REPOSITORY: ${process.env.GITHUB_REPOSITORY}`);
    core.info(`RUNNER_WORKSPACE: ${process.env.RUNNER_WORKSPACE}`);
    core.info(`Initial working directory: ${process.cwd()}`);
    
    // Ensure we're in the workspace directory
    const workspaceDir = process.env.GITHUB_WORKSPACE || process.cwd();
    core.info(`GITHUB_WORKSPACE environment variable: ${workspaceDir}`);
    
    // Try to change to the workspace directory
    try {
      process.chdir(workspaceDir);
      core.info(`Changed to workspace directory: ${process.cwd()}`);
    } catch (chdirError) {
      core.warning(`Could not change to workspace directory: ${chdirError.message}`);
      core.info(`Staying in current directory: ${process.cwd()}`);
    }
    
    // List directory contents for debugging
    try {
      const files = execSync('ls -la', { encoding: 'utf8', stdio: 'pipe' });
      core.info(`Directory contents:\n${files}`);
    } catch (listError) {
      try {
        // Fallback to dir command on Windows
        const files = execSync('dir', { encoding: 'utf8', stdio: 'pipe' });
        core.info(`Directory contents (Windows):\n${files}`);
      } catch (dirError) {
        core.warning(`Could not list directory: ${listError.message}, ${dirError.message}`);
      }
    }
    
    // Handle Git's "dubious ownership" security feature in GitHub Actions
    try {
      core.info('Configuring Git safe directory for GitHub Actions...');
      execSync(`git config --global --add safe.directory "${workspaceDir}"`, { stdio: 'pipe', timeout: 5000 });
      core.info(`Added ${workspaceDir} to Git safe directories`);
    } catch (configError) {
      core.warning(`Could not configure Git safe directory: ${configError.message}`);
    }
    
    // Check for .git directory specifically
    try {
      const gitStatus = execSync('git status --porcelain', { encoding: 'utf8', stdio: 'pipe', timeout: 5000 });
      core.info(`Git status check passed`);
    } catch (statusError) {
      core.warning(`Git status failed: ${statusError.message}`);
    }
    
    // Verify we're in a git repository with better error handling
    try {
      const gitDir = execSync('git rev-parse --git-dir', { encoding: 'utf8', stdio: 'pipe', timeout: 5000 }).trim();
      core.info(`Confirmed we are in a git repository. Git directory: ${gitDir}`);
    } catch (error) {
      core.warning(`Git repository check failed: ${error.message}`);
      
      // Try to find the git repository in parent directories or common locations
      const possibleDirectories = [
        process.cwd(),
        workspaceDir,
        path.join(process.cwd(), '..'),
        path.join(workspaceDir, '..'),
        // Common GitHub Actions patterns
        path.dirname(workspaceDir),
      ];
      
      let foundGit = false;
      
      for (const dir of possibleDirectories) {
        try {
          if (fs.existsSync(dir)) {
            process.chdir(dir);
            
            // Add this directory to safe directories as well
            try {
              execSync(`git config --global --add safe.directory "${dir}"`, { stdio: 'pipe', timeout: 3000 });
              core.info(`Added ${dir} to Git safe directories`);
            } catch (safeConfigError) {
              core.info(`Could not add ${dir} to safe directories: ${safeConfigError.message}`);
            }
            
            execSync('git rev-parse --git-dir', { stdio: 'pipe', timeout: 3000 });
            core.info(`Found git repository at: ${dir}`);
            foundGit = true;
            break;
          }
        } catch (e) {
          // Continue searching
          core.info(`Not a git repo: ${dir}`);
        }
      }
      
      if (!foundGit) {
        throw new Error(`Not in a git repository. Checked directories: ${possibleDirectories.join(', ')}. Original error: ${error.message}`);
      }
    }
    
    // Configure git for GitHub Actions
    execSync('git config user.name "github-actions"', { stdio: 'inherit', timeout: 10000 });
    execSync('git config user.email "github-actions@github.com"', { stdio: 'inherit', timeout: 10000 });
    
    // Check if tag already exists
    try {
      execSync(`git rev-parse ${fullVersion}`, { stdio: 'pipe', timeout: 5000 });
      core.warning(`Tag ${fullVersion} already exists, skipping creation`);
      return;
    } catch (error) {
      // Tag doesn't exist, continue with creation
      core.info(`Tag ${fullVersion} does not exist, creating...`);
    }
    
    // Create the tag
    execSync(`git tag ${fullVersion}`, { stdio: 'inherit', timeout: 10000 });
    core.info(`Created tag: ${fullVersion}`);
    
    // Push the tag
    if (githubToken) {
      // Set up authenticated remote URL
      const context = github.context;
      const remoteUrl = `https://x-access-token:${githubToken}@github.com/${context.repo.owner}/${context.repo.repo}`;
      execSync(`git remote set-url origin ${remoteUrl}`, { stdio: 'pipe', timeout: 5000 });
    }
    
    execSync(`git push origin ${fullVersion}`, { stdio: 'inherit', timeout: 30000 });
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
    
    // Add summary (only in GitHub Actions environment)
    if (process.env.GITHUB_STEP_SUMMARY) {
      await core.summary
        .addHeading('Tag Created')
        .addTable([
          [{data: 'Property', header: true}, {data: 'Value', header: true}],
          ['Tag Name', fullVersion],
          ['Status', 'Successfully created and pushed']
        ])
        .write();
    } else {
      core.info("Skipping GitHub Actions summary (not in GitHub Actions environment)");
    }
      
  } catch (error) {
    core.setFailed(`Failed to create tag: ${error.message}`);
    core.error(error.stack || error.toString());
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };
