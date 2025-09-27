using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data;
using System.Diagnostics;
using UiPath.Core;
using Yash.Utility.Models.Email;

namespace Yash.Utility.Test.Services.EmailHelperService
{
    [TestClass]
    public class ToHtmlTableTests : EmailHelperServiceTestBase
    {
        [TestMethod]
        public void ToHtmlTable_WithBasicDataTable_ShouldGenerateHtmlTable()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Age", typeof(int));
            dataTable.Rows.Add("John", 25);
            dataTable.Rows.Add("Jane", 30);

            // Act
            var result = _service.ToHtmlTable(dataTable);

            // Assert
            result.Should().Contain("<table");
            result.Should().Contain("<tr");
            result.Should().Contain("<th");
            result.Should().Contain("<td");
            result.Should().Contain("Name");
            result.Should().Contain("Age");
            result.Should().Contain("John");
            result.Should().Contain("Jane");
            result.Should().Contain("25");
            result.Should().Contain("30");
            result.Should().Contain("</table>");
        }

        [TestMethod]
        public void ToHtmlTable_WithEmptyDataTable_ShouldGenerateHeadersOnly()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("Column1", typeof(string));
            dataTable.Columns.Add("Column2", typeof(string));

            // Act
            var result = _service.ToHtmlTable(dataTable);

            // Assert
            result.Should().Contain("Column1");
            result.Should().Contain("Column2");
            result.Should().Contain("<th");
            result.Should().NotContain("<td"); // No data rows
        }

        [TestMethod]
        public void ToHtmlTable_WithCustomOptions_ShouldUseCustomFormatting()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Rows.Add("Test");

            var options = new DataTableToHTMLOptions
            {
                TablePrefix = "<table class=\"custom\">",
                HeaderCellPrefix = "<th class=\"header\">",
                BodyCellPrefix = "<td class=\"data\">"
            };

            // Act
            var result = _service.ToHtmlTable(dataTable, options);

            // Assert
            result.Should().Contain("class=\"custom\"");
            result.Should().Contain("class=\"header\"");
            result.Should().Contain("class=\"data\"");
        }

        [TestMethod]
        public void ToHtmlTable_WithCustomSerializers_ShouldUseCustomSerialization()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("Date", typeof(DateTime));
            dataTable.Rows.Add(new DateTime(2023, 1, 1));

            var options = new DataTableToHTMLOptions();
            options.Serializers["Date"] = (obj) => ((DateTime)obj).ToString("yyyy-MM-dd");

            // Act
            var result = _service.ToHtmlTable(dataTable, options);

            // Assert
            result.Should().Contain("2023-01-01");
        }

        [TestMethod]
        public void ToHtmlTable_ShouldLogProcessingDetails()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("Test", typeof(string));
            dataTable.Rows.Add("Value");

            // Act
            _service.ToHtmlTable(dataTable);

            // Assert
            _mockLogger.Verify(
                x => x.Invoke(
                    It.Is<string>(s => s.Contains("Converting DataTable") && s.Contains("1 rows") && s.Contains("1 columns")),
                    TraceEventType.Information),
                Times.Once);
        }
    }
}