const { execSync } = require('child_process');
const { getChangedFiles, findChangedProjects, topologicalSort } = require('./index');

// Mock dependencies
jest.mock('child_process');
jest.mock('@actions/core');
jest.mock('@actions/github');

const core = require('@actions/core');
const github = require('@actions/github');

describe('Build Matrix Functions', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Mock core.info to avoid console output during tests
    core.info = jest.fn();
    core.warning = jest.fn();
    core.error = jest.fn();
  });

  describe('getChangedFiles', () => {
    beforeEach(() => {
      // Reset environment variables
      delete process.env.GITHUB_EVENT_BEFORE;
      delete process.env.GITHUB_SHA;
    });

    test('should detect changed files using commit range when available', () => {
      github.context = {
        payload: { before: 'abc123' },
        sha: 'def456'
      };
      
      execSync.mockReturnValue('file1.txt\nfile2.js\nsrc/component.ts\n');

      const result = getChangedFiles();

      expect(execSync).toHaveBeenCalledWith('git diff --name-only abc123 def456', { encoding: 'utf8' });
      expect(result).toEqual(['file1.txt', 'file2.js', 'src/component.ts']);
    });

    test('should fallback to HEAD~1 when before is null commit', () => {
      github.context = {
        payload: { before: '0000000000000000000000000000000000000000' },
        sha: 'def456'
      };
      
      execSync.mockReturnValue('package.json\nREADME.md\n');

      const result = getChangedFiles();

      expect(execSync).toHaveBeenCalledWith('git diff --name-only HEAD~1 HEAD', { encoding: 'utf8' });
      expect(result).toEqual(['package.json', 'README.md']);
    });

    test('should use environment variables when context is unavailable', () => {
      github.context = { payload: {}, sha: null };
      process.env.GITHUB_EVENT_BEFORE = 'env123';
      process.env.GITHUB_SHA = 'env456';
      
      execSync.mockReturnValue('env-file.txt\n');

      const result = getChangedFiles();

      expect(execSync).toHaveBeenCalledWith('git diff --name-only env123 env456', { encoding: 'utf8' });
      expect(result).toEqual(['env-file.txt']);
    });

    test('should handle git command failures with fallbacks', () => {
      github.context = { payload: { before: 'bad123' }, sha: 'bad456' };
      
      execSync
        .mockImplementationOnce(() => { throw new Error('Invalid commit range'); })
        .mockImplementationOnce(() => 'fallback1.txt\nfallback2.js\n');

      const result = getChangedFiles();

      expect(execSync).toHaveBeenCalledWith('git diff --name-only bad123 bad456', { encoding: 'utf8' });
      expect(execSync).toHaveBeenCalledWith('git diff --name-only HEAD~1 HEAD', { encoding: 'utf8' });
      expect(result).toEqual(['fallback1.txt', 'fallback2.js']);
    });

    test('should try origin/main fallback when HEAD~1 fails', () => {
      github.context = { payload: {}, sha: null };
      
      execSync
        .mockImplementationOnce(() => { throw new Error('No HEAD~1'); })
        .mockImplementationOnce(() => { throw new Error('No HEAD~1'); })
        .mockImplementationOnce(() => 'origin-file.txt\n');

      const result = getChangedFiles();

      expect(execSync).toHaveBeenCalledWith('git diff --name-only HEAD~1 HEAD', { encoding: 'utf8' });
      expect(execSync).toHaveBeenCalledWith('git diff --name-only HEAD~1 HEAD', { encoding: 'utf8' });
      expect(execSync).toHaveBeenCalledWith('git diff --name-only origin/main HEAD', { encoding: 'utf8' });
      expect(result).toEqual(['origin-file.txt']);
    });

    test('should return null when all git commands fail', () => {
      github.context = { payload: {}, sha: null };
      
      execSync.mockImplementation(() => { throw new Error('Git not available'); });

      const result = getChangedFiles();

      expect(result).toBeNull();
      expect(core.warning).toHaveBeenCalledWith('Could not determine changed files, will build all projects');
    });

    test('should filter out empty lines from git output', () => {
      github.context = { payload: { before: 'abc123' }, sha: 'def456' };
      
      execSync.mockReturnValue('file1.txt\n\nfile2.js\n\n\nfile3.ts\n');

      const result = getChangedFiles();

      expect(result).toEqual(['file1.txt', 'file2.js', 'file3.ts']);
    });
  });

  describe('findChangedProjects', () => {
    const realMonoConfig = {
      "projects": [
        {
          "id": "Yash.RBC.Activities",
          "path": "uipath-libraries/Yash.RBC.Activities",
          "type": "uipath-library",
          "build": true,
          "test": false,
          "testPath": "uipath-libraries/Yash.RBC.Activities",
          "deploy": true,
          "deploySteps": ["orchestrator", "nuget"],
          "dependsOn": []
        },
        {
          "id": "Finance.IngestTransactions",
          "path": "uipath-processes/Finance.IngestTransactions",
          "type": "uipath-process",
          "build": true,
          "test": false,
          "testPath": "uipath-processes/Finance.IngestTransactions",
          "deploy": true,
          "deploySteps": ["orchestrator"],
          "dependsOn": []
        },
        {
          "id": "Yash.Config",
          "path": "vs-libraries/Yash.Config",
          "type": "vs",
          "build": true,
          "test": true,
          "testPath": "vs-libraries/Yash.Config.Tests",
          "deploy": true,
          "deploySteps": ["nuget"],
          "dependsOn": ["Yash.Orchestrator"]
        },
        {
          "id": "Yash.Orchestrator",
          "path": "vs-libraries/Yash.Orchestrator",
          "type": "vs",
          "build": true,
          "test": true,
          "testPath": "vs-libraries/Yash.Orchestrator.Tests",
          "deploy": true,
          "deploySteps": ["nuget"],
          "dependsOn": []
        }
      ]
    };

    test('should return empty array when changedFiles is null', () => {
      const result = findChangedProjects(null, realMonoConfig);

      expect(result).toHaveLength(0);
      expect(core.warning).toHaveBeenCalledWith('No changes detected - skipping all builds');
    });

    test('should detect UiPath library changes', () => {
      const changedFiles = ['uipath-libraries/Yash.RBC.Activities/Utilities.cs'];
      
      const result = findChangedProjects(changedFiles, realMonoConfig);

      const projectIds = result.map(p => p.id);
      expect(projectIds).toContain('Yash.RBC.Activities');
      expect(result).toHaveLength(1); // No other projects depend on this
    });

    test('should detect VS library changes and include dependents', () => {
      const changedFiles = ['vs-libraries/Yash.Orchestrator/Service.cs'];
      
      const result = findChangedProjects(changedFiles, realMonoConfig);

      const projectIds = result.map(p => p.id);
      expect(projectIds).toContain('Yash.Orchestrator'); // directly changed
      expect(projectIds).toContain('Yash.Config'); // depends on Yash.Orchestrator
      expect(result).toHaveLength(2);
    });

    test('should handle UiPath process changes', () => {
      const changedFiles = ['uipath-processes/Finance.IngestTransactions/Main.xaml'];
      
      const result = findChangedProjects(changedFiles, realMonoConfig);

      const projectIds = result.map(p => p.id);
      expect(projectIds).toContain('Finance.IngestTransactions');
      expect(result).toHaveLength(1); // No dependencies
    });

    test('should handle test project changes', () => {
      const changedFiles = ['vs-libraries/Yash.Config.Tests/ConfigTests.cs'];
      
      const result = findChangedProjects(changedFiles, realMonoConfig);

      // Test projects are not in the mono.json, so no builds should be triggered
      expect(result).toHaveLength(0);
    });

    test('should handle exact project path matches', () => {
      const changedFiles = ['vs-libraries/Yash.Config'];
      
      const result = findChangedProjects(changedFiles, realMonoConfig);

      expect(result.map(p => p.id)).toContain('Yash.Config');
    });

    test('should handle multiple project changes', () => {
      const changedFiles = [
        'uipath-libraries/Yash.RBC.Activities/Utilities.cs',
        'vs-libraries/Yash.Orchestrator/Service.cs'
      ];
      
      const result = findChangedProjects(changedFiles, realMonoConfig);

      const projectIds = result.map(p => p.id);
      expect(projectIds).toContain('Yash.RBC.Activities');
      expect(projectIds).toContain('Yash.Orchestrator');
      expect(projectIds).toContain('Yash.Config'); // depends on Yash.Orchestrator
      expect(result).toHaveLength(3);
    });

    test('should not match similar path names', () => {
      const changedFiles = ['vs-libraries/Yash.Config.Extended/SomeFile.cs'];
      
      const result = findChangedProjects(changedFiles, realMonoConfig);

      expect(result.map(p => p.id)).not.toContain('Yash.Config');
      expect(result).toHaveLength(0);
    });

    test('should handle cross-platform path separators with real paths', () => {
      const changedFiles = ['vs-libraries\\Yash.Orchestrator\\Service.cs']; // Windows path
      
      const result = findChangedProjects(changedFiles, realMonoConfig);

      const projectIds = result.map(p => p.id);
      expect(projectIds).toContain('Yash.Orchestrator');
      expect(projectIds).toContain('Yash.Config');
    });

    test('should handle projects without dependsOn array', () => {
      // All projects in real config that don't have dependencies work correctly
      const changedFiles = ['uipath-libraries/Yash.RBC.Activities/Main.xaml'];
      
      const result = findChangedProjects(changedFiles, realMonoConfig);

      expect(result).toHaveLength(1);
      expect(result[0].id).toBe('Yash.RBC.Activities');
    });

    test('should prevent infinite loops in dependency resolution', () => {
      // Create a test config with circular dependencies
      const configWithCircular = {
        projects: [
          { id: 'project-a', path: 'apps/a', type: 'uipath', build: true, dependsOn: ['project-b'] },
          { id: 'project-b', path: 'apps/b', type: 'uipath', build: true, dependsOn: ['project-a'] }
        ]
      };
      const changedFiles = ['apps/a/main.xaml'];
      
      const result = findChangedProjects(changedFiles, configWithCircular);

      expect(result).toHaveLength(2); // Both projects should be included
      // Note: The algorithm resolves this case without hitting the iteration limit
      // because the circular dependency is handled by the dependency graph traversal
    });

    test('should handle iteration limit with complex dependency chains', () => {
      // Create a scenario where dependency resolution takes many iterations
      const projects = [];
      for (let i = 0; i < 15; i++) {
        projects.push({
          id: `project-${i}`,
          path: `apps/project-${i}`,
          type: 'uipath',
          build: true,
          dependsOn: i > 0 ? [`project-${i-1}`] : []
        });
      }
      
      const configWithLongChain = { projects };
      const changedFiles = ['apps/project-0/main.xaml']; // Change the root project
      
      const result = findChangedProjects(changedFiles, configWithLongChain);

      expect(result).toHaveLength(15); // All projects should be included due to long dependency chain
    });
  });

  describe('topologicalSort', () => {
    test('should sort real projects with dependencies correctly', () => {
      const realProjects = [
        {
          "id": "Yash.Config",
          "path": "vs-libraries/Yash.Config",
          "type": "vs",
          "dependsOn": ["Yash.Orchestrator"]
        },
        {
          "id": "Yash.Orchestrator",
          "path": "vs-libraries/Yash.Orchestrator",
          "type": "vs",
          "dependsOn": []
        }
      ];

      const result = topologicalSort(realProjects);

      const ids = result.map(p => p.id);
      expect(ids.indexOf('Yash.Orchestrator')).toBeLessThan(ids.indexOf('Yash.Config'));
      expect(result).toHaveLength(2);
    });

    test('should handle all real projects', () => {
      const allRealProjects = [
        {
          "id": "Yash.RBC.Activities",
          "path": "uipath-libraries/Yash.RBC.Activities",
          "type": "uipath-library",
          "dependsOn": []
        },
        {
          "id": "Finance.IngestTransactions",
          "path": "uipath-processes/Finance.IngestTransactions", 
          "type": "uipath-process",
          "dependsOn": []
        },
        {
          "id": "Yash.Config",
          "path": "vs-libraries/Yash.Config",
          "type": "vs",
          "dependsOn": ["Yash.Orchestrator"]
        },
        {
          "id": "Yash.Orchestrator",
          "path": "vs-libraries/Yash.Orchestrator",
          "type": "vs",
          "dependsOn": []
        }
      ];

      const result = topologicalSort(allRealProjects);

      expect(result).toHaveLength(4);
      const ids = result.map(p => p.id);
      expect(ids.indexOf('Yash.Orchestrator')).toBeLessThan(ids.indexOf('Yash.Config'));
      
      // Independent projects can be in any order
      expect(ids).toContain('Yash.RBC.Activities');
      expect(ids).toContain('Finance.IngestTransactions');
    });

    test('should sort projects with simple dependencies', () => {
      const projects = [
        { id: 'project-b', dependsOn: ['project-a'] },
        { id: 'project-a', dependsOn: [] },
        { id: 'project-c', dependsOn: ['project-b'] }
      ];

      const result = topologicalSort(projects);

      const ids = result.map(p => p.id);
      expect(ids.indexOf('project-a')).toBeLessThan(ids.indexOf('project-b'));
      expect(ids.indexOf('project-b')).toBeLessThan(ids.indexOf('project-c'));
    });

    test('should handle projects with no dependencies', () => {
      const projects = [
        { id: 'project-a' },
        { id: 'project-b' },
        { id: 'project-c' }
      ];

      const result = topologicalSort(projects);

      expect(result).toHaveLength(3);
      expect(result.map(p => p.id)).toEqual(['project-a', 'project-b', 'project-c']);
    });

    test('should handle complex dependency chains', () => {
      const projects = [
        { id: 'project-d', dependsOn: ['project-b', 'project-c'] },
        { id: 'project-c', dependsOn: ['project-a'] },
        { id: 'project-b', dependsOn: ['project-a'] },
        { id: 'project-a', dependsOn: [] }
      ];

      const result = topologicalSort(projects);

      const ids = result.map(p => p.id);
      expect(ids.indexOf('project-a')).toBe(0); // First
      expect(ids.indexOf('project-b')).toBeGreaterThan(ids.indexOf('project-a'));
      expect(ids.indexOf('project-c')).toBeGreaterThan(ids.indexOf('project-a'));
      expect(ids.indexOf('project-d')).toBeGreaterThan(ids.indexOf('project-b'));
      expect(ids.indexOf('project-d')).toBeGreaterThan(ids.indexOf('project-c'));
    });

    test('should detect circular dependencies', () => {
      const projects = [
        { id: 'project-a', dependsOn: ['project-b'] },
        { id: 'project-b', dependsOn: ['project-a'] }
      ];

      expect(() => topologicalSort(projects)).toThrow('Circular dependency detected at project-a');
    });

    test('should handle missing dependencies gracefully', () => {
      const projects = [
        { id: 'project-a', dependsOn: ['missing-project'] },
        { id: 'project-b', dependsOn: ['project-a'] }
      ];

      const result = topologicalSort(projects);

      expect(result).toHaveLength(2);
      expect(core.info).toHaveBeenCalledWith('Dependency missing-project not in build set, skipping');
    });

    test('should handle projects with undefined dependsOn', () => {
      const projects = [
        { id: 'project-a' }, // No dependsOn property
        { id: 'project-b', dependsOn: undefined }, // Undefined dependsOn
        { id: 'project-c', dependsOn: null } // Null dependsOn
      ];

      const result = topologicalSort(projects);

      expect(result).toHaveLength(3);
    });

    test('should handle projects with non-array dependsOn', () => {
      const projects = [
        { id: 'project-a', dependsOn: 'not-an-array' },
        { id: 'project-b', dependsOn: 123 }
      ];

      const result = topologicalSort(projects);

      expect(result).toHaveLength(2);
    });

    test('should throw error for non-array input', () => {
      expect(() => topologicalSort('not-an-array')).toThrow('topologicalSort expects an array but received string');
      expect(() => topologicalSort(null)).toThrow('topologicalSort expects an array but received object');
      expect(() => topologicalSort({})).toThrow('topologicalSort expects an array but received object');
    });

    test('should preserve project properties in sorted result', () => {
      const projects = [
        { id: 'project-b', path: 'apps/b', type: 'uipath', deploy: true, deploySteps: ['orchestrator'], dependsOn: ['project-a'] },
        { id: 'project-a', path: 'apps/a', type: 'vs', deploy: true, deploySteps: ['nuget'], dependsOn: [] }
      ];

      const result = topologicalSort(projects);

      expect(result[0]).toEqual({ id: 'project-a', path: 'apps/a', type: 'vs', deploy: true, deploySteps: ['nuget'], dependsOn: [] });
      expect(result[1]).toEqual({ id: 'project-b', path: 'apps/b', type: 'uipath', deploy: true, deploySteps: ['orchestrator'], dependsOn: ['project-a'] });
    });

    test('should handle complex three-way dependency', () => {
      const projects = [
        { id: 'project-c', dependsOn: ['project-a', 'project-b'] },
        { id: 'project-b', dependsOn: ['project-a'] },
        { id: 'project-a', dependsOn: [] }
      ];

      const result = topologicalSort(projects);

      const ids = result.map(p => p.id);
      expect(ids.indexOf('project-a')).toBe(0);
      expect(ids.indexOf('project-b')).toBe(1);
      expect(ids.indexOf('project-c')).toBe(2);
    });

    test('should handle self-dependency as circular', () => {
      const projects = [
        { id: 'project-a', dependsOn: ['project-a'] }
      ];

      expect(() => topologicalSort(projects)).toThrow('Circular dependency detected at project-a');
    });
  });

  describe('Integration tests', () => {
    test('should work end-to-end with real UiPath project scenario', () => {
      const realMonoConfig = {
        "projects": [
          {
            "id": "Yash.RBC.Activities",
            "path": "uipath-libraries/Yash.RBC.Activities",
            "type": "uipath-library",
            "build": true,
            "dependsOn": []
          },
          {
            "id": "Finance.IngestTransactions",
            "path": "uipath-processes/Finance.IngestTransactions",
            "type": "uipath-process",
            "build": true,
            "dependsOn": []
          },
          {
            "id": "Yash.Config",
            "path": "vs-libraries/Yash.Config",
            "type": "vs",
            "build": true,
            "dependsOn": ["Yash.Orchestrator"]
          },
          {
            "id": "Yash.Orchestrator",
            "path": "vs-libraries/Yash.Orchestrator",
            "type": "vs",
            "build": true,
            "dependsOn": []
          }
        ]
      };

      // Simulate changing the Orchestrator service (which Yash.Config depends on)
      const changedFiles = ['vs-libraries/Yash.Orchestrator/Service.cs'];
      
      const projectsToBuild = findChangedProjects(changedFiles, realMonoConfig);
      const sortedProjects = topologicalSort(projectsToBuild);
      
      expect(sortedProjects).toHaveLength(2); // Orchestrator + Config
      
      const ids = sortedProjects.map(p => p.id);
      expect(ids).toEqual(['Yash.Orchestrator', 'Yash.Config']);
    });

    test('should handle configuration changes triggering all builds', () => {
      // Test the getAllBuildableProjects function directly since that's what handles config changes
      const realMonoConfig = {
        "projects": [
          {
            "id": "Yash.RBC.Activities", 
            "path": "uipath-libraries/Yash.RBC.Activities",
            "type": "uipath-library",
            "build": true,
            "dependsOn": []
          },
          {
            "id": "Yash.Config",
            "path": "vs-libraries/Yash.Config", 
            "type": "vs",
            "build": true,
            "dependsOn": ["Yash.Orchestrator"]
          },
          {
            "id": "Yash.Orchestrator",
            "path": "vs-libraries/Yash.Orchestrator",
            "type": "vs", 
            "build": true,
            "dependsOn": []
          }
        ]
      };

      // Import the new function for testing
      const { getAllBuildableProjects } = require('./index');
      
      const projectsToBuild = getAllBuildableProjects(realMonoConfig);
      const sortedProjects = topologicalSort(projectsToBuild);
      
      expect(sortedProjects).toHaveLength(3);
      
      const ids = sortedProjects.map(p => p.id);
      expect(ids.indexOf('Yash.Orchestrator')).toBeLessThan(ids.indexOf('Yash.Config'));
      expect(ids).toContain('Yash.RBC.Activities');
    });

    test('should handle independent UiPath library changes', () => {
      const realMonoConfig = {
        "projects": [
          {
            "id": "Yash.RBC.Activities",
            "path": "uipath-libraries/Yash.RBC.Activities",
            "type": "uipath-library", 
            "build": true,
            "dependsOn": []
          },
          {
            "id": "Finance.IngestTransactions",
            "path": "uipath-processes/Finance.IngestTransactions",
            "type": "uipath-process",
            "build": true,
            "dependsOn": []
          }
        ]
      };

      const changedFiles = ['uipath-libraries/Yash.RBC.Activities/Utilities.cs'];
      
      const projectsToBuild = findChangedProjects(changedFiles, realMonoConfig);
      const sortedProjects = topologicalSort(projectsToBuild);
      
      expect(sortedProjects).toHaveLength(1);
      expect(sortedProjects[0].id).toBe('Yash.RBC.Activities');
    });
  });
});
