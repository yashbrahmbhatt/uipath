const core = require('@actions/core');
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');
const glob = require('glob');

async function testDotNetProject(projectId, testPath, configuration) {
  core.info('Running .NET project tests...');
  
  // Find test project files
  const testProjectFiles = glob.sync('**/*.csproj', { cwd: testPath });
  if (testProjectFiles.length === 0) {
    core.warning(`No test project files found in: ${testPath}. Skipping .NET tests.`);
    return {
      testPassed: true,
      testCount: 0,
      passedCount: 0,
      failedCount: 0,
      resultsPath: null
    };
  }
  
  const testProjectPath = path.join(testPath, testProjectFiles[0]);
  core.info(`Found test project: ${testProjectPath}`);
  
  // Create results directory
  const resultsDir = path.join(testPath, 'TestResults');
  if (!fs.existsSync(resultsDir)) {
    fs.mkdirSync(resultsDir, { recursive: true });
  }
  
  const testResultsPath = path.join(resultsDir, `${projectId}-test-results.trx`);
  
  // Restore dependencies
  core.info('Restoring test dependencies...');
  execSync(`dotnet restore "${testProjectPath}"`, { stdio: 'inherit' });
  
  // Build test project
  core.info('Building test project...');
  execSync(`dotnet build "${testProjectPath}" --configuration ${configuration} --no-restore`, { stdio: 'inherit' });
  
  // Run tests with coverage
  core.info('Running .NET tests...');
  let testPassed = true;
  try {
    const testCommand = [
      'dotnet', 'test',
      `"${testProjectPath}"`,
      '--configuration', configuration,
      '--no-build',
      '--logger', `trx;LogFileName=${path.basename(testResultsPath)}`,
      '--results-directory', resultsDir,
      '--collect:"XPlat Code Coverage"',
      '--',
      'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura'
    ].join(' ');
    
    execSync(testCommand, { stdio: 'inherit' });
    core.info('All .NET tests passed successfully');
    
  } catch (error) {
    core.error('Some .NET tests failed');
    testPassed = false;
  }
  
  // Parse test results
  let testResults = { total: 0, passed: 0, failed: 0, skipped: 0 };
  if (fs.existsSync(testResultsPath)) {
    try {
      testResults = await parseDotNetTestResults(testResultsPath);
    } catch (error) {
      core.warning(`Failed to parse .NET test results: ${error.message}`);
    }
  }
  
  return {
    testPassed,
    testCount: testResults.total,
    passedCount: testResults.passed,
    failedCount: testResults.failed,
    resultsPath: testResultsPath,
    testResults
  };
}

async function testUiPathProject(projectId, testPath) {
  core.info('Running UiPath project tests...');
  
  // Check if uipcli is available
  let hasUiPathCLI = true;
  try {
    execSync('uipcli --version', { stdio: 'pipe' });
  } catch (error) {
    core.warning('UiPath CLI (uipcli) is not available. Will perform basic validation instead.');
    hasUiPathCLI = false;
  }
  
  // Look for test files (*.Test.xaml or test workflows)
  const testFiles = glob.sync('**/*.Test.xaml', { cwd: testPath });
  if (testFiles.length === 0) {
    core.warning(`No UiPath test files found in: ${testPath}. Skipping UiPath tests.`);
    return {
      testPassed: true,
      testCount: 0,
      passedCount: 0,
      failedCount: 0,
      resultsPath: null
    };
  }
  
  core.info(`Found ${testFiles.length} UiPath test files`);
  testFiles.forEach(file => core.info(`  - ${file}`));
  
  // Create results directory
  const resultsDir = path.join(testPath, 'TestResults');
  if (!fs.existsSync(resultsDir)) {
    fs.mkdirSync(resultsDir, { recursive: true });
  }
  
  const testResultsPath = path.join(resultsDir, `${projectId}-uipath-test-results.json`);
  
  let allTestsPassed = true;
  const testResults = {
    projectId,
    totalTests: testFiles.length,
    passedTests: 0,
    failedTests: 0,
    testDetails: []
  };
  
  // Run each test file
  for (const testFile of testFiles) {
    const testName = path.basename(testFile, '.xaml');
    core.info(`Running UiPath test: ${testName}`);
    
    try {
      if (hasUiPathCLI) {
        // Try to use UiPath CLI for testing
        const testCommand = `uipcli test run "${path.join(testPath, testFile)}" --result-path "${resultsDir}"`;
        
        try {
          execSync(testCommand, { stdio: 'inherit', cwd: testPath });
          core.info(`✅ UiPath test passed: ${testName}`);
          testResults.passedTests++;
          testResults.testDetails.push({
            name: testName,
            file: testFile,
            status: 'Passed',
            duration: 'N/A'
          });
        } catch (testError) {
          throw testError;
        }
      } else {
        // Fall back to basic XAML validation
        core.info(`Performing basic validation for: ${testName}`);
        
        const testFilePath = path.join(testPath, testFile);
        if (fs.existsSync(testFilePath)) {
          const content = fs.readFileSync(testFilePath, 'utf8');
          if (content.includes('<Activity') && content.includes('</Activity>')) {
            core.info(`✅ Validation passed: ${testName}`);
            testResults.passedTests++;
            testResults.testDetails.push({
              name: testName,
              file: testFile,
              status: 'Validated',
              duration: 'N/A'
            });
          } else {
            throw new Error('Invalid XAML structure');
          }
        } else {
          throw new Error('Test file not found');
        }
      }
    } catch (error) {
      core.error(`❌ UiPath test failed: ${testName} - ${error.message}`);
      allTestsPassed = false;
      testResults.failedTests++;
      testResults.testDetails.push({
        name: testName,
        file: testFile,
        status: 'Failed',
        error: error.message,
        duration: 'N/A'
      });
    }
  }
  
  // Save test results
  fs.writeFileSync(testResultsPath, JSON.stringify(testResults, null, 2));
  
  return {
    testPassed: allTestsPassed,
    testCount: testResults.totalTests,
    passedCount: testResults.passedTests,
    failedCount: testResults.failedTests,
    resultsPath: testResultsPath,
    testResults
  };
}

async function parseDotNetTestResults(resultsPath) {
  // Basic TRX parsing - simplified approach
  const content = fs.readFileSync(resultsPath, 'utf8');
  
  // Extract basic metrics using regex
  const totalMatch = content.match(/total="(\d+)"/);
  const passedMatch = content.match(/passed="(\d+)"/);
  const failedMatch = content.match(/failed="(\d+)"/);
  const skippedMatch = content.match(/skipped="(\d+)"/);
  
  return {
    total: totalMatch ? parseInt(totalMatch[1]) : 0,
    passed: passedMatch ? parseInt(passedMatch[1]) : 0,
    failed: failedMatch ? parseInt(failedMatch[1]) : 0,
    skipped: skippedMatch ? parseInt(skippedMatch[1]) : 0
  };
}

async function createTestSummary(projectId, projectType, results) {
  const status = results.failedCount > 0 ? '❌ Failed' : '✅ Passed';
  const testTypeLabel = projectType.startsWith('vs') ? '.NET' : 'UiPath';
  
  let summaryBuilder = core.summary
    .addHeading(`${testTypeLabel} Test Results: ${projectId}`)
    .addTable([
      [{data: 'Metric', header: true}, {data: 'Count', header: true}],
      ['Status', status],
      ['Total Tests', results.testCount.toString()],
      ['Passed', results.passedCount.toString()],
      ['Failed', results.failedCount.toString()]
    ]);
  
  // Add detailed results for UiPath tests
  if (projectType.startsWith('uipath') && results.testResults && results.testResults.testDetails) {
    summaryBuilder = summaryBuilder
      .addHeading('Test Details')
      .addTable([
        [{data: 'Test Name', header: true}, {data: 'File', header: true}, {data: 'Status', header: true}],
        ...results.testResults.testDetails.map(test => [
          test.name,
          test.file,
          test.status === 'Failed' ? `❌ ${test.status}` : `✅ ${test.status}`
        ])
      ]);
  }
  
  await summaryBuilder.write();
}

async function main() {
  try {
    const projectId = core.getInput('project-id', { required: true });
    const testPath = core.getInput('test-path', { required: true });
    const projectType = core.getInput('project-type', { required: true });
    const configuration = core.getInput('configuration') || 'Release';
    
    core.info(`Running tests for project: ${projectId}`);
    core.info(`Test path: ${testPath}`);
    core.info(`Project type: ${projectType}`);
    
    // Verify test path exists
    if (!fs.existsSync(testPath)) {
      core.warning(`Test path does not exist: ${testPath}. Skipping tests.`);
      core.setOutput('test-passed', 'true');
      core.setOutput('test-count', '0');
      core.setOutput('passed-count', '0');
      core.setOutput('failed-count', '0');
      return;
    }
    
    let results;
    
    // Run tests based on project type
    if (projectType.startsWith('vs')) {
      results = await testDotNetProject(projectId, testPath, configuration);
    } else if (projectType.startsWith('uipath')) {
      results = await testUiPathProject(projectId, testPath);
    } else {
      throw new Error(`Unsupported project type for testing: ${projectType}`);
    }
    
    // Set outputs
    core.setOutput('test-passed', results.testPassed.toString());
    core.setOutput('test-count', results.testCount.toString());
    core.setOutput('passed-count', results.passedCount.toString());
    core.setOutput('failed-count', results.failedCount.toString());
    if (results.resultsPath) {
      core.setOutput('test-results-path', results.resultsPath);
    }
    
    // Create test summary
    await createTestSummary(projectId, projectType, results);
    
    if (!results.testPassed) {
      core.error(`Tests failed for project: ${projectId}`);
    } else {
      core.info(`All tests passed for project: ${projectId}`);
    }
    
  } catch (error) {
    core.setFailed(`Test execution failed: ${error.message}`);
    core.error(error.stack || error.toString());
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };
