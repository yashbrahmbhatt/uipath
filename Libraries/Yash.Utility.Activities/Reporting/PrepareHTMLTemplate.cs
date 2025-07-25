using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using PreMailer.Net;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.Utility.Activities.Reporting
{
    public class PrepareHTMLTemplate : CodedWorkflow
    {
        [Workflow]
        public (string out_str_Body, string out_str_Subject) Execute(
            string in_str_Template,
            Dictionary<string, object> in_dict_Data,
            Dictionary<string, Func<object, string>> in_dict_Serializers = null)
        {
            var list_TemplateKeys = HTMLUtilities.GetTemplateKeys(in_str_Template);

            Log($"Preparing template with {list_TemplateKeys.Count} variables using {in_dict_Data.Keys.Count} keys");

            if (list_TemplateKeys.Count > in_dict_Data.Count)
                Log($"Some templated variables are not provided", LogLevel.Warn);

            foreach (var key in in_dict_Data.Keys)
            {
                string placeholder = $"{{{{{key}}}}}";
                object value = in_dict_Data[key];

                string replacement = null;

                // 1. Caller-provided serializer
                if (in_dict_Serializers != null && in_dict_Serializers.TryGetValue(key, out var customSerializer))
                {
                    try
                    {
                        replacement = customSerializer?.Invoke(value);
                    }
                    catch (Exception ex)
                    {
                        Log($"Error in custom serializer for key '{key}': {ex.Message}", LogLevel.Warn);
                    }
                }

                // 2. Fallback default serializer
                if (replacement == null)
                    replacement = SerializeWithFallback(value);

                replacement ??= string.Empty;

                in_str_Template = in_str_Template.Replace(placeholder, replacement);
            }
            // 1. Extract :root block
            var rootVars = new Dictionary<string, string>();
            var rootMatch = Regex.Match(in_str_Template, @":root\s*{([^}]*)}", RegexOptions.Multiline);

            if (rootMatch.Success)
            {
                var body = rootMatch.Groups[1].Value;

                foreach (Match varMatch in Regex.Matches(body, @"--([\w-]+)\s*:\s*(.*?);"))
                {
                    var key = varMatch.Groups[1].Value.Trim();
                    var value = varMatch.Groups[2].Value.Trim();
                    rootVars[key] = value;
                }

                // 2. Replace var(--key) with value in entire HTML
                foreach (var kvp in rootVars)
                {
                    in_str_Template = Regex.Replace(in_str_Template, $@"var\(--{Regex.Escape(kvp.Key)}\)", kvp.Value, RegexOptions.IgnoreCase);
                }

                // 3. Remove :root block entirely
                in_str_Template = Regex.Replace(in_str_Template, @":root\s*{[^}]*}", "", RegexOptions.Multiline);
            }
            // Now extract title AFTER variable substitution
            var subject = HTMLUtilities.GetTitleOrDefault(in_str_Template);
            var premailer = new PreMailer.Net.PreMailer(in_str_Template);
            var result = premailer.MoveCssInline(true);

            return (result.Html, subject);
        }

        private string SerializeWithFallback(object value)
        {
            if (value is null) return string.Empty;

            switch (value)
            {
                case DateTime dt:
                    return dt.ToString("MMM dd, yyyy HH:mm:ss");

                case DataTable dtTable:
                    return workflows.ConvertDataTableToHTML(dtTable, new());

                default:
                    return value.ToString();
            }
        }
    }



}