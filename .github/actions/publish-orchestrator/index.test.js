// Mock dependencies first, before any imports
jest.mock('child_process');
jest.mock('@actions/core');
jest.mock('fs');

describe('Publish Orchestrator Action - Full Tests', () => {
  let core, fs, execSync, main;
  
  beforeAll(() => {
    // Import after mocking
    core = require('@actions/core');
    fs = require('fs');
    execSync = require('child_process').execSync;
    main = require('./index').main;
  });

  beforeEach(() => {
    jest.clearAllMocks();
    // Mock core functions to avoid console output during tests
    core.info = jest.fn();
    core.warning = jest.fn();
    core.error = jest.fn();
    core.setFailed = jest.fn();
    core.summary = {
      addHeading: jest.fn().mockReturnThis(),
      addTable: jest.fn().mockReturnThis(),
      write: jest.fn().mockResolvedValue()
    };
    
    // Mock fs.existsSync to return true by default
    fs.existsSync = jest.fn().mockReturnValue(true);
    
    // Mock execSync to succeed by default
    execSync.mockReturnValue('Success');
  });

  describe('Input validation', () => {
    test('should fail when package-path is missing', async () => {
      core.getInput = jest.fn((name, options) => {
        if (name === 'package-path' && options?.required) {
          throw new Error('Input required and not supplied: package-path');
        }
        if (name === 'orchestrator-url') return 'https://test.orchestrator.com';
        if (name === 'orchestrator-tenant') return 'TestTenant';
        return '';
      });

      await main();

      expect(core.setFailed).toHaveBeenCalledWith(
        expect.stringContaining('package-path')
      );
    });

    test('should fail when package file does not exist', async () => {
      core.getInput = jest.fn((name) => {
        if (name === 'package-path') return '/path/to/missing.nupkg';
        if (name === 'orchestrator-url') return 'https://test.orchestrator.com';
        if (name === 'orchestrator-tenant') return 'TestTenant';
        if (name === 'orchestrator-username') return 'testuser';
        if (name === 'orchestrator-password') return 'testpass';
        return '';
      });

      fs.existsSync.mockReturnValue(false);

      await main();

      expect(core.setFailed).toHaveBeenCalledWith(
        expect.stringContaining('Package file or folder not found')
      );
    });

    test('should fail when no authentication method is provided', async () => {
      core.getInput = jest.fn((name) => {
        if (name === 'package-path') return '/path/to/package.nupkg';
        if (name === 'orchestrator-url') return 'https://test.orchestrator.com';
        if (name === 'orchestrator-tenant') return 'TestTenant';
        return '';
      });

      await main();

      expect(core.setFailed).toHaveBeenCalledWith(
        expect.stringContaining('Authentication required')
      );
    });
  });

  describe('Authentication methods', () => {
    const baseInputs = {
      'package-path': '/path/to/package.nupkg',
      'orchestrator-url': 'https://test.orchestrator.com',
      'orchestrator-tenant': 'TestTenant'
    };

    test('should use username/password authentication', async () => {
      core.getInput = jest.fn((name) => {
        const inputs = {
          ...baseInputs,
          'orchestrator-username': 'testuser',
          'orchestrator-password': 'testpass'
        };
        return inputs[name] || '';
      });

      await main();

      expect(execSync).toHaveBeenCalledWith(
        expect.stringContaining('-u "testuser" -p "testpass"'),
        expect.any(Object)
      );
      expect(core.info).toHaveBeenCalledWith('Package published successfully to UiPath Orchestrator');
    });

    test('should use token authentication', async () => {
      core.getInput = jest.fn((name) => {
        const inputs = {
          ...baseInputs,
          'auth-token': 'test-token',
          'account-name': 'test-account'
        };
        return inputs[name] || '';
      });

      await main();

      expect(execSync).toHaveBeenCalledWith(
        expect.stringContaining('-t "test-token" -a "test-account"'),
        expect.any(Object)
      );
    });
  });

  describe('Command building', () => {
    test('should build correct basic command', async () => {
      core.getInput = jest.fn((name) => {
        const inputs = {
          'package-path': '/path/to/package.nupkg',
          'orchestrator-url': 'https://test.orchestrator.com',
          'orchestrator-tenant': 'TestTenant',
          'orchestrator-username': 'testuser',
          'orchestrator-password': 'testpass'
        };
        return inputs[name] || '';
      });

      await main();

      expect(execSync).toHaveBeenCalledWith(
        'uipcli package deploy "/path/to/package.nupkg" "https://test.orchestrator.com" "TestTenant" -u "testuser" -p "testpass"',
        expect.any(Object)
      );
    });
  });
});
