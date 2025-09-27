using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TraceEventType = System.Diagnostics.TraceEventType;

namespace Yash.Utility.Services.Excel
{
    public class ExcelHelperService : Base.BaseService, IExcelHelperService, IDisposable
    {
        private readonly ExcelPackage _excelPackage = new();

        /// <summary>
        /// Creates a new ExcelHelperService instance
        /// </summary>
        /// <param name="log">Optional logging action</param>
        public ExcelHelperService(Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information) : base(null, log, minLogLevel)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Reads a file (Excel or CSV) and converts its contents into a DataSet.
        /// Automatically detects file format based on extension and uses appropriate reader.
        /// </summary>
        /// <param name="filePath">Path to the file (Excel or CSV).</param>
        /// <param name="password">Password for protected Excel files (ignored for CSV).</param>
        /// <returns>A DataSet containing the file data as DataTables.</returns>
        public DataSet ReadFile(string filePath, string password = "")
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                Log($"File not found: {filePath}", TraceEventType.Error);
                throw new FileNotFoundException("File not found.", filePath);
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            Log($"Reading file: {filePath} (detected format: {extension})", TraceEventType.Information);

            return extension switch
            {
                ".csv" => ReadCsvFile(filePath),
                ".xlsx" or ".xls" => ReadExcelFile(filePath, password),
                _ => throw new NotSupportedException($"File format '{extension}' is not supported. Supported formats: .csv, .xlsx, .xls")
            };
        }

        /// <summary>
        /// Reads a CSV file and converts it into a DataSet with a single DataTable.
        /// </summary>
        /// <param name="filePath">Path to the CSV file.</param>
        /// <param name="delimiter">Column delimiter. Auto-detected if not specified.</param>
        /// <param name="hasHeaders">Whether the first row contains headers. Auto-detected if not specified.</param>
        /// <param name="encoding">Text encoding. UTF-8 if not specified.</param>
        /// <returns>A DataSet containing the CSV data as a single DataTable.</returns>
        public DataSet ReadCsvFile(string filePath, string? delimiter = null, bool? hasHeaders = null, Encoding? encoding = null)
        {
            if (!File.Exists(filePath))
            {
                Log($"CSV file not found: {filePath}", TraceEventType.Error);
                throw new FileNotFoundException("CSV file not found.", filePath);
            }

            Log($"Reading CSV file: {filePath}", TraceEventType.Information);

            var dataSet = new DataSet();
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var dataTable = new DataTable(fileName);

            try
            {
                var actualEncoding = encoding ?? Encoding.UTF8;
                var lines = File.ReadAllLines(filePath, actualEncoding);

                if (lines.Length == 0)
                {
                    Log($"CSV file '{filePath}' is empty, creating empty DataTable.", TraceEventType.Warning);
                    dataSet.Tables.Add(dataTable);
                    return dataSet;
                }

                // Auto-detect delimiter if not provided
                var actualDelimiter = delimiter ?? DetectCsvDelimiter(lines[0]);
                Log($"Using delimiter: '{actualDelimiter}'", TraceEventType.Verbose);

                // Auto-detect headers if not specified
                var actualHasHeaders = hasHeaders ?? DetectCsvHeaders(lines, actualDelimiter);
                Log($"Headers detected: {actualHasHeaders}", TraceEventType.Verbose);

                var firstDataRowIndex = 0;

                // Parse headers or create default column names
                var headerRow = lines[0].Split(new[] { actualDelimiter }, StringSplitOptions.None);

                if (actualHasHeaders)
                {
                    firstDataRowIndex = 1;
                    for (int i = 0; i < headerRow.Length; i++)
                    {
                        var columnName = headerRow[i].Trim().Trim('"');
                        if (string.IsNullOrEmpty(columnName))
                        {
                            columnName = $"Column{i + 1}";
                        }

                        // Handle duplicate column names
                        var originalName = columnName;
                        int suffix = 1;
                        while (dataTable.Columns.Contains(columnName))
                        {
                            columnName = $"{originalName}_{suffix}";
                            suffix++;
                        }

                        dataTable.Columns.Add(columnName, typeof(string));
                    }
                }
                else
                {
                    // Create default column names based on first row
                    for (int i = 0; i < headerRow.Length; i++)
                    {
                        dataTable.Columns.Add($"Column{i + 1}", typeof(string));
                    }
                }

                Log($"Created {dataTable.Columns.Count} columns", TraceEventType.Verbose);

                // Parse data rows
                for (int lineIndex = firstDataRowIndex; lineIndex < lines.Length; lineIndex++)
                {
                    var line = lines[lineIndex].Trim();
                    if (string.IsNullOrEmpty(line)) continue; // Skip empty lines

                    var values = ParseCsvLine(line, actualDelimiter);
                    var row = dataTable.NewRow();

                    for (int i = 0; i < Math.Min(values.Length, dataTable.Columns.Count); i++)
                    {
                        var value = values[i].Trim().Trim('"');
                        row[i] = string.IsNullOrEmpty(value) ? DBNull.Value : value;
                    }

                    dataTable.Rows.Add(row);
                }

                Log($"CSV file '{fileName}' loaded with {dataTable.Rows.Count} rows and {dataTable.Columns.Count} columns.", TraceEventType.Information);
                dataSet.Tables.Add(dataTable);
            }
            catch (Exception ex)
            {
                Log($"Error reading CSV file {filePath}: {ex.Message}", TraceEventType.Error);
                throw new InvalidOperationException($"Failed to read CSV file: {ex.Message}", ex);
            }

            return dataSet;
        }

        /// <summary>
        /// Detects the most likely delimiter used in a CSV line.
        /// </summary>
        private string DetectCsvDelimiter(string line)
        {
            var delimiters = new[] { ",", ";", "\t", "|" };
            var delimiter = ","; // Default
            var maxCount = 0;

            foreach (var testDelimiter in delimiters)
            {
                var count = line.Split(new[] { testDelimiter }, StringSplitOptions.None).Length - 1;
                if (count > maxCount)
                {
                    maxCount = count;
                    delimiter = testDelimiter;
                }
            }

            return delimiter;
        }

        /// <summary>
        /// Attempts to detect if the first row contains headers by analyzing the data pattern.
        /// </summary>
        private bool DetectCsvHeaders(string[] lines, string delimiter)
        {
            if (lines.Length < 2) return true; // Assume headers if only one line

            var firstRow = lines[0].Split(new[] { delimiter }, StringSplitOptions.None);
            var secondRow = lines[1].Split(new[] { delimiter }, StringSplitOptions.None);

            if (firstRow.Length != secondRow.Length) return true; // Different column counts suggest headers

            // Check if first row contains mostly non-numeric data while second row contains numeric data
            var firstRowNumeric = 0;
            var secondRowNumeric = 0;

            for (int i = 0; i < Math.Min(firstRow.Length, secondRow.Length); i++)
            {
                if (double.TryParse(firstRow[i].Trim().Trim('"'), out _)) firstRowNumeric++;
                if (double.TryParse(secondRow[i].Trim().Trim('"'), out _)) secondRowNumeric++;
            }

            // If second row has more numeric values than first row, first row is likely headers
            return secondRowNumeric > firstRowNumeric;
        }

        /// <summary>
        /// Parses a CSV line handling quoted values and escaped quotes.
        /// </summary>
        private string[] ParseCsvLine(string line, string delimiter)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;
            var i = 0;

            while (i < line.Length)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        current.Append('"');
                        i += 2;
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                        i++;
                    }
                }
                else if (!inQuotes && line.Substring(i).StartsWith(delimiter))
                {
                    // Found delimiter outside quotes
                    result.Add(current.ToString());
                    current.Clear();
                    i += delimiter.Length;
                }
                else
                {
                    current.Append(c);
                    i++;
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        /// <summary>
        /// Reads an Excel file and converts its sheets into a DataSet.
        /// Uses EPPlus which can read files even when they are open in Excel.
        /// </summary>
        /// <param name="filePath">Path to the Excel workbook.</param>
        /// <param name="password">Password for protected Excel files.</param>
        /// <returns>A DataSet containing all sheets as DataTables.</returns>
        public DataSet ReadExcelFile(string filePath, string password = "")
        {
            _excelPackage.File = new FileInfo(filePath);
            var dataSet = new DataSet();

            if (!File.Exists(filePath))
            {
                Log($"File not found: {filePath}", TraceEventType.Error);
                throw new FileNotFoundException("Excel file not found.", filePath);
            }

            Log($"Opening Excel file: {filePath}", TraceEventType.Information);

            try
            {
                // Use FileInfo to create a copy that can be read even if the original is open
                var fileInfo = new FileInfo(filePath);

                using (var package = new ExcelPackage(fileInfo, password))
                {
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        Log($"Processing sheet: {worksheet.Name}", TraceEventType.Verbose);

                        var dataTable = new DataTable(worksheet.Name);

                        // Get the used range of the worksheet
                        var start = worksheet.Dimension?.Start;
                        var end = worksheet.Dimension?.End;

                        if (start == null || end == null)
                        {
                            Log($"Sheet '{worksheet.Name}' is empty, creating empty DataTable.", TraceEventType.Warning);
                            var emptyTable = new DataTable(worksheet.Name);
                            dataSet.Tables.Add(emptyTable);
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

                        Log($"Header columns: {string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}", TraceEventType.Verbose);

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

                        Log($"Sheet '{worksheet.Name}' loaded with {dataTable.Rows.Count} rows and {dataTable.Columns.Count} columns.", TraceEventType.Information);

                        // Explicitly set the table name to prevent DataSet from auto-naming it
                        dataTable.TableName = worksheet.Name;
                        dataSet.Tables.Add(dataTable);
                    }
                }
            }
            catch (IOException ioEx)
            {
                Log($"IO Error reading file {filePath}: {ioEx.Message}. Attempting to read with retry strategy.", TraceEventType.Warning);

                // Try reading the file with a different approach for locked files
                return ReadExcelFileWithRetry(filePath);
            }
            catch (Exception ex)
            {
                Log($"Error reading Excel file {filePath}: {ex.Message}", TraceEventType.Error);
                throw new InvalidOperationException($"Failed to read Excel file: {ex.Message}", ex);
            }

            Log($"Completed reading Excel file: {filePath}", TraceEventType.Information);
            return dataSet;
        }

        /// <summary>
        /// Attempts to read an Excel file that may be locked by copying it to a temporary location first.
        /// Uses exponential backoff retry strategy for handling FileShare.None locks.
        /// </summary>
        private DataSet ReadExcelFileWithRetry(string filePath)
        {
            const int maxRetries = 5;
            const int baseDelayMs = 500;
            string tempFilePath = "";

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    Log($"Retry attempt {attempt + 1} of {maxRetries}", TraceEventType.Information);

                    // Wait before retry (exponential backoff)
                    if (attempt > 0)
                    {
                        int delay = baseDelayMs * (int)Math.Pow(2, attempt - 1);
                        Log($"Waiting {delay}ms before retry", TraceEventType.Verbose);
                        Thread.Sleep(delay);
                    }

                    // Generate a unique temp file path for each attempt
                    tempFilePath = Path.Combine(Path.GetTempPath(), $"temp_excel_{Guid.NewGuid()}.xlsx");

                    Log($"Copying locked file to temporary location: {tempFilePath}", TraceEventType.Information);

                    // Try to copy the file to temp location to avoid lock issues
                    File.Copy(filePath, tempFilePath, true);

                    // Read from the temp file
                    var result = ReadExcelFile(tempFilePath);

                    Log($"Successfully read Excel file from temporary copy", TraceEventType.Information);
                    return result;
                }
                catch (IOException ioEx) when (attempt < maxRetries - 1)
                {
                    Log($"Retry {attempt + 1} failed: {ioEx.Message}", TraceEventType.Warning);

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
                    Log($"All retry attempts failed. File may be exclusively locked.", TraceEventType.Error);

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
        /// Creates a file (Excel or CSV) with data from a DataSet.
        /// Format is automatically determined by file extension.
        /// </summary>
        /// <param name="outputPath">Path where the file will be saved.</param>
        /// <param name="dataSet">DataSet containing DataTables to export.</param>
        /// <param name="csvDelimiter">Delimiter for CSV files (default: comma).</param>
        /// <param name="csvEncoding">Encoding for CSV files (default: UTF-8).</param>
        public void CreateFile(string outputPath, DataSet dataSet, string csvDelimiter = ",", Encoding? csvEncoding = null)
        {
            var extension = Path.GetExtension(outputPath).ToLowerInvariant();

            Log($"Creating file: {outputPath} (format: {extension})", TraceEventType.Information);

            switch (extension)
            {
                case ".csv":
                    CreateCsvFile(outputPath, dataSet, csvDelimiter, csvEncoding);
                    break;
                case ".xlsx":
                case ".xls":
                    CreateExcelFile(outputPath, dataSet);
                    break;
                default:
                    throw new NotSupportedException($"File format '{extension}' is not supported. Supported formats: .csv, .xlsx, .xls");
            }
        }

        /// <summary>
        /// Creates a file (Excel or CSV) from a single DataTable.
        /// Format is automatically determined by file extension.
        /// </summary>
        /// <param name="outputPath">Path where the file will be saved.</param>
        /// <param name="dataTable">DataTable with data to export.</param>
        /// <param name="sheetName">Sheet name for Excel files (ignored for CSV).</param>
        /// <param name="csvDelimiter">Delimiter for CSV files (default: comma).</param>
        /// <param name="csvEncoding">Encoding for CSV files (default: UTF-8).</param>
        public void CreateFile(string outputPath, DataTable dataTable, string? sheetName = null, string csvDelimiter = ",", Encoding? csvEncoding = null)
        {
            var extension = Path.GetExtension(outputPath).ToLowerInvariant();

            Log($"Creating file from DataTable: {outputPath} (format: {extension})", TraceEventType.Information);

            switch (extension)
            {
                case ".csv":
                    CreateCsvFile(outputPath, dataTable, csvDelimiter, csvEncoding);
                    break;
                case ".xlsx":
                case ".xls":
                    CreateExcelFile(outputPath, dataTable, sheetName);
                    break;
                default:
                    throw new NotSupportedException($"File format '{extension}' is not supported. Supported formats: .csv, .xlsx, .xls");
            }
        }

        /// <summary>
        /// Creates a file (Excel or CSV) from a collection of objects by flattening their properties.
        /// Format is automatically determined by file extension.
        /// </summary>
        /// <param name="outputPath">Path where the file will be saved.</param>
        /// <param name="objects">Collection of objects to flatten and export.</param>
        /// <param name="sheetName">Sheet name for Excel files (ignored for CSV).</param>
        /// <param name="csvDelimiter">Delimiter for CSV files (default: comma).</param>
        /// <param name="csvEncoding">Encoding for CSV files (default: UTF-8).</param>
        public void CreateFileFromObjects(string outputPath, IEnumerable<object> objects, string? sheetName = null, string csvDelimiter = ",", Encoding? csvEncoding = null)
        {
            var extension = Path.GetExtension(outputPath).ToLowerInvariant();
            var actualSheetName = sheetName ?? "Data";

            Log($"Creating file from objects: {outputPath} (format: {extension})", TraceEventType.Information);

            var dataTable = CreateDataTableFromObjects(objects, actualSheetName);

            switch (extension)
            {
                case ".csv":
                    CreateCsvFile(outputPath, dataTable, csvDelimiter, csvEncoding);
                    break;
                case ".xlsx":
                case ".xls":
                    CreateExcelFile(outputPath, dataTable, actualSheetName);
                    break;
                default:
                    throw new NotSupportedException($"File format '{extension}' is not supported. Supported formats: .csv, .xlsx, .xls");
            }
        }

        /// <summary>
        /// Creates a CSV file from a DataSet. If DataSet contains multiple tables, 
        /// only the first table is exported to CSV.
        /// </summary>
        /// <param name="outputPath">Path where the CSV file will be saved.</param>
        /// <param name="dataSet">DataSet containing data to export.</param>
        /// <param name="delimiter">Column delimiter (default: comma).</param>
        /// <param name="encoding">Text encoding (default: UTF-8).</param>
        public void CreateCsvFile(string outputPath, DataSet dataSet, string delimiter = ",", Encoding? encoding = null)
        {
            if (dataSet.Tables.Count == 0)
            {
                Log("DataSet contains no tables, creating empty CSV file.", TraceEventType.Warning);
                File.WriteAllText(outputPath, "", encoding ?? Encoding.UTF8);
                return;
            }

            if (dataSet.Tables.Count > 1)
            {
                Log($"DataSet contains {dataSet.Tables.Count} tables. Only the first table will be exported to CSV.", TraceEventType.Warning);
            }

            CreateCsvFile(outputPath, dataSet.Tables[0], delimiter, encoding);
        }

        /// <summary>
        /// Creates a CSV file from a DataTable.
        /// </summary>
        /// <param name="outputPath">Path where the CSV file will be saved.</param>
        /// <param name="dataTable">DataTable with data to export.</param>
        /// <param name="delimiter">Column delimiter (default: comma).</param>
        /// <param name="encoding">Text encoding (default: UTF-8).</param>
        public void CreateCsvFile(string outputPath, DataTable dataTable, string delimiter = ",", Encoding? encoding = null)
        {
            Log($"Creating CSV file: {outputPath}", TraceEventType.Information);

            var actualEncoding = encoding ?? Encoding.UTF8;
            var csv = new StringBuilder();

            try
            {
                // Write header row
                var headers = dataTable.Columns.Cast<DataColumn>()
                    .Select(column => EscapeCsvValue(column.ColumnName, delimiter));
                csv.AppendLine(string.Join(delimiter, headers));

                Log($"Writing {dataTable.Rows.Count} data rows", TraceEventType.Verbose);

                // Write data rows
                foreach (DataRow row in dataTable.Rows)
                {
                    var values = row.ItemArray.Select(value =>
                    {
                        var stringValue = value?.ToString() ?? "";
                        return EscapeCsvValue(stringValue, delimiter);
                    });
                    csv.AppendLine(string.Join(delimiter, values));
                }

                File.WriteAllText(outputPath, csv.ToString(), actualEncoding);
                Log($"CSV file saved to: {outputPath}", TraceEventType.Information);
            }
            catch (Exception ex)
            {
                Log($"Error creating CSV file {outputPath}: {ex.Message}", TraceEventType.Error);
                throw new InvalidOperationException($"Failed to create CSV file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Escapes a CSV value by wrapping it in quotes if it contains the delimiter, quotes, or newlines.
        /// </summary>
        private string EscapeCsvValue(string value, string delimiter)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // Need to escape if value contains delimiter, quotes, or newlines
            if (value.Contains(delimiter) || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                // Escape internal quotes by doubling them
                var escapedValue = value.Replace("\"", "\"\"");
                return $"\"{escapedValue}\"";
            }

            return value;
        }

        /// <summary>
        /// Creates a basic Excel file with specified sheets and headers.
        /// </summary>
        /// <param name="outputPath">Path where the Excel file will be saved.</param>
        /// <param name="sheets">Dictionary where key is sheet name and value is array of header names.</param>
        public void CreateExcelFile(string outputPath, Dictionary<string, string[]> sheets)
        {
            Log($"Creating Excel file: {outputPath}", TraceEventType.Information);

            foreach (var sheet in sheets)
            {
                var worksheet = _excelPackage.Workbook.Worksheets.Add(sheet.Key);

                // Add headers
                for (int i = 0; i < sheet.Value.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = sheet.Value[i];
                }

                worksheet.Cells.AutoFitColumns();
                Log($"Created sheet '{sheet.Key}' with {sheet.Value.Length} columns.", TraceEventType.Verbose);
            }

            var fileInfo = new FileInfo(outputPath);
            _excelPackage.SaveAs(fileInfo);
            Log($"Excel file saved to: {outputPath}", TraceEventType.Information);
        }

        /// <summary>
        /// Creates an Excel file with data from a DataSet.
        /// </summary>
        /// <param name="outputPath">Path where the Excel file will be saved.</param>
        /// <param name="dataSet">DataSet containing DataTables to export as sheets.</param>
        public void CreateExcelFile(string outputPath, DataSet dataSet)
        {
            Log($"Creating Excel file with DataSet: {outputPath}", TraceEventType.Information);

            using (var package = new ExcelPackage())
            {
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    var sheetName = dataTable.TableName;
                    var worksheet = package.Workbook.Worksheets.Add(sheetName);

                    Log($"Processing sheet '{sheetName}' with {dataTable.Rows.Count} rows and {dataTable.Columns.Count} columns.", TraceEventType.Verbose);

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
                                // Write boolean values as uppercase strings to maintain consistency when reading back
                                worksheet.Cells[excelRow, excelCol].Value = boolValue ? "TRUE" : "FALSE";
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

                    Log($"Completed sheet '{sheetName}' with {dataTable.Rows.Count} data rows.", TraceEventType.Verbose);
                }

                var fileInfo = new FileInfo(outputPath);
                package.SaveAs(fileInfo);
                Log($"Excel file with DataTables saved to: {outputPath}", TraceEventType.Information);
            }
        }

        /// <summary>
        /// Creates an Excel file from a single DataTable.
        /// </summary>
        /// <param name="outputPath">Path where the Excel file will be saved.</param>
        /// <param name="dataTable">DataTable with data to export.</param>
        /// <param name="sheetName">Optional sheet name. Defaults to the DataTable's TableName or "Sheet1".</param>
        public void CreateExcelFile(string outputPath, DataTable dataTable, string? sheetName = null)
        {
            string actualSheetName = "Sheet1";
            if (!string.IsNullOrWhiteSpace(sheetName))
                actualSheetName = sheetName;
            else if (!string.IsNullOrWhiteSpace(dataTable.TableName))
                actualSheetName = dataTable.TableName;


            dataTable.TableName = actualSheetName;

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable.Copy());

            CreateExcelFile(outputPath, dataSet);
        }

        /// <summary>
        /// Creates an Excel file from a collection of objects by flattening their properties into a DataTable schema.
        /// Nested objects are flattened using dot notation (e.g., { "A": { "B": "value" } } becomes column "A.B").
        /// </summary>
        /// <param name="outputPath">Path where the Excel file will be saved.</param>
        /// <param name="objects">Collection of objects to flatten and export.</param>
        /// <param name="sheetName">Optional sheet name. Defaults to "Data".</param>
        public void CreateExcelFileFromObjects(string outputPath, IEnumerable<object> objects, string? sheetName = null)
        {
            var actualSheetName = sheetName ?? "Data";
            Log($"Creating Excel file from objects: {outputPath}", TraceEventType.Information);

            var dataTable = CreateDataTableFromObjects(objects, actualSheetName);
            CreateExcelFile(outputPath, dataTable, actualSheetName);
        }

        /// <summary>
        /// Creates a DataTable from a collection of objects by flattening their properties.
        /// Nested objects are flattened using dot notation (e.g., { "A": { "B": "value" } } becomes column "A.B").
        /// </summary>
        /// <param name="objects">Collection of objects to flatten.</param>
        /// <param name="tableName">Optional table name. Defaults to "Data".</param>
        /// <returns>A DataTable with flattened object properties as columns.</returns>
        public DataTable CreateDataTableFromObjects(IEnumerable<object> objects, string? tableName = null)
        {
            var actualTableName = tableName ?? "Data";
            Log($"Creating DataTable from objects: {actualTableName}", TraceEventType.Information);

            var dataTable = new DataTable(actualTableName);
            var objectList = objects?.ToList() ?? new List<object>();

            if (!objectList.Any())
            {
                Log("No objects provided, returning empty DataTable.", TraceEventType.Warning);
                return dataTable;
            }

            // First pass: collect all possible column names by flattening all objects
            var allColumns = new HashSet<string>();
            var flattenedObjects = new List<Dictionary<string, object>>();

            foreach (var obj in objectList)
            {
                var flattened = FlattenObject(obj);
                flattenedObjects.Add(flattened);

                foreach (var key in flattened.Keys)
                {
                    allColumns.Add(key);
                }
            }

            // Create columns in the DataTable
            foreach (var columnName in allColumns.OrderBy(c => c))
            {
                dataTable.Columns.Add(columnName, typeof(object));
            }

            // Second pass: populate the DataTable with flattened data
            foreach (var flattened in flattenedObjects)
            {
                var row = dataTable.NewRow();

                foreach (var kvp in flattened)
                {
                    if (dataTable.Columns.Contains(kvp.Key))
                    {
                        row[kvp.Key] = kvp.Value ?? DBNull.Value;
                    }
                }

                dataTable.Rows.Add(row);
            }

            Log($"Created DataTable with {dataTable.Columns.Count} columns and {dataTable.Rows.Count} rows.", TraceEventType.Information);
            return dataTable;
        }

        /// <summary>
        /// Flattens an object into a dictionary where nested properties are represented with dot notation.
        /// For example: { "A": { "B": "value" } } becomes { "A.B": "value" }.
        /// </summary>
        /// <param name="obj">The object to flatten.</param>
        /// <param name="prefix">Internal prefix for nested properties.</param>
        /// <returns>A dictionary with flattened key-value pairs.</returns>
        private Dictionary<string, object> FlattenObject(object obj, string prefix = "")
        {
            var result = new Dictionary<string, object>();

            if (obj == null)
            {
                return result;
            }

            var type = obj.GetType();

            // Handle primitive types and strings
            if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) || type == typeof(TimeSpan) || type == typeof(Guid) ||
                type == typeof(decimal) || type.IsEnum)
            {
                var key = string.IsNullOrEmpty(prefix) ? "Value" : prefix;
                result[key] = obj;
                return result;
            }

            // Handle collections (arrays, lists, etc.)
            if (obj is System.Collections.IEnumerable enumerable && !(obj is string))
            {
                int index = 0;
                foreach (var item in enumerable)
                {
                    var itemPrefix = string.IsNullOrEmpty(prefix) ? $"[{index}]" : $"{prefix}[{index}]";
                    var flattenedItem = FlattenObject(item, itemPrefix);

                    foreach (var kvp in flattenedItem)
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                    index++;
                }
                return result;
            }

            // Handle dictionaries
            if (obj is System.Collections.IDictionary dictionary)
            {
                foreach (System.Collections.DictionaryEntry entry in dictionary)
                {
                    var key = entry.Key?.ToString() ?? "null";
                    var itemPrefix = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                    var flattenedValue = FlattenObject(entry.Value, itemPrefix);

                    foreach (var kvp in flattenedValue)
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
                return result;
            }

            // Handle complex objects using reflection
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                 .Where(p => p.CanRead)
                                 .ToArray();

            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(obj);
                    var propertyPrefix = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

                    // Handle null values
                    if (value == null)
                    {
                        result[propertyPrefix] = DBNull.Value;
                        continue;
                    }

                    // Handle primitive properties directly
                    var propertyType = property.PropertyType;
                    if (propertyType.IsPrimitive || propertyType == typeof(string) ||
                        propertyType == typeof(DateTime) || propertyType == typeof(DateTimeOffset) ||
                        propertyType == typeof(TimeSpan) || propertyType == typeof(Guid) ||
                        propertyType == typeof(decimal) || propertyType.IsEnum ||
                        propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        result[propertyPrefix] = value;
                    }
                    else
                    {
                        // Recursively flatten complex properties
                        var flattenedProperty = FlattenObject(value, propertyPrefix);
                        foreach (var kvp in flattenedProperty)
                        {
                            result[kvp.Key] = kvp.Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error accessing property '{property.Name}' on type '{type.Name}': {ex.Message}", TraceEventType.Warning);
                    var propertyPrefix = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    result[propertyPrefix] = $"[Error: {ex.Message}]";
                }
            }

            return result;
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

        public void Dispose()
        {
            _excelPackage.Dispose();
            GC.SuppressFinalize(this); // Ensures that the finalizer is suppressed for this object
        }
    }
}
