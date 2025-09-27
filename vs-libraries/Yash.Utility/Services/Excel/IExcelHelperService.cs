using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Yash.Utility.Services.Excel
{
    /// <summary>
    /// Interface for file helper service providing Excel and CSV file operations
    /// </summary>
    public interface IExcelHelperService
    {
        /// <summary>
        /// Reads a file (Excel or CSV) and converts its contents into a DataSet.
        /// Automatically detects file format based on extension.
        /// </summary>
        /// <param name="filePath">Path to the file (Excel or CSV)</param>
        /// <param name="password">Password for protected Excel files (ignored for CSV)</param>
        /// <returns>A DataSet containing the file data as DataTables</returns>
        DataSet ReadFile(string filePath, string password = "");

        /// <summary>
        /// Reads an Excel file and converts its sheets into a DataSet
        /// </summary>
        /// <param name="filePath">Path to the Excel workbook</param>
        /// <param name="password">Password for protected Excel files</param>
        /// <returns>A DataSet containing all sheets as DataTables</returns>
        DataSet ReadExcelFile(string filePath, string password = "");

        /// <summary>
        /// Reads a CSV file and converts it into a DataSet with a single DataTable
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <param name="delimiter">Column delimiter. Auto-detected if not specified</param>
        /// <param name="hasHeaders">Whether the first row contains headers. Auto-detected if not specified</param>
        /// <param name="encoding">Text encoding. UTF-8 if not specified</param>
        /// <returns>A DataSet containing the CSV data as a single DataTable</returns>
        DataSet ReadCsvFile(string filePath, string? delimiter = null, bool? hasHeaders = null, Encoding? encoding = null);

        /// <summary>
        /// Creates a file (Excel or CSV) with data from a DataSet.
        /// Format is automatically determined by file extension.
        /// </summary>
        /// <param name="outputPath">Path where the file will be saved</param>
        /// <param name="dataSet">DataSet containing DataTables to export</param>
        /// <param name="csvDelimiter">Delimiter for CSV files (default: comma)</param>
        /// <param name="csvEncoding">Encoding for CSV files (default: UTF-8)</param>
        void CreateFile(string outputPath, DataSet dataSet, string csvDelimiter = ",", Encoding? csvEncoding = null);

        /// <summary>
        /// Creates a file (Excel or CSV) from a single DataTable.
        /// Format is automatically determined by file extension.
        /// </summary>
        /// <param name="outputPath">Path where the file will be saved</param>
        /// <param name="dataTable">DataTable with data to export</param>
        /// <param name="sheetName">Sheet name for Excel files (ignored for CSV)</param>
        /// <param name="csvDelimiter">Delimiter for CSV files (default: comma)</param>
        /// <param name="csvEncoding">Encoding for CSV files (default: UTF-8)</param>
        void CreateFile(string outputPath, DataTable dataTable, string? sheetName = null, string csvDelimiter = ",", Encoding? csvEncoding = null);

        /// <summary>
        /// Creates a file (Excel or CSV) from a collection of objects by flattening their properties.
        /// Format is automatically determined by file extension.
        /// </summary>
        /// <param name="outputPath">Path where the file will be saved</param>
        /// <param name="objects">Collection of objects to flatten and export</param>
        /// <param name="sheetName">Sheet name for Excel files (ignored for CSV)</param>
        /// <param name="csvDelimiter">Delimiter for CSV files (default: comma)</param>
        /// <param name="csvEncoding">Encoding for CSV files (default: UTF-8)</param>
        void CreateFileFromObjects(string outputPath, IEnumerable<object> objects, string? sheetName = null, string csvDelimiter = ",", Encoding? csvEncoding = null);

        /// <summary>
        /// Creates a basic Excel file with specified sheets and headers
        /// </summary>
        /// <param name="outputPath">Path where the Excel file will be saved</param>
        /// <param name="sheets">Dictionary where key is sheet name and value is array of header names</param>
        void CreateExcelFile(string outputPath, Dictionary<string, string[]> sheets);

        /// <summary>
        /// Creates an Excel file with data from a DataSet
        /// </summary>
        /// <param name="outputPath">Path where the Excel file will be saved</param>
        /// <param name="dataSet">DataSet containing DataTables to export as sheets</param>
        void CreateExcelFile(string outputPath, DataSet dataSet);

        /// <summary>
        /// Creates an Excel file from a single DataTable
        /// </summary>
        /// <param name="outputPath">Path where the Excel file will be saved</param>
        /// <param name="dataTable">DataTable with data to export</param>
        /// <param name="sheetName">Optional sheet name</param>
        void CreateExcelFile(string outputPath, DataTable dataTable, string? sheetName = null);

        /// <summary>
        /// Creates an Excel file from a collection of objects by flattening their properties into a DataTable schema.
        /// Nested objects are flattened using dot notation (e.g., { "A": { "B": "value" } } becomes column "A.B").
        /// </summary>
        /// <param name="outputPath">Path where the Excel file will be saved</param>
        /// <param name="objects">Collection of objects to flatten and export</param>
        /// <param name="sheetName">Optional sheet name. Defaults to "Data"</param>
        void CreateExcelFileFromObjects(string outputPath, IEnumerable<object> objects, string? sheetName = null);

        /// <summary>
        /// Creates a CSV file from a DataSet. If DataSet contains multiple tables,
        /// only the first table is exported to CSV.
        /// </summary>
        /// <param name="outputPath">Path where the CSV file will be saved</param>
        /// <param name="dataSet">DataSet containing data to export</param>
        /// <param name="delimiter">Column delimiter (default: comma)</param>
        /// <param name="encoding">Text encoding (default: UTF-8)</param>
        void CreateCsvFile(string outputPath, DataSet dataSet, string delimiter = ",", Encoding? encoding = null);

        /// <summary>
        /// Creates a CSV file from a DataTable
        /// </summary>
        /// <param name="outputPath">Path where the CSV file will be saved</param>
        /// <param name="dataTable">DataTable with data to export</param>
        /// <param name="delimiter">Column delimiter (default: comma)</param>
        /// <param name="encoding">Text encoding (default: UTF-8)</param>
        void CreateCsvFile(string outputPath, DataTable dataTable, string delimiter = ",", Encoding? encoding = null);

        /// <summary>
        /// Creates a DataTable from a collection of objects by flattening their properties.
        /// Nested objects are flattened using dot notation (e.g., { "A": { "B": "value" } } becomes column "A.B").
        /// </summary>
        /// <param name="objects">Collection of objects to flatten</param>
        /// <param name="tableName">Optional table name. Defaults to "Data"</param>
        /// <returns>A DataTable with flattened object properties as columns</returns>
        DataTable CreateDataTableFromObjects(IEnumerable<object> objects, string? tableName = null);
    }
}