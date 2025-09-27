using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Yash.Utility.Test.Services.ExcelHelperService
{
    [TestClass]
    public class ErrorHandlingTests : ExcelHelperServiceTestBase
    {
        [TestMethod]
        public void CreateExcelFile_WithInvalidPath_ShouldThrowException()
        {
            // Arrange
            var invalidPath = "Z:\\NonExistentDrive\\file.xlsx";
            var sheets = new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "Sheet1", new[] { "Column1" } }
            };

            // Act & Assert
            var action = () => _service.CreateExcelFile(invalidPath, sheets);
            action.Should().Throw<Exception>();
        }

        [TestMethod]
        public void ReadExcelFile_WithInvalidFile_ShouldLogAndThrowException()
        {
            // Arrange
            var invalidFile = Path.Combine(_testDirectory, "invalid.txt");
            File.WriteAllText(invalidFile, "This is not an Excel file");

            // Act & Assert
            var action = () => _service.ReadExcelFile(invalidFile);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Failed to read Excel file*");
        }

        [TestMethod]
        public void ReadExcelFile_WithCorruptedFile_ShouldHandleGracefully()
        {
            // Arrange
            var corruptedFile = Path.Combine(_testDirectory, "corrupted.xlsx");
            File.WriteAllBytes(corruptedFile, new byte[] { 0x50, 0x4B, 0x03, 0x04 }); // Partial ZIP header

            // Act & Assert
            var action = () => _service.ReadExcelFile(corruptedFile);
            action.Should().Throw<InvalidOperationException>();
        }
    }
}