using ClosedXML.Excel;
using System;
using System.Data;
using Yash.Config.Models;
using TraceEventType = System.Diagnostics.TraceEventType;

namespace Yash.Config.Helpers
{
    public static class ExcelHelpers
    {
        /// <summary>
        /// Reads an Excel file and converts its sheets into a DataSet.
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

            using (var workbook = new XLWorkbook(filePath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    Log?.Invoke($"[ExcelHelpers] Processing sheet: {worksheet.Name}", TraceEventType.Verbose);

                    var dataTable = new DataTable(worksheet.Name);
                    bool firstRow = true;

                    foreach (var row in worksheet.RowsUsed())
                    {
                        if (firstRow)
                        {
                            foreach (var cell in row.Cells())
                            {
                                var colName = cell.GetString();
                                if (!dataTable.Columns.Contains(colName))
                                    dataTable.Columns.Add(colName);
                                else
                                    dataTable.Columns.Add(colName + "_1"); // handle duplicate headers
                            }

                            Log?.Invoke($"[ExcelHelpers] Header columns: {string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}", TraceEventType.Verbose);
                            firstRow = false;
                        }
                        else
                        {
                            var dataRow = dataTable.NewRow();
                            int i = 0;
                            foreach (var cell in row.Cells(1, dataTable.Columns.Count))
                            {
                                dataRow[i++] = string.IsNullOrWhiteSpace(cell.GetString()) ? DBNull.Value : cell.Value;
                            }
                            dataTable.Rows.Add(dataRow);
                        }
                    }

                    Log?.Invoke($"[ExcelHelpers] Sheet '{worksheet.Name}' loaded with {dataTable.Rows.Count} rows and {dataTable.Columns.Count} columns.", TraceEventType.Information);
                    dataSet.Tables.Add(dataTable);
                }
            }

            Log?.Invoke($"[ExcelHelpers] Completed reading Excel file: {filePath}", TraceEventType.Information);
            return dataSet;
        }

        public static void GenerateExcelTemplate(string outputPath)
        {
            var workbook = new XLWorkbook();

            // 1. _Instructions Sheet
            var instructions = workbook.Worksheets.Add("_Instructions");
            instructions.Cell("A1").Value = "Welcome to the Config Template!";
            instructions.Cell("A3").Value = "This file is used to define your automation settings, assets, and file references.";
            instructions.Cell("A5").Value = "Use the Yash.Config wizards in UiPath to generate strongly-typed C# classes based on this file.";
            instructions.Columns().AdjustToContents();

            // 2. _ConfigFileSettings Sheet (defines types)
            var typeSheet = workbook.Worksheets.Add("_ConfigFileSettings");
            typeSheet.Cell("A1").Value = "SupportedTypes";
            var supportedTypes = TypeParsers.Parsers;
            foreach (var type in supportedTypes.Keys)
            {
                var i = supportedTypes.Keys.ToList().IndexOf(type);
                typeSheet.Cell(i + 2, 1).Value = GetFriendlyTypeName(type);
            }

            var typeRange = typeSheet.Range($"A2:A{supportedTypes.Count + 1}");
            typeRange.CreateTable("TypeOptions");

            // 3. Settings / Assets / Files with type dropdown
            void AddConfigSheet(string name)
            {
                var ws = workbook.Worksheets.Add(name);
                ws.Cell("A1").Value = "Name";
                ws.Cell("B1").Value = "Value / Path";
                ws.Cell("C1").Value = "Type";
                ws.Cell("D1").Value = "Description";

                // Apply validation to column C (Type)
                var typeListRange = typeSheet.Range("A2:A" + (supportedTypes.Count + 1));
                var firstDataRow = 2;
                var maxRows = 100;

                for (int i = 0; i < maxRows; i++)
                {
                    var cell = ws.Cell(i + firstDataRow, 3);
                    var validation = cell.CreateDataValidation();
                    validation.List(typeListRange);
                    validation.IgnoreBlanks = false;
                    validation.InCellDropdown = true;
                }

                ws.Columns().AdjustToContents();
            }

            AddConfigSheet("Settings");
            AddConfigSheet("Assets");
            AddConfigSheet("Files");

            // Save file
            workbook.SaveAs(outputPath);
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
                return $"{GetFriendlyTypeName(type.GetElementType())}[]";
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
