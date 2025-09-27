using System;
using System.Collections.Generic;
using System.Data;
using Yash.Utility.Services;
using Yash.Config.Services;

// Test class to verify the refactoring works
public class TestRefactoring
{
    public static void Main()
    {
        Console.WriteLine("Testing ExcelHelperService refactoring...");
        
        // Test object flattening functionality
        var excelService = new ExcelHelperService();
        
        // Create test objects with nested properties
        var testObjects = new[]
        {
            new
            {
                Name = "John",
                Age = 30,
                Address = new
                {
                    Street = "123 Main St",
                    City = "Anytown",
                    Country = "USA"
                },
                Skills = new[] { "C#", "UiPath", "Excel" }
            },
            new
            {
                Name = "Jane",
                Age = 25,
                Address = new
                {
                    Street = "456 Oak Ave",
                    City = "Another City",
                    Country = "Canada"
                },
                Skills = new[] { "Python", "JavaScript" }
            }
        };
        
        // Test DataTable creation from objects
        var dataTable = excelService.CreateDataTableFromObjects(testObjects, "TestData");
        
        Console.WriteLine($"Created DataTable with {dataTable.Columns.Count} columns:");
        foreach (DataColumn column in dataTable.Columns)
        {
            Console.WriteLine($"  - {column.ColumnName}");
        }
        
        Console.WriteLine($"DataTable has {dataTable.Rows.Count} rows");
        
        // Test Excel file creation from objects
        var tempPath = System.IO.Path.GetTempFileName() + ".xlsx";
        excelService.CreateExcelFileFromObjects(tempPath, testObjects, "FlattenedData");
        Console.WriteLine($"Created Excel file: {tempPath}");
        
        // Test Config Service template generation
        Console.WriteLine("\nTesting ConfigService template generation...");
        var configService = new ConfigService("dummy_path.xlsx", "https://dummy.url", "clientId", "clientSecret", new[] { "scope" });
        var templatePath = System.IO.Path.GetTempFileName() + ".xlsx";
        configService.GenerateExcelTemplate(templatePath);
        Console.WriteLine($"Created config template: {templatePath}");
        
        Console.WriteLine("\nRefactoring test completed successfully!");
        
        // Clean up
        try
        {
            System.IO.File.Delete(tempPath);
            System.IO.File.Delete(templatePath);
        }
        catch { /* Ignore cleanup errors */ }
    }
}