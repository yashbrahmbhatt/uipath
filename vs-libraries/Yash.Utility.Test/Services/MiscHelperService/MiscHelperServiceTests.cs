using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FluentAssertions;

namespace Yash.Utility.Test.Services.MiscHelperService
{
    [TestClass]
    public class MiscHelperServiceTests
    {
        private Utility.Services.Misc.MiscHelperService _service;
        private Utility.Services.Misc.MiscHelperService _serviceWithLogger;
        private List<string> _logMessages;
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            _logMessages = new List<string>();
            _testDirectory = Path.Combine(Path.GetTempPath(), "MiscHelperServiceTests", Guid.NewGuid().ToString());

            // Create service without logger
            _service = new Yash.Utility.Services.MiscHelperService();

            // Create service with logger
            _serviceWithLogger = new Yash.Utility.Services.MiscHelperService((message, level) =>
            {
                _logMessages.Add($"[{level}] {message}");
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test directory
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithoutLogger_ShouldSucceed()
        {
            // Arrange & Act
            var service = new Yash.Utility.Services.MiscHelperService();

            // Assert
            service.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithLogger_ShouldSucceed()
        {
            // Arrange
            var logMessages = new List<string>();

            // Act
            var service = new Yash.Utility.Services.MiscHelperService((message, level) =>
            {
                logMessages.Add($"[{level}] {message}");
            });

            // Assert
            service.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithNullLogger_ShouldSucceed()
        {
            // Arrange & Act
            var service = new Yash.Utility.Services.MiscHelperService(null);

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region IsMaintenanceTime Tests

        [TestMethod]
        public void IsMaintenanceTime_WithCurrentTimeInWindow_ShouldReturnTrue()
        {
            // Arrange
            var start = new TimeSpan(9, 0, 0);  // 9:00 AM
            var end = new TimeSpan(17, 0, 0);   // 5:00 PM
            var current = new TimeSpan(12, 0, 0); // 12:00 PM (noon)

            // Act
            var result = _serviceWithLogger.IsMaintenanceTime(start, end, current);

            // Assert
            result.Should().BeTrue();
            _logMessages.Should().Contain(msg => msg.Contains("IsMaintenanceTime") && msg.Contains("True"));
        }

        [TestMethod]
        public void IsMaintenanceTime_WithCurrentTimeBeforeWindow_ShouldReturnFalse()
        {
            // Arrange
            var start = new TimeSpan(9, 0, 0);  // 9:00 AM
            var end = new TimeSpan(17, 0, 0);   // 5:00 PM
            var current = new TimeSpan(8, 0, 0); // 8:00 AM

            // Act
            var result = _serviceWithLogger.IsMaintenanceTime(start, end, current);

            // Assert
            result.Should().BeFalse();
            _logMessages.Should().Contain(msg => msg.Contains("IsMaintenanceTime") && msg.Contains("False"));
        }

        [TestMethod]
        public void IsMaintenanceTime_WithCurrentTimeAfterWindow_ShouldReturnFalse()
        {
            // Arrange
            var start = new TimeSpan(9, 0, 0);  // 9:00 AM
            var end = new TimeSpan(17, 0, 0);   // 5:00 PM
            var current = new TimeSpan(18, 0, 0); // 6:00 PM

            // Act
            var result = _serviceWithLogger.IsMaintenanceTime(start, end, current);

            // Assert
            result.Should().BeFalse();
            _logMessages.Should().Contain(msg => msg.Contains("IsMaintenanceTime") && msg.Contains("False"));
        }

        [TestMethod]
        public void IsMaintenanceTime_WithCurrentTimeAtStartOfWindow_ShouldReturnTrue()
        {
            // Arrange
            var start = new TimeSpan(9, 0, 0);  // 9:00 AM
            var end = new TimeSpan(17, 0, 0);   // 5:00 PM
            var current = new TimeSpan(9, 0, 0); // 9:00 AM

            // Act
            var result = _service.IsMaintenanceTime(start, end, current);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsMaintenanceTime_WithCurrentTimeAtEndOfWindow_ShouldReturnTrue()
        {
            // Arrange
            var start = new TimeSpan(9, 0, 0);  // 9:00 AM
            var end = new TimeSpan(17, 0, 0);   // 5:00 PM
            var current = new TimeSpan(17, 0, 0); // 5:00 PM

            // Act
            var result = _service.IsMaintenanceTime(start, end, current);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsMaintenanceTime_WithOvernightWindow_InFirstPart_ShouldReturnTrue()
        {
            // Arrange
            var start = new TimeSpan(22, 0, 0); // 10:00 PM
            var end = new TimeSpan(6, 0, 0);    // 6:00 AM (next day)
            var current = new TimeSpan(23, 0, 0); // 11:00 PM

            // Act
            var result = _service.IsMaintenanceTime(start, end, current);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsMaintenanceTime_WithOvernightWindow_InSecondPart_ShouldReturnTrue()
        {
            // Arrange
            var start = new TimeSpan(22, 0, 0); // 10:00 PM
            var end = new TimeSpan(6, 0, 0);    // 6:00 AM (next day)
            var current = new TimeSpan(3, 0, 0); // 3:00 AM

            // Act
            var result = _service.IsMaintenanceTime(start, end, current);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsMaintenanceTime_WithOvernightWindow_OutsideWindow_ShouldReturnFalse()
        {
            // Arrange
            var start = new TimeSpan(22, 0, 0); // 10:00 PM
            var end = new TimeSpan(6, 0, 0);    // 6:00 AM (next day)
            var current = new TimeSpan(12, 0, 0); // 12:00 PM (noon)

            // Act
            var result = _service.IsMaintenanceTime(start, end, current);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsMaintenanceTime_WithoutCurrentTime_ShouldUseSystemTime()
        {
            // Arrange
            var start = new TimeSpan(0, 0, 0);  // Midnight
            var end = new TimeSpan(23, 59, 59); // End of day

            // Act
            var result = _service.IsMaintenanceTime(start, end);

            // Assert
            // Should return true since we're testing within a full day window
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsMaintenanceTime_WithZeroCurrentTime_ShouldUseSystemTime()
        {
            // Arrange
            var start = new TimeSpan(0, 0, 0);  // Midnight
            var end = new TimeSpan(23, 59, 59); // End of day
            var current = new DateTime(0).TimeOfDay; // Zero time (should trigger system time usage)

            // Act
            var result = _service.IsMaintenanceTime(start, end, current);

            // Assert
            // Should return true since we're testing within a full day window
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsMaintenanceTime_WithSameStartAndEndTime_ShouldReturnTrueForExactMatch()
        {
            // Arrange
            var start = new TimeSpan(12, 0, 0); // Noon
            var end = new TimeSpan(12, 0, 0);   // Noon
            var current = new TimeSpan(12, 0, 0); // Noon

            // Act
            var result = _service.IsMaintenanceTime(start, end, current);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsMaintenanceTime_WithSameStartAndEndTime_ShouldReturnFalseForDifferentTime()
        {
            // Arrange
            var start = new TimeSpan(12, 0, 0); // Noon
            var end = new TimeSpan(12, 0, 0);   // Noon
            var current = new TimeSpan(12, 0, 1); // One second past noon

            // Act
            var result = _service.IsMaintenanceTime(start, end, current);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region ResetFolder Tests

        [TestMethod]
        public void ResetFolder_WithNonExistentFolder_ShouldCreateFolder()
        {
            // Arrange
            var folderPath = Path.Combine(_testDirectory, "NewFolder");

            // Act
            _service.ResetFolder(folderPath);

            // Assert
            Directory.Exists(folderPath).Should().BeTrue();
            Directory.GetFiles(folderPath).Should().BeEmpty();
            Directory.GetDirectories(folderPath).Should().BeEmpty();
        }

        [TestMethod]
        public void ResetFolder_WithExistingEmptyFolder_ShouldRecreateFolder()
        {
            // Arrange
            var folderPath = Path.Combine(_testDirectory, "EmptyFolder");
            Directory.CreateDirectory(folderPath);

            // Act
            _service.ResetFolder(folderPath);

            // Assert
            Directory.Exists(folderPath).Should().BeTrue();
            Directory.GetFiles(folderPath).Should().BeEmpty();
            Directory.GetDirectories(folderPath).Should().BeEmpty();
        }

        [TestMethod]
        public void ResetFolder_WithExistingFolderContainingFiles_ShouldDeleteAndRecreateFolder()
        {
            // Arrange
            var folderPath = Path.Combine(_testDirectory, "FolderWithFiles");
            Directory.CreateDirectory(folderPath);
            
            var testFile1 = Path.Combine(folderPath, "test1.txt");
            var testFile2 = Path.Combine(folderPath, "test2.txt");
            File.WriteAllText(testFile1, "Test content 1");
            File.WriteAllText(testFile2, "Test content 2");

            // Verify files exist before reset
            File.Exists(testFile1).Should().BeTrue();
            File.Exists(testFile2).Should().BeTrue();

            // Act
            _service.ResetFolder(folderPath);

            // Assert
            Directory.Exists(folderPath).Should().BeTrue();
            Directory.GetFiles(folderPath).Should().BeEmpty();
            File.Exists(testFile1).Should().BeFalse();
            File.Exists(testFile2).Should().BeFalse();
        }

        [TestMethod]
        public void ResetFolder_WithExistingFolderContainingSubfolders_ShouldDeleteAndRecreateFolder()
        {
            // Arrange
            var folderPath = Path.Combine(_testDirectory, "FolderWithSubfolders");
            var subFolder1 = Path.Combine(folderPath, "SubFolder1");
            var subFolder2 = Path.Combine(folderPath, "SubFolder2");
            
            Directory.CreateDirectory(subFolder1);
            Directory.CreateDirectory(subFolder2);
            
            File.WriteAllText(Path.Combine(subFolder1, "file1.txt"), "Content 1");
            File.WriteAllText(Path.Combine(subFolder2, "file2.txt"), "Content 2");

            // Verify structure exists before reset
            Directory.Exists(subFolder1).Should().BeTrue();
            Directory.Exists(subFolder2).Should().BeTrue();

            // Act
            _service.ResetFolder(folderPath);

            // Assert
            Directory.Exists(folderPath).Should().BeTrue();
            Directory.GetDirectories(folderPath).Should().BeEmpty();
            Directory.GetFiles(folderPath).Should().BeEmpty();
            Directory.Exists(subFolder1).Should().BeFalse();
            Directory.Exists(subFolder2).Should().BeFalse();
        }

        [TestMethod]
        public void ResetFolder_WithNestedDirectoryStructure_ShouldDeleteEverythingAndRecreate()
        {
            // Arrange
            var folderPath = Path.Combine(_testDirectory, "ComplexFolder");
            var deepPath = Path.Combine(folderPath, "Level1", "Level2", "Level3");
            
            Directory.CreateDirectory(deepPath);
            File.WriteAllText(Path.Combine(folderPath, "root.txt"), "Root file");
            File.WriteAllText(Path.Combine(deepPath, "deep.txt"), "Deep file");

            // Act
            _service.ResetFolder(folderPath);

            // Assert
            Directory.Exists(folderPath).Should().BeTrue();
            Directory.GetDirectories(folderPath).Should().BeEmpty();
            Directory.GetFiles(folderPath).Should().BeEmpty();
        }

        [TestMethod]
        public void ResetFolder_WithParentDirectoryNotExisting_ShouldCreateParentAndTargetDirectories()
        {
            // Arrange
            var parentPath = Path.Combine(_testDirectory, "NonExistentParent");
            var folderPath = Path.Combine(parentPath, "TargetFolder");

            // Ensure parent doesn't exist
            Directory.Exists(parentPath).Should().BeFalse();

            // Act
            _service.ResetFolder(folderPath);

            // Assert
            Directory.Exists(parentPath).Should().BeTrue();
            Directory.Exists(folderPath).Should().BeTrue();
            Directory.GetFiles(folderPath).Should().BeEmpty();
        }

        #endregion

        #region TakeScreenshot Tests

        [TestMethod]
        public void TakeScreenshot_WithValidParameters_ShouldCreateScreenshotFile()
        {
            // Arrange
            var folder = _testDirectory;
            var prefix = "TestScreenshot";

            // Act
            var screenshotPath = _serviceWithLogger.TakeScreenshot(folder, "", prefix);

            // Assert
            screenshotPath.Should().NotBeNullOrEmpty();
            File.Exists(screenshotPath).Should().BeTrue();
            Path.GetExtension(screenshotPath).Should().Be(".png");
            screenshotPath.Should().Contain(prefix);
            screenshotPath.Should().Contain(Environment.MachineName);
            screenshotPath.Should().Contain(Environment.UserName);
            _logMessages.Should().Contain(msg => msg.Contains("Screenshot saved"));

            // Verify file is not empty (has some content)
            var fileInfo = new FileInfo(screenshotPath);
            fileInfo.Length.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void TakeScreenshot_WithSpecificFileName_ShouldUseProvidedFileName()
        {
            // Arrange
            var folder = _testDirectory;
            var fileName = Path.Combine(_testDirectory, "CustomScreenshot.png");
            var prefix = "TestPrefix";

            // Act
            var screenshotPath = _service.TakeScreenshot(folder, fileName, prefix);

            // Assert
            screenshotPath.Should().Be(fileName);
            File.Exists(fileName).Should().BeTrue();
            Path.GetExtension(screenshotPath).Should().Be(".png");
        }

        [TestMethod]
        public void TakeScreenshot_WithNonExistentFolder_ShouldCreateFolderAndScreenshot()
        {
            // Arrange
            var folder = Path.Combine(_testDirectory, "NonExistentFolder");
            var prefix = "TestScreenshot";

            // Ensure folder doesn't exist
            Directory.Exists(folder).Should().BeFalse();

            // Act
            var screenshotPath = _service.TakeScreenshot(folder, "", prefix);

            // Assert
            Directory.Exists(folder).Should().BeTrue();
            File.Exists(screenshotPath).Should().BeTrue();
            screenshotPath.Should().StartWith(folder);
        }

        [TestMethod]
        public void TakeScreenshot_WithEmptyPrefix_ShouldGenerateFileNameWithoutPrefix()
        {
            // Arrange
            var folder = _testDirectory;
            var prefix = "";

            // Act
            var screenshotPath = _service.TakeScreenshot(folder, "", prefix);

            // Assert
            screenshotPath.Should().NotBeNullOrEmpty();
            File.Exists(screenshotPath).Should().BeTrue();
            Path.GetFileName(screenshotPath).Should().StartWith($"_{Environment.MachineName}");
        }

        [TestMethod]
        public void TakeScreenshot_WithNullPrefix_ShouldHandleGracefully()
        {
            // Arrange
            var folder = _testDirectory;
            string prefix = null;

            // Act
            var screenshotPath = _service.TakeScreenshot(folder, "", prefix);

            // Assert
            screenshotPath.Should().NotBeNullOrEmpty();
            File.Exists(screenshotPath).Should().BeTrue();
        }

        [TestMethod]
        public void TakeScreenshot_Multiple_ShouldCreateUniqueFileNames()
        {
            // Arrange
            var folder = _testDirectory;
            var prefix = "MultiTest";

            // Act
            var screenshot1 = _service.TakeScreenshot(folder, "", prefix);
            System.Threading.Thread.Sleep(1000); // Ensure different timestamp
            var screenshot2 = _service.TakeScreenshot(folder, "", prefix);

            // Assert
            screenshot1.Should().NotBe(screenshot2);
            File.Exists(screenshot1).Should().BeTrue();
            File.Exists(screenshot2).Should().BeTrue();
        }

        [TestMethod]
        public void TakeScreenshot_WithLongPrefix_ShouldTruncateIfNecessary()
        {
            // Arrange
            var folder = _testDirectory;
            var longPrefix = new string('A', 200); // Very long prefix

            // Act
            var screenshotPath = _service.TakeScreenshot(folder, "", longPrefix);

            // Assert
            screenshotPath.Should().NotBeNullOrEmpty();
            File.Exists(screenshotPath).Should().BeTrue();
            // Verify the path is valid (doesn't exceed system limits)
            screenshotPath.Length.Should().BeLessThan(260); // Typical Windows path limit
        }

        [TestMethod]
        public void TakeScreenshot_WithSpecialCharactersInPrefix_ShouldSanitizeFileName()
        {
            // Arrange
            var folder = _testDirectory;
            var prefix = "Test<>:|?*\"\\Screenshot";

            // Act
            var screenshotPath = _service.TakeScreenshot(folder, "", prefix);

            // Assert
            screenshotPath.Should().NotBeNullOrEmpty();
            File.Exists(screenshotPath).Should().BeTrue();
            
            // The file name should be valid (no illegal characters)
            var fileName = Path.GetFileName(screenshotPath);
            fileName.Should().NotContain("<");
            fileName.Should().NotContain(">");
            fileName.Should().NotContain(":");
            fileName.Should().NotContain("|");
            fileName.Should().NotContain("?");
            fileName.Should().NotContain("*");
            fileName.Should().NotContain("\"");
            fileName.Should().NotContain("\\");
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void IntegrationTest_MaintenanceTimeAndFolderOperations_ShouldWorkTogether()
        {
            // Arrange
            var folder = Path.Combine(_testDirectory, "MaintenanceTest");
            var start = new TimeSpan(0, 0, 0);
            var end = new TimeSpan(23, 59, 59);
            var current = new TimeSpan(12, 0, 0);

            // Act
            var isMaintenanceTime = _service.IsMaintenanceTime(start, end, current);
            _service.ResetFolder(folder);

            // Assert
            isMaintenanceTime.Should().BeTrue();
            Directory.Exists(folder).Should().BeTrue();
            Directory.GetFiles(folder).Should().BeEmpty();
        }

        [TestMethod]
        public void IntegrationTest_ScreenshotAfterFolderReset_ShouldWorkCorrectly()
        {
            // Arrange
            var folder = _testDirectory;
            var prefix = "IntegrationTest";

            // Act
            _service.ResetFolder(folder);
            var screenshotPath = _service.TakeScreenshot(folder, "", prefix);

            // Assert
            Directory.Exists(folder).Should().BeTrue();
            File.Exists(screenshotPath).Should().BeTrue();
            screenshotPath.Should().StartWith(folder);
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public void ResetFolder_WithNullPath_ShouldThrowException()
        {
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => _service.ResetFolder(null));
        }

        [TestMethod]
        public void ResetFolder_WithEmptyPath_ShouldThrowException()
        {
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentException>(() => _service.ResetFolder(string.Empty));
        }

        [TestMethod]
        public void TakeScreenshot_WithNullFolder_ShouldThrowException()
        {
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentException>(() => _service.TakeScreenshot(null, "", "prefix"));
        }

        [TestMethod]
        public void TakeScreenshot_WithEmptyFolder_ShouldThrowException()
        {
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentException>(() => _service.TakeScreenshot(string.Empty, "", "prefix"));
        }

        #endregion

        #region Logging Tests

        [TestMethod]
        public void _service_WithoutLogger_ShouldNotThrowOnOperations()
        {
            // Arrange
            var folder = _testDirectory;
            var start = new TimeSpan(9, 0, 0);
            var end = new TimeSpan(17, 0, 0);
            var current = new TimeSpan(12, 0, 0);

            // Act & Assert (should not throw)
            _service.IsMaintenanceTime(start, end, current);
            _service.ResetFolder(folder);
            var screenshotPath = _service.TakeScreenshot(folder, "", "NoLogger");
            
            // Verify operations worked
            Directory.Exists(folder).Should().BeTrue();
            File.Exists(screenshotPath).Should().BeTrue();
        }

        [TestMethod]
        public void _service_WithLogger_ShouldLogMaintenanceTimeOperations()
        {
            // Arrange
            var start = new TimeSpan(9, 0, 0);
            var end = new TimeSpan(17, 0, 0);
            var current = new TimeSpan(12, 0, 0);

            // Act
            _serviceWithLogger.IsMaintenanceTime(start, end, current);

            // Assert
            _logMessages.Should().NotBeEmpty();
            _logMessages.Should().Contain(msg => msg.Contains("MiscHelperService"));
            _logMessages.Should().Contain(msg => msg.Contains("IsMaintenanceTime"));
        }

        [TestMethod]
        public void _service_WithLogger_ShouldLogScreenshotOperations()
        {
            // Arrange
            var folder = _testDirectory;
            var prefix = "LoggerTest";

            // Act
            _serviceWithLogger.TakeScreenshot(folder, "", prefix);

            // Assert
            _logMessages.Should().NotBeEmpty();
            _logMessages.Should().Contain(msg => msg.Contains("MiscHelperService"));
            _logMessages.Should().Contain(msg => msg.Contains("Screenshot saved"));
        }

        #endregion
    }
}