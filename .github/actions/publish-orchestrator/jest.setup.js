// Mock fs.promises before @actions/core is imported
const fs = require('fs');
fs.promises = {
  access: jest.fn(),
  writeFile: jest.fn(),
  readFile: jest.fn(),
  mkdir: jest.fn()
};
