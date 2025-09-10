using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using TraceEventType = System.Diagnostics.TraceEventType;

namespace Yash.Utility.Helpers
{
    public static class ExcelHelpers
    {
        static ExcelHelpers()
        {
            // Set the license context for EPPlus (required for v5+)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Reads an Excel file and converts its sheets into a DataSet.
        /// Uses EPPlus which can read files even when they are open in Excel.
        /// </summary>
        /// <param name="filePath">Path to the Excel workbook.</param>
        /// <param name="Log">Optional logging delegate for diagnostics.</param>
        /// <returns>A DataSet containing all sheets as DataTables.</returns>
        public static DataSet ReadExcelFile(string filePath, Action<string, TraceEventType>? Log = null)
        {
            var dataSet = new DataSet();

            if (!File.Exists(filePath))
            {
                Log?.Invoke($"[ExcelHelpers] File not found: {filePath}", TraceEventType.Error);
                throw new FileNotFoundException("Excel file not found.", filePath);
            }

            Log?.Invoke($"[ExcelHelpers] Opening Excel file: {filePath}", TraceEventType.Information);

            try
            {
                // Use FileInfo to create a copy that can be read even if the original is open
                var fileInfo = new FileInfo(filePath);

                using (var package = new ExcelPackage(fileInfo))
                {
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        Log?.Invoke($"[ExcelHelpers] Processing sheet: {worksheet.Name}", TraceEventType.Verbose);

                        var dataTable = new DataTable(worksheet.Name);

                        // Get the used range of the worksheet
                        var start = worksheet.Dimension?.Start;
                        var end = worksheet.Dimension?.End;

                        if (start == null || end == null)
                        {
                            Log?.Invoke($"[ExcelHelpers] Sheet '{worksheet.Name}' is empty, skipping.", TraceEventType.Warning);
                            continue;
                        }

                        // Read header row to create columns
                        for (int col = start.Column; col <= end.Column; col++)
                        {
                            var headerValue = worksheet.Cells[start.Row, col].Text?.Trim();
                            if (string.IsNullOrEmpty(headerValue))
                            {
                                headerValue = $"Column{col}"; // Default name for empty headers
                            }

                            // Handle duplicate column names
                            var originalName = headerValue;
                            int suffix = 1;
                            while (dataTable.Columns.Contains(headerValue))
                            {
                                headerValue = $"{originalName}_{suffix}";
                                suffix++;
                            }

                            dataTable.Columns.Add(headerValue, typeof(object));
                        }

                        Log?.Invoke($"[ExcelHelpers] Header columns: {string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}", TraceEventType.Verbose);

                        // Read data rows (skip header row)
                        for (int row = start.Row + 1; row <= end.Row; row++)
                        {
                            var dataRow = dataTable.NewRow();
                            bool hasData = false;

                            for (int col = start.Column; col <= end.Column; col++)
                            {
                                var cellValue = worksheet.Cells[row, col].Value;
                                var columnIndex = col - start.Column;

                                if (cellValue != null)
                                {
                                    // Convert Excel data types appropriately
                                    if (cellValue is DateTime dateTime)
                                    {
                                        dataRow[columnIndex] = dateTime;
                                    }
                                    else if (cellValue is double || cellValue is decimal || cellValue is float)
                                    {
                                        dataRow[columnIndex] = cellValue;
                                    }
                                    else if (cellValue is bool)
                                    {
                                        dataRow[columnIndex] = cellValue;
                                    }
                                    else
                                    {
                                        var textValue = worksheet.Cells[row, col].Text?.Trim();
                                        dataRow[columnIndex] = string.IsNullOrEmpty(textValue) ? DBNull.Value : textValue;
                                    }
                                    hasData = true;
                                }
                                else
                                {
                                    dataRow[columnIndex] = DBNull.Value;
                                }
                            }

                            // Only add rows that have at least some data
                            if (hasData)
                            {
                                dataTable.Rows.Add(dataRow);
                            }
                        }

                        Log?.Invoke($"[ExcelHelpers] Sheet '{worksheet.Name}' loaded with {dataTable.Rows.Count} rows and {dataTable.Columns.Count} columns.", TraceEventType.Information);
                        dataSet.Tables.Add(dataTable);
                    }
                }
            }
            catch (IOException ioEx)
            {
                Log?.Invoke($"[ExcelHelpers] IO Error reading file {filePath}: {ioEx.Message}. Attempting to read with retry strategy.", TraceEventType.Warning);

                // Try reading the file with a different approach for locked files
                return ReadExcelFileWithRetry(filePath, Log);
            }
            catch (Exception ex)
            {
                Log?.Invoke($"[ExcelHelpers] Error reading Excel file {filePath}: {ex.Message}", TraceEventType.Error);
                throw new InvalidOperationException($"Failed to read Excel file: {ex.Message}", ex);
            }

            Log?.Invoke($"[ExcelHelpers] Completed reading Excel file: {filePath}", TraceEventType.Information);
            return dataSet;
        }

        /// <summary>
        /// Attempts to read an Excel file that may be locked by copying it to a temporary location first.
        /// Uses exponential backoff retry strategy for handling FileShare.None locks.
        /// </summary>
        private static DataSet ReadExcelFileWithRetry(string filePath, Action<string, TraceEventType>? Log = null)
        {
            const int maxRetries = 5;
            const int baseDelayMs = 500;
            string tempFilePath = "";

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    Log?.Invoke($"[ExcelHelpers] Retry attempt {attempt + 1} of {maxRetries}", TraceEventType.Information);

                    // Wait before retry (exponential backoff)
                    if (attempt > 0)
                    {
                        int delay = baseDelayMs * (int)Math.Pow(2, attempt - 1);
                        Log?.Invoke($"[ExcelHelpers] Waiting {delay}ms before retry", TraceEventType.Verbose);
                        Thread.Sleep(delay);
                    }

                    // Generate a unique temp file path for each attempt
                    tempFilePath = Path.Combine(Path.GetTempPath(), $"temp_excel_{Guid.NewGuid()}.xlsx");

                    Log?.Invoke($"[ExcelHelpers] Copying locked file to temporary location: {tempFilePath}", TraceEventType.Information);

                    // Try to copy the file to temp location to avoid lock issues
                    File.Copy(filePath, tempFilePath, true);

                    // Read from the temp file
                    var result = ReadExcelFile(tempFilePath, Log);

                    Log?.Invoke($"[ExcelHelpers] Successfully read Excel file from temporary copy", TraceEventType.Information);
                    return result;
                }
                catch (IOException ioEx) when (attempt < maxRetries - 1)
                {
                    Log?.Invoke($"[ExcelHelpers] Retry {attempt + 1} failed: {ioEx.Message}", TraceEventType.Warning);

                    // Clean up temp file if it was created
                    if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
                    {
                        try
                        {
                            File.Delete(tempFilePath);
                        }
                        catch
                        {
                            // Ignore cleanup errors during retry
                        }
                    }
                    continue;
                }
                catch (IOException ioEx) when (attempt == maxRetries - 1)
                {
                    // Final attempt failed - provide detailed error message
                    Log?.Invoke($"[ExcelHelpers] All retry attempts failed. File may be exclusively locked.", TraceEventType.Error);

                    // Clean up temp file if it was created
                    if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
                    {
                        try
                        {
                            File.Delete(tempFilePath);
                        }
                        catch
                        {
                            // Ignore cleanup errors
                        }
                    }

                    throw new InvalidOperationException(
                        $"Unable to access Excel file '{filePath}' after {maxRetries} attempts. " +
                        "The file may be exclusively locked by another process (such as Excel). " +
                        "Please close the file in other applications and try again.", ioEx);
                }
            }

            // This should never be reached due to the exception handling above
            throw new InvalidOperationException("Unexpected end of retry loop");
        }

        /// <summary>
        /// Creates a basic Excel file with specified sheets and headers.
        /// </summary>
        /// <param name="outputPath">Path where the Excel file will be saved.</param>
        /// <param name="sheets">Dictionary where key is sheet name and value is array of header names.</param>
        /// <param name="Log">Optional logging delegate for diagnostics.</param>
        public static void CreateExcelFile(string outputPath, Dictionary<string, string[]> sheets, Action<string, TraceEventType>? Log = null)
        {
            Log?.Invoke($"[ExcelHelpers] Creating Excel file: {outputPath}", TraceEventType.Information);

            using (var package = new ExcelPackage())
            {
                foreach (var sheet in sheets)
                {
                    var worksheet = package.Workbook.Worksheets.Add(sheet.Key);

                    // Add headers
                    for (int i = 0; i < sheet.Value.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = sheet.Value[i];
                    }

                    worksheet.Cells.AutoFitColumns();
                    Log?.Invoke($"[ExcelHelpers] Created sheet '{sheet.Key}' with {sheet.Value.Length} columns.", TraceEventType.Verbose);
                }

                var fileInfo = new FileInfo(outputPath);
                package.SaveAs(fileInfo);
                Log?.Invoke($"[ExcelHelpers] Excel file saved to: {outputPath}", TraceEventType.Information);
            }
        }

        /// <summary>
        /// Creates an Excel file with data from a DataSet.
        /// </summary>
        /// <param name="outputPath">Path where the Excel file will be saved.</param>
        /// <param name="dataSet">DataSet containing DataTables to export as sheets.</param>
        /// <param name="Log">Optional logging delegate for diagnostics.</param>
        public static void CreateExcelFile(string outputPath, DataSet dataSet, Action<string, TraceEventType>? Log = null)
        {
            Log?.Invoke($"[ExcelHelpers] Creating Excel file with DataSet: {outputPath}", TraceEventType.Information);

            using (var package = new ExcelPackage())
            {
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    var sheetName = dataTable.TableName;
                    var worksheet = package.Workbook.Worksheets.Add(sheetName);

                    Log?.Invoke($"[ExcelHelpers] Processing sheet '{sheetName}' with {dataTable.Rows.Count} rows and {dataTable.Columns.Count} columns.", TraceEventType.Verbose);

                    // Add column headers
                    for (int col = 0; col < dataTable.Columns.Count; col++)
                    {
                        worksheet.Cells[1, col + 1].Value = dataTable.Columns[col].ColumnName;

                        // Optional: Add some formatting to headers
                        worksheet.Cells[1, col + 1].Style.Font.Bold = true;
                    }

                    // Add data rows
                    for (int row = 0; row < dataTable.Rows.Count; row++)
                    {
                        for (int col = 0; col < dataTable.Columns.Count; col++)
                        {
                            var cellValue = dataTable.Rows[row][col];
                            var excelRow = row + 2; // +2 because Excel is 1-indexed and we skip header row
                            var excelCol = col + 1;  // +1 because Excel is 1-indexed

                            // Handle different data types appropriately
                            if (cellValue == null || cellValue == DBNull.Value)
                            {
                                worksheet.Cells[excelRow, excelCol].Value = "";
                            }
                            else if (cellValue is DateTime dateTime)
                            {
                                worksheet.Cells[excelRow, excelCol].Value = dateTime;
                                worksheet.Cells[excelRow, excelCol].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                            }
                            else if (cellValue is TimeSpan timeSpan)
                            {
                                worksheet.Cells[excelRow, excelCol].Value = timeSpan.TotalDays;
                                worksheet.Cells[excelRow, excelCol].Style.Numberformat.Format = "[h]:mm:ss";
                            }
                            else if (cellValue is decimal || cellValue is double || cellValue is float)
                            {
                                worksheet.Cells[excelRow, excelCol].Value = cellValue;
                            }
                            else if (cellValue is int || cellValue is long || cellValue is short)
                            {
                                worksheet.Cells[excelRow, excelCol].Value = cellValue;
                            }
                            else if (cellValue is bool boolValue)
                            {
                                worksheet.Cells[excelRow, excelCol].Value = boolValue;
                            }
                            else
                            {
                                // Default to string representation
                                worksheet.Cells[excelRow, excelCol].Value = cellValue.ToString();
                            }
                        }
                    }

                    // Auto-fit columns for better readability
                    worksheet.Cells.AutoFitColumns();

                    Log?.Invoke($"[ExcelHelpers] Completed sheet '{sheetName}' with {dataTable.Rows.Count} data rows.", TraceEventType.Verbose);
                }

                var fileInfo = new FileInfo(outputPath);
                package.SaveAs(fileInfo);
                Log?.Invoke($"[ExcelHelpers] Excel file with DataTables saved to: {outputPath}", TraceEventType.Information);
            }
        }

        /// <summary>
        /// Creates an Excel file from a single DataTable.
        /// </summary>
        /// <param name="outputPath">Path where the Excel file will be saved.</param>
        /// <param name="dataTable">DataTable with data to export.</param>
        /// <param name="sheetName">Optional sheet name. Defaults to the DataTable's TableName or "Sheet1".</param>
        /// <param name="Log">Optional logging delegate for diagnostics.</param>
        public static void CreateExcelFile(string outputPath, DataTable dataTable, string? sheetName = null, Action<string, TraceEventType>? Log = null)
        {
            var actualSheetName = sheetName ?? dataTable.TableName ?? "Sheet1";
            dataTable.TableName = actualSheetName;

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable.Copy());

            CreateExcelFile(outputPath, dataSet, Log);
        }

        /// <summary>
        /// Generates an Excel template with proper data validation.
        /// </summary>
        /// <param name="outputPath">Path where the template file will be saved.</param>
        /// <param name="supportedTypeNames">Array of supported type names for dropdown validation.</param>
        /// <param name="Log">Optional logging delegate for diagnostics.</param>
        public static void GenerateExcelTemplate(string outputPath, string[] supportedTypeNames, Action<string, TraceEventType>? Log = null)
        {
            Log?.Invoke($"[ExcelHelpers] Creating Excel template: {outputPath}", TraceEventType.Information);

            using (var package = new ExcelPackage())
            {
                // 1. _Instructions Sheet
                var instructions = package.Workbook.Worksheets.Add("_Instructions");
                instructions.Cells["A1"].Value = "Welcome to the Config Template!";
                instructions.Cells["A3"].Value = "This file is used to define your automation settings, assets, and file references.";
                instructions.Cells["A5"].Value = "Use the Yash.Config wizards in UiPath to generate strongly-typed C# classes based on this file.";
                instructions.Cells.AutoFitColumns();

                // 2. _ConfigFileSettings Sheet (defines types)
                var typeSheet = package.Workbook.Worksheets.Add("_ConfigFileSettings");
                typeSheet.Cells["A1"].Value = "SupportedTypes";
                int row = 2;
                foreach (var typeName in supportedTypeNames)
                {
                    typeSheet.Cells[row, 1].Value = typeName;
                    row++;
                }

                // 3. Settings / Assets / Files with type dropdown
                void AddConfigSheet(string name)
                {
                    var ws = package.Workbook.Worksheets.Add(name);
                    ws.Cells["A1"].Value = "Name";
                    ws.Cells["B1"].Value = "Value / Path";
                    ws.Cells["C1"].Value = "Type";
                    ws.Cells["D1"].Value = "Description";

                    // Apply validation to column C (Type) for first 100 rows
                    var maxRows = 100;
                    for (int i = 2; i <= maxRows + 1; i++)
                    {
                        var validation = ws.DataValidations.AddListValidation($"C{i}");
                        validation.Formula.ExcelFormula = $"_ConfigFileSettings.$A$2:$A${supportedTypeNames.Length + 1}";
                        validation.AllowBlank = false;
                        validation.ShowInputMessage = true;
                        validation.PromptTitle = "Data Type";
                        validation.Prompt = "Select a valid data type from the list.";
                    }

                    ws.Cells.AutoFitColumns();
                }

                AddConfigSheet("Settings");
                AddConfigSheet("Assets");
                AddConfigSheet("Files");

                // Save file
                var fileInfo = new FileInfo(outputPath);
                package.SaveAs(fileInfo);
                Log?.Invoke($"[ExcelHelpers] Excel template saved to: {outputPath}", TraceEventType.Information);
            }
        }

        /// <summary>
        /// Returns a C#-style friendly name for the given type, including support for generics and arrays.
        /// </summary>
        public static string GetFriendlyTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var typeDefName = type.GetGenericTypeDefinition().Name;
                var tickIndex = typeDefName.IndexOf('`');
                if (tickIndex > 0)
                    typeDefName = typeDefName.Substring(0, tickIndex);

                var genericArgs = type.GetGenericArguments().Select(GetFriendlyTypeName);
                return $"{typeDefName}<{string.Join(", ", genericArgs)}>";
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return elementType != null ? $"{GetFriendlyTypeName(elementType)}[]" : "object[]";
            }

            return type switch
            {
                _ when type == typeof(int) => "int",
                _ when type == typeof(string) => "string",
                _ when type == typeof(bool) => "bool",
                _ when type == typeof(double) => "double",
                _ when type == typeof(DateTime) => "DateTime",
                _ when type == typeof(TimeSpan) => "TimeSpan",
                _ => type.Name
            };
        }        
    }
}
