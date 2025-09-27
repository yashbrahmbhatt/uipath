using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data;
using System.Diagnostics;
using Yash.Utility.Services;

namespace Yash.Utility.Test.Services.EmailHelperService
{
    [TestClass]
    public class ConstructorAndLoggingTests : EmailHelperServiceTestBase
    {
        [TestMethod]
        public void Constructor_WithNullLogger_ShouldNotThrow()
        {
            // Act & Assert
            var action = () => new Yash.Utility.Services.EmailHelperService(null);
            action.Should().NotThrow();
        }

        [TestMethod]
        public void Constructor_WithLogger_ShouldUseProvidedLogger()
        {
            // Arrange
            var mockLogger = new Mock<Action<string, TraceEventType>>();
            var service = new Yash.Utility.Services.EmailHelperService(mockLogger.Object);
            var dataTable = new DataTable();
            dataTable.Columns.Add("Test", typeof(string));

            // Act
            service.ToHtmlTable(dataTable);

            // Assert
            mockLogger.Verify(
                x => x.Invoke(
                    It.Is<string>(s => s.Contains("[EmailHelperService]")),
                    It.IsAny<TraceEventType>()),
                Times.AtLeastOnce);
        }

        [TestMethod]
        public void _service_WithNullLogger_ShouldNotThrowOnLogging()
        {
            // Arrange
            var service = new Yash.Utility.Services.EmailHelperService(null);
            var dataTable = new DataTable();
            dataTable.Columns.Add("Test", typeof(string));

            // Act & Assert
            var action = () => service.ToHtmlTable(dataTable);
            action.Should().NotThrow();
        }
    }
}