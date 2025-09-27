using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Yash.Utility.Services;

namespace Yash.Utility.Test.Services.ExcelHelperService
{
    [TestClass]
    public class ConstructorAndLoggingTests : ExcelHelperServiceTestBase
    {
        [TestMethod]
        public void Constructor_WithNullLogger_ShouldNotThrow()
        {
            // Act & Assert
            var action = () => new Yash.Utility.Services.ExcelHelperService(null);
            action.Should().NotThrow();
        }

        [TestMethod]
        public void Constructor_WithLogger_ShouldUseProvidedLogger()
        {
            // Arrange
            var mockLogger = new Mock<Action<string, TraceEventType>>();
            var service = new Yash.Utility.Services.ExcelHelperService(mockLogger.Object);
            var outputPath = Path.Combine(_testDirectory, "test.xlsx");

            // Act
            service.CreateExcelFile(outputPath, new Dictionary<string, string[]>
            {
                { "Sheet1", new[] { "Column1" } }
            });

            // Assert
            mockLogger.Verify(
                x => x.Invoke(
                    It.Is<string>(s => s.Contains("[ExcelHelpers]")),
                    It.IsAny<TraceEventType>()),
                Times.AtLeastOnce);
        }
    }
}