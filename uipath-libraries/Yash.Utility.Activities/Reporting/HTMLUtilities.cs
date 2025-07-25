using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using UiPath.Core;
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

namespace Yash.Utility.Activities.Reporting
{
    public static class HTMLUtilities
    {
        /// <summary>
        /// Retrieves the content of the <title> tag under the <html> tag in the given HTML content.
        /// If no <title> is found, returns a default error message.
        /// </summary>
        /// <param name="html">The HTML string to parse.</param>
        /// <returns>The content of the <title> tag or a default error message if not found.</returns>
        public static string GetTitleOrDefault(string html, string defaultString = @"Error: No Subject can be created because no ""<title>"" tag was found under the ""<html>"" tag.")
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return defaultString;
            }

            // Regex to match <title>...</title> inside <html>...</html>
            var match = Regex.Match(
                html,
                @"<html.*?>.*?<title>(.*?)<\/title>.*?<\/html>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase
            );

            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value.Trim();
            }

            return defaultString;
        }

        /// <summary>
        /// Extracts all unique keys in the format {{Key}} from an HTML template string.
        /// </summary>
        /// <param name="htmlTemplate">The HTML template containing keys like {{Key}}.</param>
        /// <returns>A list of unique key names found in the template.</returns>
        public static List<string> GetTemplateKeys(string htmlTemplate)
        {
            if (string.IsNullOrWhiteSpace(htmlTemplate))
                return new List<string>();

            // Match any {{Key}} with optional whitespace inside
            var matches = Regex.Matches(htmlTemplate, @"\{\{\s*(\w+)\s*\}\}");

            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string key = match.Groups[1].Value.Trim();
                    keys.Add(key);
                }
            }

            return new List<string>(keys);
        }
    }
}