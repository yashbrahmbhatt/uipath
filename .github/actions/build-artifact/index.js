const core = require('@actions/core');
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');
const glob = require('glob');

async function buildVSProject(projectPath, version) {
  core.info('Building .NET project...');
  
  // Find .csproj file
  const csprojFiles = glob.sync('**/*.csproj', { cwd: projectPath });
  if (csprojFiles.length === 0) {
    throw new Error('No .csproj file found in project directory');
  }
  
  const csprojPath = path.join(projectPath, csprojFiles[0]);
  core.info(`Found project file: ${csprojPath}`);
  
  // Restore dependencies
  core.info('Restoring dependencies...');
  execSync(`dotnet restore "${csprojPath}"`, { stdio: 'inherit' });
  
  // Build project
  core.info('Building project...');
  execSync(`dotnet build "${csprojPath}" --configuration Release --no-restore`, { stdio: 'inherit' });
  
  // Pack project
  core.info('Packing project...');
  execSync(`dotnet pack "${csprojPath}" --configuration Release --no-build -p:PackageVersion=${version} --output "${projectPath}"`, { stdio: 'inherit' });
  
  // Find generated package
  const nupkgFiles = glob.sync('*.nupkg', { cwd: projectPath });
  if (nupkgFiles.length === 0) {
    throw new Error('No .nupkg file generated');
  }
  
  return path.join(projectPath, nupkgFiles[0]);
}

async function buildUiPathProject(projectPath, projectId) {
  core.info('Building UiPath project...');
  
  // Check if uipcli is available
  try {
    const versionOutput = execSync('uipcli --version', { stdio: 'pipe', encoding: 'utf8' });
    core.info(`UiPath CLI version: ${versionOutput.trim()}`);
  } catch (error) {
    throw new Error('UiPath CLI (uipcli) is not available. Please install UiPath CLI.');
  }
  
  // Find project.json file to verify this is a UiPath project
  const projectJsonPath = path.join(projectPath, 'project.json');
  if (!fs.existsSync(projectJsonPath)) {
    throw new Error(`No project.json file found in ${projectPath}. This may not be a valid UiPath project.`);
  }
  
  // Read and validate project.json
  try {
    const projectData = JSON.parse(fs.readFileSync(projectJsonPath, 'utf8'));
    core.info(`Project name: ${projectData.name || 'Unknown'}`);
    core.info(`Project version: ${projectData.projectVersion || 'Unknown'}`);
    
    // Check for problematic characters in project path that might cause URI issues
    if (projectPath.includes('%') || projectPath.includes('#') || projectPath.includes('?')) {
      core.warning('Project path contains characters that may cause URI format issues with UiPath CLI');
    }
  } catch (parseError) {
    core.warning(`Could not parse project.json: ${parseError.message}`);
  }
  
  core.info(`Found UiPath project.json: ${projectJsonPath}`);
  
  // List files before packing for debugging
  core.info('Files in project directory before packing:');
  const filesBefore = fs.readdirSync(projectPath);
  filesBefore.forEach(file => core.info(`  - ${file}`));
  
  // Pack UiPath project with correct syntax
  core.info(`Packing UiPath project: ${projectId} from ${projectPath}`);
  
  try {
    // Use the correct UiPath CLI syntax: uipcli package pack <project_path> -o <output_path>
    // Try with absolute paths to avoid URI format issues
    const absoluteProjectPath = path.resolve(projectPath);
    const packCommand = `uipcli package pack "${absoluteProjectPath}" -o "${absoluteProjectPath}"`;
    core.info(`Executing command: ${packCommand}`);
    const output = execSync(packCommand, { stdio: 'pipe', encoding: 'utf8', cwd: projectPath });
    core.info(`UiPath CLI output: ${output}`);
  } catch (firstError) {
    core.warning(`Pack command failed: ${firstError.message}`);
    core.info(`Error output: ${firstError.stdout || ''}`);
    core.info(`Error stderr: ${firstError.stderr || ''}`);
    
    // Check if this is a known URI format issue and try a workaround
    if (firstError.message.includes('Invalid URI') || firstError.message.includes('format of the URI could not be determined')) {
      core.warning('Detected URI format issue. This may be due to special characters in the path or UiPath CLI version compatibility.');
      
      // Try with a different output directory to avoid path issues
      try {
        const tempOutputDir = path.join(process.cwd(), 'temp-uipath-output');
        if (!fs.existsSync(tempOutputDir)) {
          fs.mkdirSync(tempOutputDir, { recursive: true });
        }
        
        const projectJsonPath = path.join(projectPath, 'project.json');
        const altCommand = `uipcli package pack "${projectJsonPath}" -o "${tempOutputDir}"`;
        core.info(`Trying with temp output directory: ${altCommand}`);
        const output = execSync(altCommand, { stdio: 'pipe', encoding: 'utf8' });
        core.info(`UiPath CLI output: ${output}`);
        
        // Move the generated package back to the project directory
        const tempNupkgFiles = glob.sync('*.nupkg', { cwd: tempOutputDir });
        if (tempNupkgFiles.length > 0) {
          const sourcePath = path.join(tempOutputDir, tempNupkgFiles[0]);
          const destPath = path.join(projectPath, tempNupkgFiles[0]);
          fs.copyFileSync(sourcePath, destPath);
          core.info(`Moved package from ${sourcePath} to ${destPath}`);
          // Clean up temp directory
          fs.rmSync(tempOutputDir, { recursive: true, force: true });
        }
      } catch (secondError) {
        throw new Error(`UiPath CLI pack failed with URI format error. This may be due to UiPath CLI version compatibility or project configuration issues. Original error: ${firstError.message}. Fallback error: ${secondError.message}`);
      }
    } else {
      throw new Error(`UiPath CLI pack failed. Error: ${firstError.message}. Stderr: ${firstError.stderr || ''}`);
    }
  }
  
  // List files after packing for debugging
  core.info('Files in project directory after packing:');
  const filesAfter = fs.readdirSync(projectPath);
  filesAfter.forEach(file => core.info(`  - ${file}`));
  
  // Find generated package
  const nupkgFiles = glob.sync('*.nupkg', { cwd: projectPath });
  if (nupkgFiles.length === 0) {
    // Also check for packages in a dist or output folder
    const distPath = path.join(projectPath, 'dist');
    const outputPath = path.join(projectPath, 'output');
    
    let altNupkgFiles = [];
    if (fs.existsSync(distPath)) {
      altNupkgFiles = glob.sync('*.nupkg', { cwd: distPath });
      if (altNupkgFiles.length > 0) {
        return path.join(distPath, altNupkgFiles[0]);
      }
    }
    
    if (fs.existsSync(outputPath)) {
      altNupkgFiles = glob.sync('*.nupkg', { cwd: outputPath });
      if (altNupkgFiles.length > 0) {
        return path.join(outputPath, altNupkgFiles[0]);
      }
    }
    
    throw new Error('No .nupkg file generated by UiPath CLI');
  }
  
  return path.join(projectPath, nupkgFiles[0]);
}

async function main() {
  try {
    const projectId = core.getInput('project-id', { required: true });
    const projectPath = core.getInput('project-path', { required: true });
    const projectType = core.getInput('project-type', { required: true });
    const version = core.getInput('version', { required: true });
    const fullVersion = core.getInput('full-version', { required: true });
    
    core.info(`Building project: ${projectId}`);
    core.info(`Project path: ${projectPath}`);
    core.info(`Project type: ${projectType}`);
    core.info(`Version: ${version}`);
    
    // Verify project path exists
    if (!fs.existsSync(projectPath)) {
      throw new Error(`Project path does not exist: ${projectPath}`);
    }
    
    let packagePath;
    
    // Build based on project type
    if (projectType.startsWith('vs')) {
      packagePath = await buildVSProject(projectPath, version);
    } else if (projectType.startsWith('uipath')) {
      packagePath = await buildUiPathProject(projectPath, projectId);
    } else {
      throw new Error(`Unsupported project type: ${projectType}`);
    }
    
    core.info(`Package generated: ${packagePath}`);
    
    // Upload artifact using actions/upload-artifact equivalent
    // Since we can't directly call other actions from JavaScript, we'll output the necessary info
    core.setOutput('package-path', packagePath);
    core.setOutput('artifact-name', fullVersion);
    
    // Create a summary (only in GitHub Actions environment)
    if (process.env.GITHUB_STEP_SUMMARY) {
      await core.summary
        .addHeading('Build Complete')
        .addTable([
          [{data: 'Property', header: true}, {data: 'Value', header: true}],
          ['Project ID', projectId],
          ['Project Type', projectType],
          ['Version', version],
          ['Package Path', packagePath],
          ['Artifact Name', fullVersion]
        ])
        .write();
    } else {
      core.info("Skipping GitHub Actions summary (not in GitHub Actions environment)");
    }
      
  } catch (error) {
    core.setFailed(`Build failed: ${error.message}`);
    core.error(error.stack || error.toString());
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };
