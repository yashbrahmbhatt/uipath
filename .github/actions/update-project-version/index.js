const core = require('@actions/core');
const fs = require('fs');
const path = require('path');

/**
 * Validate that project.json exists and is valid JSON
 */
function validateProjectFile(projectPath) {
  const projectJsonPath = path.join(projectPath, 'project.json');
  
  if (!fs.existsSync(projectJsonPath)) {
    throw new Error('project.json not found in project directory');
  }
  
  try {
    const content = fs.readFileSync(projectJsonPath, 'utf8');
    JSON.parse(content);
    return projectJsonPath;
  } catch (error) {
    if (error instanceof SyntaxError) {
      throw new Error('project.json contains invalid JSON');
    }
    throw error;
  }
}

/**
 * Get current project version from project.json
 */
function getCurrentVersion(projectJsonPath) {
  const projectData = JSON.parse(fs.readFileSync(projectJsonPath, 'utf8'));
  return projectData.projectVersion || '1.0.0';
}

/**
 * Validate version format (major.minor.patch)
 */
function validateVersion(version) {
  const versionRegex = /^[0-9]+\.[0-9]+\.[0-9]+$/;
  if (!versionRegex.test(version)) {
    throw new Error(`Invalid version format: ${version} (expected: major.minor.patch)`);
  }
}

/**
 * Increment version number based on action
 */
function incrementVersion(currentVersion, action) {
  validateVersion(currentVersion);
  
  const [major, minor, patch] = currentVersion.split('.').map(Number);
  
  switch (action) {
    case 'major':
      return `${major + 1}.0.0`;
    case 'minor':
      return `${major}.${minor + 1}.0`;
    case 'patch':
      return `${major}.${minor}.${patch + 1}`;
    default:
      throw new Error(`Unknown version action: ${action}`);
  }
}

/**
 * Parse package string (NAME:VERSION format)
 */
function parsePackage(packageString) {
  const match = packageString.match(/^([^:]+):(.+)$/);
  if (!match) {
    throw new Error(`Invalid package format: ${packageString} (expected: NAME:VERSION)`);
  }
  return { name: match[1], version: match[2] };
}

/**
 * Parse packages input (supports JSON array or newline-separated strings)
 */
function parsePackagesInput(packagesInput) {
  if (!packagesInput || packagesInput.trim() === '') {
    return [];
  }
  
  // Try to parse as JSON array first
  try {
    const parsed = JSON.parse(packagesInput);
    if (Array.isArray(parsed)) {
      return parsed.map(pkg => {
        if (typeof pkg === 'string') {
          return parsePackage(pkg);
        } else if (pkg.name && pkg.version) {
          return { name: pkg.name, version: pkg.version };
        } else {
          throw new Error(`Invalid package object: ${JSON.stringify(pkg)}`);
        }
      });
    }
  } catch (error) {
    // Not valid JSON, fall through to string parsing
  }
  
  // Parse as newline-separated strings
  return packagesInput
    .split(/\r?\n/)
    .map(line => line.trim())
    .filter(line => line && !line.startsWith('#'))
    .map(parsePackage);
}

/**
 * Get current package version from project.json
 */
function getPackageVersion(projectJsonPath, packageName) {
  const projectData = JSON.parse(fs.readFileSync(projectJsonPath, 'utf8'));
  const dependencies = projectData.dependencies || {};
  return dependencies[packageName] || null;
}

/**
 * Create backup file
 */
function createBackup(projectJsonPath) {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
  const backupPath = `${projectJsonPath}.backup.${timestamp}`;
  fs.copyFileSync(projectJsonPath, backupPath);
  core.info(`Created backup: ${backupPath}`);
  return backupPath;
}

/**
 * Update project version in project.json
 */
function updateProjectVersion(projectJsonPath, newVersion, dryRun) {
  if (dryRun) {
    core.info(`DRY RUN: Would update project version to: ${newVersion}`);
    return;
  }
  
  const projectData = JSON.parse(fs.readFileSync(projectJsonPath, 'utf8'));
  projectData.projectVersion = newVersion;
  
  fs.writeFileSync(projectJsonPath, JSON.stringify(projectData, null, 2), 'utf8');
  core.info(`✅ Updated project version to: ${newVersion}`);
}

/**
 * Update package dependency in project.json
 */
function updatePackageDependency(projectJsonPath, packageName, packageVersion, dryRun) {
  if (dryRun) {
    core.info(`DRY RUN: Would update ${packageName} to: [${packageVersion}]`);
    return;
  }
  
  const projectData = JSON.parse(fs.readFileSync(projectJsonPath, 'utf8'));
  
  if (!projectData.dependencies) {
    projectData.dependencies = {};
  }
  
  projectData.dependencies[packageName] = `[${packageVersion}]`;
  
  fs.writeFileSync(projectJsonPath, JSON.stringify(projectData, null, 2), 'utf8');
  core.info(`✅ Updated ${packageName} to: [${packageVersion}]`);
}

/**
 * Show current dependencies
 */
function showCurrentDependencies(projectJsonPath, verbose) {
  if (!verbose) return;
  
  const projectData = JSON.parse(fs.readFileSync(projectJsonPath, 'utf8'));
  const dependencies = projectData.dependencies || {};
  
  core.info('Current dependencies:');
  Object.entries(dependencies).forEach(([name, version]) => {
    core.info(`  ${name}: ${version}`);
  });
}

/**
 * Main execution function
 */
async function main() {
  try {
    const projectPath = core.getInput('project-path') || '.';
    const versionInput = core.getInput('version');
    const major = core.getInput('major') === 'true';
    const minor = core.getInput('minor') === 'true';
    const patch = core.getInput('patch') === 'true';
    const packagesInput = core.getInput('packages');
    const dryRun = core.getInput('dry-run') === 'true';
    const verbose = core.getInput('verbose') === 'true';
    const createBackup = core.getInput('create-backup') === 'true';
    
    core.info('🚀 Starting project version update...');
    
    if (dryRun) {
      core.warning('Running in DRY RUN mode - no changes will be made');
    }
    
    // Verify project path exists
    if (!fs.existsSync(projectPath)) {
      throw new Error(`Project path does not exist: ${projectPath}`);
    }
    
    // Validate project.json
    const projectJsonPath = validateProjectFile(projectPath);
    
    // Get current version
    const currentVersion = getCurrentVersion(projectJsonPath);
    core.info(`Current project version: ${currentVersion}`);
    
    // Show current dependencies if verbose
    showCurrentDependencies(projectJsonPath, verbose);
    
    // Parse packages to update
    const packagesToUpdate = parsePackagesInput(packagesInput);
    
    // Validate version action inputs
    const versionActions = [major, minor, patch].filter(Boolean);
    if (versionInput && versionActions.length > 0) {
      throw new Error('Cannot specify both explicit version and version increment action');
    }
    if (versionActions.length > 1) {
      throw new Error('Cannot specify multiple version increment actions');
    }
    if (!versionInput && versionActions.length === 0 && packagesToUpdate.length === 0) {
      throw new Error('No action specified. Must specify version update or package updates.');
    }
    
    let backupFile = null;
    
    // Create backup before making changes
    if (!dryRun && createBackup) {
      backupFile = createBackup(projectJsonPath);
    }
    
    let newVersion = currentVersion;
    
    // Update project version if requested
    if (versionInput) {
      validateVersion(versionInput);
      newVersion = versionInput;
      core.info(`Setting project version: ${currentVersion} → ${newVersion}`);
      updateProjectVersion(projectJsonPath, newVersion, dryRun);
    } else if (versionActions.length > 0) {
      const action = major ? 'major' : minor ? 'minor' : 'patch';
      newVersion = incrementVersion(currentVersion, action);
      core.info(`Incrementing project version (${action}): ${currentVersion} → ${newVersion}`);
      updateProjectVersion(projectJsonPath, newVersion, dryRun);
    }
    
    // Update packages if requested
    let packagesUpdated = 0;
    if (packagesToUpdate.length > 0) {
      core.info(`Updating ${packagesToUpdate.length} package(s)...`);
      
      for (const pkg of packagesToUpdate) {
        const currentPkgVersion = getPackageVersion(projectJsonPath, pkg.name);
        
        if (!currentPkgVersion) {
          core.warning(`Package not found in dependencies: ${pkg.name}`);
          continue;
        }
        
        // Remove brackets from current version for comparison
        const cleanCurrentVersion = currentPkgVersion.replace(/[\[\]]/g, '');
        
        if (cleanCurrentVersion === pkg.version) {
          if (verbose) {
            core.info(`Package ${pkg.name} already at version: ${pkg.version}`);
          }
          continue;
        }
        
        core.info(`Updating package: ${pkg.name} (${cleanCurrentVersion} → ${pkg.version})`);
        updatePackageDependency(projectJsonPath, pkg.name, pkg.version, dryRun);
        packagesUpdated++;
      }
    }
    
    // Summary
    core.info('📊 Summary:');
    
    if (newVersion !== currentVersion) {
      if (dryRun) {
        core.info(`  Project version would be updated: ${currentVersion} → ${newVersion}`);
      } else {
        core.info(`  Project version updated: ${currentVersion} → ${newVersion}`);
      }
    }
    
    if (packagesToUpdate.length > 0) {
      if (dryRun) {
        core.info(`  ${packagesUpdated} package(s) would be updated`);
      } else {
        core.info(`  ${packagesUpdated} package(s) updated`);
      }
    }
    
    if (dryRun) {
      core.warning('No actual changes were made (dry run mode)');
    } else {
      core.info('✅ Project update completed successfully!');
    }
    
    // Set outputs
    core.setOutput('old-version', currentVersion);
    core.setOutput('new-version', newVersion);
    core.setOutput('packages-updated', packagesUpdated.toString());
    if (backupFile) {
      core.setOutput('backup-file', backupFile);
    }
    
    // Create a summary (only in GitHub Actions environment)
    if (process.env.GITHUB_STEP_SUMMARY) {
      const summaryTable = [
        [{data: 'Property', header: true}, {data: 'Value', header: true}],
        ['Original Version', currentVersion],
        ['New Version', newVersion],
        ['Packages Updated', packagesUpdated.toString()],
        ['Dry Run Mode', dryRun ? 'Yes' : 'No']
      ];
      
      if (backupFile) {
        summaryTable.push(['Backup File', backupFile]);
      }
      
      await core.summary
        .addHeading('Project Version Update Complete')
        .addTable(summaryTable)
        .write();
    }
    
  } catch (error) {
    core.setFailed(`Project version update failed: ${error.message}`);
    core.error(error.stack || error.toString());
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };