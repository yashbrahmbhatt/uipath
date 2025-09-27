using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;

namespace Yash.Utility.Test.Services.ExcelHelperService
{
    [TestClass]
    public class GetFriendlyTypeNameTests : ExcelHelperServiceTestBase
    {
        [TestMethod]
        public void GetFriendlyTypeName_WithPrimitiveTypes_ShouldReturnFriendlyNames()
        {
            // Act & Assert
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(int)).Should().Be("int");
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(string)).Should().Be("string");
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(bool)).Should().Be("bool");
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(double)).Should().Be("double");
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(DateTime)).Should().Be("DateTime");
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(TimeSpan)).Should().Be("TimeSpan");
        }

        [TestMethod]
        public void GetFriendlyTypeName_WithGenericTypes_ShouldReturnGenericFormat()
        {
            // Act & Assert
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(List<string>)).Should().Be("List<string>");
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(Dictionary<string, int>)).Should().Be("Dictionary<string, int>");
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(Nullable<int>)).Should().Be("Nullable<int>");
        }

        [TestMethod]
        public void GetFriendlyTypeName_WithArrayTypes_ShouldReturnArrayFormat()
        {
            // Act & Assert
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(int[])).Should().Be("int[]");
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(string[])).Should().Be("string[]");
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(DateTime[])).Should().Be("DateTime[]");
        }

        [TestMethod]
        public void GetFriendlyTypeName_WithCustomTypes_ShouldReturnTypeName()
        {
            // Act & Assert
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(Utility.Services.Excel.ExcelHelperService)).Should().Be("ExcelHelperService");
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(DataTable)).Should().Be("DataTable");
        }

        [TestMethod]
        public void GetFriendlyTypeName_WithNestedGenericTypes_ShouldReturnNestedFormat()
        {
            // Act & Assert
            Utility.Services.Excel.ExcelHelperService.GetFriendlyTypeName(typeof(List<Dictionary<string, int>>))
                .Should().Be("List<Dictionary<string, int>>");
        }
    }
}