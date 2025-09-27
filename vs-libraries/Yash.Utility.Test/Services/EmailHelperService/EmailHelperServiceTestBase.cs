using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Yash.Utility.Test.Services.EmailHelperService
{
    [TestClass]
    public abstract class EmailHelperServiceTestBase
    {
        protected Utility.Services.Email.EmailHelperService _service;
        protected Mock<Action<string, TraceEventType>> _mockLogger;
        protected string _testDirectory;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _mockLogger = new Mock<Action<string, TraceEventType>>();
            _service = new Yash.Utility.Services.EmailHelperService(_mockLogger.Object);
            _testDirectory = Path.Combine(Path.GetTempPath(), "EmailHelperServiceTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
    }
}