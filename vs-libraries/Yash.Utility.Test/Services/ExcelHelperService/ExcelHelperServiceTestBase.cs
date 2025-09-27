using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics;
using System.IO;

namespace Yash.Utility.Test.Services.ExcelHelperService
{
    [TestClass]
    public abstract class ExcelHelperServiceTestBase
    {
        protected Utility.Services.Excel.ExcelHelperService _service;
        protected Mock<Action<string, TraceEventType>> _mockLogger;
        protected string _testDirectory;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _mockLogger = new Mock<Action<string, TraceEventType>>();
            _service = new Yash.Utility.Services.ExcelHelperService(_mockLogger.Object);
            _testDirectory = Path.Combine(Path.GetTempPath(), "ExcelHelperServiceTests", Guid.NewGuid().ToString());
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