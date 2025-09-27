const core = require('@actions/core');
const fs = require('fs');
const path = require('path');
const glob = require('glob');

// Configuration
const HIDDEN_DIRS = ['.codedworkflows', '.entities', '.local', '.objects', '.project', '.settings', '.templates', '.tmh'];

/**
 * Extract root namespace from project.json
 */
function getRootNamespace(projectPath) {
  const projectJsonPath = path.join(projectPath, 'project.json');
  
  if (!fs.existsSync(projectJsonPath)) {
    throw new Error('project.json not found in project directory');
  }
  
  try {
    const projectData = JSON.parse(fs.readFileSync(projectJsonPath, 'utf8'));
    const rootNamespace = projectData.name;
    
    if (!rootNamespace) {
      throw new Error('Could not extract root namespace (name field) from project.json');
    }
    
    return rootNamespace;
  } catch (error) {
    if (error instanceof SyntaxError) {
      throw new Error('project.json contains invalid JSON');
    }
    throw error;
  }
}

/**
 * Convert directory path to namespace component
 */
function dirToNamespace(dirPath) {
  // Skip certain directories that shouldn't have namespaces
  for (const hiddenDir of HIDDEN_DIRS) {
    if (dirPath.startsWith(hiddenDir)) {
      return null; // Skip this directory
    }
  }
  
  // Split path by path separator and process each component
  const pathParts = dirPath.split(/[/\\]/).filter(part => part && !part.startsWith('.'));
  
  if (pathParts.length === 0) {
    return '';
  }
  
  // Process each part
  const namespaceParts = pathParts.map(part => {
    // Add underscore prefix to parts that start with numbers
    if (/^[0-9]/.test(part)) {
      return `_${part}`;
    }
    return part;
  });
  
  return namespaceParts.join('.');
}

/**
 * Generate expected namespace for a file
 */
function getExpectedNamespace(filePath, rootNamespace, projectPath) {
  // Get directory path relative to project root
  const relativePath = path.relative(projectPath, filePath);
  const dirPath = path.dirname(relativePath);
  
  // Skip if in root directory
  if (dirPath === '.' || dirPath === '') {
    return rootNamespace;
  }
  
  // Convert directory path to namespace component
  const namespaceSuffix = dirToNamespace(dirPath.replace(/\\/g, '/'));
  
  // Return null if directory should be skipped
  if (namespaceSuffix === null) {
    return null;
  }
  
  // Combine root namespace with directory-based suffix
  if (namespaceSuffix === '') {
    return rootNamespace;
  } else {
    return `${rootNamespace}.${namespaceSuffix}`;
  }
}

/**
 * Extract current namespace from a C# file
 */
function getCurrentNamespace(filePath) {
  const content = fs.readFileSync(filePath, 'utf8');
  const namespaceMatch = content.match(/^\s*namespace\s+([^\s\{;]+)/m);
  
  return namespaceMatch ? namespaceMatch[1] : '';
}

/**
 * Update namespace in a C# file
 */
function updateNamespace(filePath, newNamespace, createBackup) {
  if (createBackup) {
    const backupPath = `${filePath}.bak`;
    fs.copyFileSync(filePath, backupPath);
  }
  
  let content = fs.readFileSync(filePath, 'utf8');
  
  // Update the namespace line
  content = content.replace(/^(\s*)namespace\s+[^\s\{;]+/m, `$1namespace ${newNamespace}`);
  
  fs.writeFileSync(filePath, content, 'utf8');
  
  core.info(`Updated namespace in ${filePath} to: ${newNamespace}`);
}

/**
 * Process a single C# file
 */
function processCSFile(filePath, rootNamespace, projectPath, dryRun, verbose, createBackup) {
  if (verbose) {
    core.info(`Processing: ${filePath}`);
  }
  
  // Get current and expected namespaces
  const currentNamespace = getCurrentNamespace(filePath);
  const expectedNamespace = getExpectedNamespace(filePath, rootNamespace, projectPath);
  
  // Skip if directory should be ignored
  if (expectedNamespace === null) {
    if (verbose) {
      core.info(`  Skipping (directory should be ignored)`);
    }
    return { processed: true, updated: false };
  }
  
  if (verbose) {
    core.info(`  Current namespace: ${currentNamespace}`);
    core.info(`  Expected namespace: ${expectedNamespace}`);
  }
  
  // Check if namespace needs updating
  if (currentNamespace !== expectedNamespace) {
    if (!currentNamespace) {
      core.warning(`No namespace found in ${filePath} - skipping`);
      return { processed: true, updated: false };
    }
    
    core.info(`Namespace mismatch in ${filePath}:`);
    core.info(`  Current:  ${currentNamespace}`);
    core.info(`  Expected: ${expectedNamespace}`);
    
    if (dryRun) {
      core.info(`DRY RUN: Would update namespace in ${filePath} to: ${expectedNamespace}`);
      return { processed: true, updated: true };
    } else {
      updateNamespace(filePath, expectedNamespace, createBackup);
      return { processed: true, updated: true };
    }
  } else {
    if (verbose) {
      core.info(`  Namespace is correct`);
    }
    return { processed: true, updated: false };
  }
}

/**
 * Main execution function
 */
async function main() {
  try {
    const projectPath = core.getInput('project-path') || '.';
    const dryRun = core.getInput('dry-run') === 'true';
    const verbose = core.getInput('verbose') === 'true';
    const createBackup = core.getInput('create-backup') === 'true';
    
    core.info('🚀 Starting namespace synchronization...');
    
    if (dryRun) {
      core.warning('Running in DRY RUN mode - no changes will be made');
    }
    
    // Verify project path exists
    if (!fs.existsSync(projectPath)) {
      throw new Error(`Project path does not exist: ${projectPath}`);
    }
    
    // Get root namespace from project.json
    const rootNamespace = getRootNamespace(projectPath);
    core.info(`Root namespace: ${rootNamespace}`);
    
    // Find all C# files
    const pattern = path.join(projectPath, '**/*.cs').replace(/\\/g, '/');
    const csFiles = glob.sync(pattern, {
      ignore: [
        '**/.*/**',  // Ignore hidden directories
        '**/*.bak'   // Ignore backup files
      ]
    });
    
    if (csFiles.length === 0) {
      core.warning('No C# files found');
      core.setOutput('files-processed', '0');
      core.setOutput('files-updated', '0');
      core.setOutput('root-namespace', rootNamespace);
      return;
    }
    
    core.info(`Found ${csFiles.length} C# files to process`);
    
    // Process each file
    let filesProcessed = 0;
    let filesUpdated = 0;
    
    for (const file of csFiles) {
      // Skip files that don't contain namespace declaration
      const content = fs.readFileSync(file, 'utf8');
      if (!/^\s*namespace\s+/m.test(content)) {
        if (verbose) {
          core.info(`Skipping ${file} (no namespace declaration)`);
        }
        continue;
      }
      
      const result = processCSFile(file, rootNamespace, projectPath, dryRun, verbose, createBackup);
      
      if (result.processed) {
        filesProcessed++;
        if (result.updated) {
          filesUpdated++;
        }
      }
    }
    
    // Summary
    core.info('📊 Summary:');
    core.info(`  Files processed: ${filesProcessed}`);
    
    if (dryRun) {
      core.info(`  Files that would be updated: ${filesUpdated}`);
      core.warning('No actual changes were made (dry run mode)');
    } else {
      core.info(`  Files updated: ${filesUpdated}`);
      if (filesUpdated > 0) {
        core.info('✅ Namespace synchronization completed successfully!');
      } else {
        core.info('✅ All namespaces were already correct!');
      }
    }
    
    // Set outputs
    core.setOutput('files-processed', filesProcessed.toString());
    core.setOutput('files-updated', filesUpdated.toString());
    core.setOutput('root-namespace', rootNamespace);
    
    // Create a summary (only in GitHub Actions environment)
    if (process.env.GITHUB_STEP_SUMMARY) {
      await core.summary
        .addHeading('Namespace Synchronization Complete')
        .addTable([
          [{data: 'Property', header: true}, {data: 'Value', header: true}],
          ['Root Namespace', rootNamespace],
          ['Files Processed', filesProcessed.toString()],
          ['Files Updated', filesUpdated.toString()],
          ['Dry Run Mode', dryRun ? 'Yes' : 'No']
        ])
        .write();
    }
    
  } catch (error) {
    core.setFailed(`Namespace synchronization failed: ${error.message}`);
    core.error(error.stack || error.toString());
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };