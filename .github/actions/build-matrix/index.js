const core = require('@actions/core');
const github = require('@actions/github');
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

function getChangedFiles() {
  core.info('Detecting changed files...');
  
  try {
    const context = github.context;
    const before = context.payload.before || process.env.GITHUB_EVENT_BEFORE;
    const after = context.sha || process.env.GITHUB_SHA;
    
    let command;
    let files = [];
    
    // Try different strategies to get changed files
    if (before && after && before !== '0000000000000000000000000000000000000000') {
      // First try: use the specific commit range
      try {
        command = `git diff --name-only ${before} ${after}`;
        core.info(`Using commit range: ${before}..${after}`);
        const output = execSync(command, { encoding: 'utf8' });
        files = output.split('\n').filter(line => line.trim());
      } catch (error) {
        core.warning(`Failed to use commit range ${before}..${after}: ${error.message}`);
        files = [];
      }
    }
    
    // Fallback 1: Compare with previous commit
    if (files.length === 0) {
      try {
        command = 'git diff --name-only HEAD~1 HEAD';
        core.info('Fallback: Using HEAD~1..HEAD comparison');
        const output = execSync(command, { encoding: 'utf8' });
        files = output.split('\n').filter(line => line.trim());
      } catch (error) {
        core.warning(`Failed HEAD~1 comparison: ${error.message}`);
        files = [];
      }
    }
    
    // Fallback 2: Compare with origin/main
    if (files.length === 0) {
      try {
        command = 'git diff --name-only origin/main HEAD';
        core.info('Fallback: Using origin/main..HEAD comparison');
        const output = execSync(command, { encoding: 'utf8' });
        files = output.split('\n').filter(line => line.trim());
      } catch (error) {
        core.warning(`Failed origin/main comparison: ${error.message}`);
        files = [];
      }
    }
    
    // Fallback 3: If all else fails, build all projects
    if (files.length === 0) {
      core.warning('Could not determine changed files, will build all projects');
      return null; // Signal to build all projects
    }
    
    core.info(`Found ${files.length} changed files`);
    files.forEach(file => core.info(`Changed: ${file}`));
    
    return files;
  } catch (error) {
    core.warning(`Error getting changed files: ${error.message}`);
    return null; // Signal to build all projects
  }
}

function findChangedProjects(changedFiles, monoConfig) {
  core.info('Finding changed projects...');
  const projectsToBuild = new Map();
  
  // Find directly changed projects
  for (const project of monoConfig.projects) {
    if (!project.build) {
      core.info(`Skipping ${project.id} (build: false)`);
      continue;
    }
    
    // Normalize paths for cross-platform compatibility
    const projectPath = project.path.replace(/[/\\]/g, path.sep);
    
    for (const file of changedFiles) {
      const filePath = file.replace(/[/\\]/g, path.sep);
      if (filePath.startsWith(projectPath + path.sep) || filePath === projectPath) {
        core.info(`Project ${project.id} changed (file: ${file})`);
        projectsToBuild.set(project.id, project);
        break;
      }
    }
  }
  
  core.info(`Found ${projectsToBuild.size} directly changed projects`);
  
  // Add transitive dependencies (projects that depend on changed projects)
  let added = true;
  let iteration = 0;
  while (added && iteration < 10) { // Safety limit
    added = false;
    iteration++;
    
    for (const project of monoConfig.projects) {
      if (projectsToBuild.has(project.id) || !project.build) continue;
      
      if (project.dependsOn && Array.isArray(project.dependsOn)) {
        for (const dep of project.dependsOn) {
          if (projectsToBuild.has(dep)) {
            core.info(`Adding ${project.id} due to dependency on ${dep}`);
            projectsToBuild.set(project.id, project);
            added = true;
            break;
          }
        }
      }
    }
  }
  
  if (iteration >= 10) {
    core.warning('Dependency resolution hit iteration limit');
  }
  
  const result = Array.from(projectsToBuild.values());
  core.info(`Total projects to build: ${result.length}`);
  result.forEach(p => core.info(`  - ${p.id}`));
  
  return result;
}

function topologicalSort(projects) {
  core.info('Performing topological sort...');
  const visited = new Map();
  const result = [];
  const projectMap = new Map(projects.map(p => [p.id, p]));
  
  function visit(project) {
    const state = visited.get(project.id);
    if (state === 'temp') {
      throw new Error(`Circular dependency detected at ${project.id}`);
    }
    if (state === 'perm') return;
    
    visited.set(project.id, 'temp');
    
    if (project.dependsOn && Array.isArray(project.dependsOn)) {
      for (const depId of project.dependsOn) {
        const dep = projectMap.get(depId);
        if (dep) {
          visit(dep);
        } else {
          core.info(`Dependency ${depId} not in build set, skipping`);
        }
      }
    }
    
    visited.set(project.id, 'perm');
    result.push(project);
  }
  
  for (const project of projects) {
    if (!visited.has(project.id)) {
      visit(project);
    }
  }
  
  core.info(`Sorted ${result.length} projects`);
  result.forEach((p, i) => core.info(`  ${i + 1}. ${p.id}`));
  
  return result;
}

async function main() {
  try {
    const monoConfigPath = core.getInput('mono-config-path') || 'mono.json';
    
    // Check if mono.json exists
    if (!fs.existsSync(monoConfigPath)) {
      throw new Error(`Mono config file not found: ${monoConfigPath}`);
    }

    const monoConfig = JSON.parse(fs.readFileSync(monoConfigPath, 'utf8'));
    
    // Validate mono config structure
    if (!monoConfig.projects || !Array.isArray(monoConfig.projects)) {
      throw new Error('Invalid mono.json: missing or invalid projects array');
    }

    const changedFiles = getChangedFiles();
    let projectsToBuild;
    
    if (changedFiles === null) {
      // Build all projects if we can't determine changes
      core.warning('Building all projects due to inability to determine changes');
      projectsToBuild = new Map();
      for (const project of monoConfig.projects) {
        if (project.build !== false) {
          projectsToBuild.set(project.id, project);
        }
      }
    } else {
      projectsToBuild = findChangedProjects(changedFiles, monoConfig);
    }
    
    const sortedProjects = topologicalSort(projectsToBuild);
    
    const matrix = {
      include: sortedProjects.map(p => ({
        id: p.id,
        path: p.path,
        type: p.type,
        test: p.test || false,
        testPath: p.testPath || p.path,
        deploySteps: p.deploySteps || [],
        dependsOn: p.dependsOn || []
      }))
    };
    
    const matrixJson = JSON.stringify(matrix);
    core.info(`Generated matrix: ${matrixJson}`);
    core.setOutput('matrix', matrixJson);
    
    // Also set a summary
    await core.summary
      .addHeading('Build Matrix Generated')
      .addTable([
        [{data: 'Project ID', header: true}, {data: 'Type', header: true}, {data: 'Path', header: true}],
        ...sortedProjects.map(p => [p.id, p.type, p.path])
      ])
      .write();
      
  } catch (error) {
    core.setFailed(`Failed to generate build matrix: ${error.message}`);
    core.error(error.stack || error.toString());
  }
}

// Only run if this is the main module
if (require.main === module) {
  main();
}

module.exports = { getChangedFiles, findChangedProjects, topologicalSort };
