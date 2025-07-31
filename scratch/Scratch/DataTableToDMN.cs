using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace Scratch
{
    public class DataTableToDMN : CodedWorkflow
    {
        [Workflow]
        public string ConvertDataTableToDMN(DataTable table, string decisionName = "DecisionTable", string hitPolicy = "UNIQUE")
        {
            if (table.Columns.Count < 2)
                throw new InvalidOperationException("Table must have at least one input and one output column.");

            var inputColumns = new string[table.Columns.Count - 1];
            for (int i = 0; i < inputColumns.Length; i++)
                inputColumns[i] = table.Columns[i].ColumnName;

            var outputColumn = table.Columns[table.Columns.Count - 1].ColumnName;

            var ns = (XNamespace)"https://www.omg.org/spec/DMN/20191111/MODEL/";

            var definitions = new XElement(ns + "definitions",
                new XAttribute("id", "definitions"),
                new XAttribute("name", "GeneratedDMN"),
                new XAttribute("namespace", "https://your.namespace/dmn"),
                new XElement(ns + "decision",
                    new XAttribute("id", decisionName),
                    new XAttribute("name", decisionName),
                    new XElement(ns + "decisionTable",
                        new XAttribute("hitPolicy", hitPolicy),
                        // Inputs
                        GenerateInputClauses(ns, table, inputColumns),
                        // Output
                        new XElement(ns + "output",
                            new XAttribute("name", outputColumn)
                        ),
                        // Rules
                        GenerateRules(ns, table, inputColumns, outputColumn)
                    )
                )
            );

            var document = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), definitions);
            return document.ToString();
        }

        private object GenerateInputClauses(XNamespace ns, DataTable table, string[] inputColumns)
        {
            var clauses = new XElement[inputColumns.Length];
            for (int i = 0; i < inputColumns.Length; i++)
            {
                clauses[i] = new XElement(ns + "input",
                    new XAttribute("id", $"input_{i}"),
                    new XElement(ns + "inputExpression",
                        new XAttribute("id", $"inputExpr_{i}"),
                        new XAttribute("typeRef", "string"), // or infer type
                        new XElement(ns + "text", inputColumns[i])
                    )
                );
            }
            return clauses;
        }

        private object GenerateRules(XNamespace ns, DataTable table, string[] inputColumns, string outputColumn)
        {
            var rules = new XElement[table.Rows.Count];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                var inputEntries = new XElement[inputColumns.Length];

                for (int j = 0; j < inputColumns.Length; j++)
                {
                    var val = row[inputColumns[j]];
                    inputEntries[j] = new XElement(ns + "inputEntry",
                        new XElement(ns + "text", FormatLiteral(val))
                    );
                }

                var outputEntry = new XElement(ns + "outputEntry",
                    new XElement(ns + "text", FormatLiteral(row[outputColumn]))
                );

                rules[i] = new XElement(ns + "rule",
                    inputEntries,
                    outputEntry
                );
            }

            return rules;
        }

        private static string FormatLiteral(object value)
        {
            if (value == null || value == DBNull.Value)
                return "-";
            if (value is string)
                return $"\"{value}\"";
            if (value is bool)
                return value.ToString().ToLower();
            return value.ToString();
        }
    }
}