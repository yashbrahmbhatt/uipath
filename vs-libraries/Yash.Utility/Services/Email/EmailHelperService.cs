using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UiPath.Core;
using UiPath.Robot.Activities.Api;
using Yash.Utility.Services.Email.Models;

namespace Yash.Utility.Services.Email
{
    public class EmailHelperService : Base.BaseService, IEmailHelperService
    {

        /// <summary>
        /// Creates a new EmailHelperService instance
        /// </summary>
        /// <param name="log">Optional logging action</param>
        public EmailHelperService(Action<string, TraceEventType>? log, TraceEventType minLevel = TraceEventType.Information) : base(null, log, minLevel)
        {
        }

        public string ToHtmlTable(DataTable table, DataTableToHTMLOptions? options = null)
        {
            Log($"Converting DataTable with {table.Rows.Count} rows and {table.Columns.Count} columns to HTML table.");
            if (options == null) options = new();

            // Log options details without serializing the function delegates
            Log($"Using HTML table options with {options.Serializers.Count} custom serializers", TraceEventType.Verbose);

            string html = options.TablePrefix + options.HeaderRowPrefix;

            foreach (DataColumn col in table.Columns)
            {
                html += options.HeaderCellPrefix + col.ColumnName + options.HeaderCellSuffix;

                if (!options.Serializers.ContainsKey(col.ColumnName))
                {
                    options.Serializers.Add(col.ColumnName, (o) => o.ToString());
                }
            }

            html += options.HeaderRowSuffix;

            foreach (DataRow row in table.Rows)
            {
                html += options.BodyRowPrefix;

                foreach (DataColumn col in table.Columns)
                {
                    html += options.BodyCellPrefix + options.Serializers[col.ColumnName].Invoke(row[col]) + options.BodyCellSuffix;
                }

                html += options.BodyRowSuffix;
            }

            html += options.TableSuffix;
            Log($"Generated HTML table with length {html.Length} characters.", TraceEventType.Verbose);
            return html;
        }
        public Dictionary<string, object> GenerateDiagnosticDictionary(Exception? ex = null, QueueItem? transaction = null, IRunningJobInformation? jobInfo = null)
        {
            Dictionary<string, object> dict = new();
            var now = DateTime.Now;

            dict["Env_MachineName"] = Environment.MachineName;
            dict["Env_User"] = Environment.UserName;
            dict["Env_Date"] = now.ToLongDateString();
            dict["Env_Time"] = now.ToLongTimeString();
            dict["Today"] = now.ToString("dd-MM-yyyy");

            if (ex != null)
            {
                dict["Ex_Message"] = ex.Message;
                dict["Ex_Source"] = ex.Source;
                dict["Ex_Stack"] = ex.StackTrace;
                dict["Ex_Type"] =
                ex is BusinessRuleException ?
                    nameof(BusinessRuleException) :
                    ex is Exception ?
                        "System" :
                        "Unknow";
                dict["Ex_Data"] = JsonConvert.SerializeObject(ex.Data);
            }

            if (transaction != null)
            {
                dict["Transaction_Reference"] = transaction.Reference;
                dict["Transaction_SpecificData"] = JsonConvert.SerializeObject(transaction.SpecificContent);
            }
            if (jobInfo != null)
            {
                if (transaction != null)
                {

                }
            }
            return dict;
        }
        public static class GenerateDiagnosticDictionaryKeys
        {
            public const string Env_MachineName = "Env_MachineName";
            public const string Env_User = "Env_User";
            public const string Env_Date = "Env_Date";
            public const string Env_Time = "Env_Time";
            public const string Today = "Today";
            public const string Ex_Message = "Ex_Message";
            public const string Ex_Source = "Ex_Source";
            public const string Ex_Stack = "Ex_Stack";
            public const string Ex_Type = "Ex_Type";
            public const string Ex_Data = "Ex_Data";
            public const string Transaction_Reference = "Transaction_Reference";
            public const string Transaction_SpecificData = "Transaction_SpecificData";
        }
        public static class DefaultEmailTemplates
        {
            public const string BusEx =
                @"<html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>[{ProcessName}] Business Rule Exception - Transaction {Reference}</title>
                    <style>
                        * {
                            margin: 0;
                            padding: 0;
                            box-sizing: border-box;
                        }
        
                        body {
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            line-height: 1.6;
                            color: #333;
                            background: linear-gradient(135deg, #ffeaa7 0%, #fab1a0 100%);
                            min-height: 100vh;
                            padding: 20px;
                        }
        
                        .container {
                            max-width: 900px;
                            margin: 0 auto;
                            background: white;
                            border-radius: 12px;
                            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
                            overflow: hidden;
                        }
        
                        .header {
                            background: linear-gradient(135deg, #f39c12 0%, #e67e22 100%);
                            color: white;
                            padding: 30px;
                            text-align: center;
                        }
        
                        .header h1 {
                            font-size: 1.8em;
                            font-weight: 600;
                            margin-bottom: 10px;
                            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
                        }
        
                        .alert-icon {
                            font-size: 3em;
                            margin-bottom: 15px;
                            opacity: 0.9;
                        }
        
                        .content {
                            padding: 0;
                        }
        
                        .section {
                            padding: 25px 30px;
                            border-bottom: 1px solid #e9ecef;
                        }
        
                        .section:last-child {
                            border-bottom: none;
                        }
        
                        .section h3 {
                            color: #495057;
                            font-size: 1.3em;
                            margin-bottom: 15px;
                            display: flex;
                            align-items: center;
                            gap: 10px;
                        }
        
                        .section-icon {
                            font-size: 1.2em;
                            width: 30px;
                            height: 30px;
                            border-radius: 50%;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            color: white;
                        }
        
                        .transaction-icon {
                            background: #17a2b8;
                        }
        
                        .environment-icon {
                            background: #28a745;
                        }
        
                        .exception-icon {
                            background: #f39c12;
                        }
        
                        .detail-card {
                            background: #f8f9fa;
                            border-radius: 8px;
                            padding: 20px;
                            border-left: 4px solid #f39c12;
                            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
                        }
        
                        .detail-item {
                            display: flex;
                            padding: 8px 0;
                            border-bottom: 1px solid #e9ecef;
                        }
        
                        .detail-item:last-child {
                            border-bottom: none;
                            padding-bottom: 0;
                        }
        
                        .detail-item:first-child {
                            padding-top: 0;
                        }
        
                        .detail-label {
                            font-weight: 600;
                            color: #495057;
                            min-width: 120px;
                            flex-shrink: 0;
                        }
        
                        .detail-value {
                            color: #6c757d;
                            word-break: break-all;
                            font-family: 'Consolas', 'Monaco', monospace;
                        }
        
                        .business-notice {
                            background: linear-gradient(135deg, #3498db 0%, #2980b9 100%);
                            color: white;
                            padding: 20px;
                            margin: 20px 30px;
                            border-radius: 8px;
                            text-align: center;
                            font-weight: 500;
                            box-shadow: 0 4px 15px rgba(52, 152, 219, 0.3);
                        }
        
                        .message-highlight {
                            background: linear-gradient(135deg, #fff3cd 0%, #ffeaa7 100%);
                            border: 1px solid #f39c12;
                            border-radius: 8px;
                            padding: 20px;
                            margin: 15px 0;
                            font-size: 1.1em;
                            font-weight: 500;
                            color: #856404;
                        }
        
                        .footer {
                            background: #f8f9fa;
                            padding: 20px 30px;
                            text-align: center;
                            color: #6c757d;
                            font-size: 0.9em;
                        }
        
                        @media (max-width: 768px) {
                            .container {
                                margin: 10px;
                                border-radius: 8px;
                            }
            
                            .header {
                                padding: 20px;
                            }
            
                            .section {
                                padding: 20px;
                            }
            
                            .business-notice {
                                margin: 20px;
                            }
            
                            .detail-item {
                                flex-direction: column;
                                gap: 8px;
                                padding: 8px 0;
                                border-bottom: 1px solid #dee2e6;
                            }
            
                            .detail-item:last-child {
                                border-bottom: none;
                            }
            
                            .detail-label {
                                min-width: auto;
                            }
                        }
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <div class=""alert-icon"">📋</div>
                            <h1>Business Rule Exception</h1>
                            <p>{ProcessName} encountered a business rule exception while processing transaction '{Reference}'.<br>Review and handle as required.</p>
                        </div>
        
                        <div class=""business-notice"">
                            ℹ️ This exception indicates a business logic validation failure that may require manual review
                        </div>
        
                        <div class=""content"">
                            <div class=""section"">
                                <h3>
                                    <span class=""section-icon transaction-icon"">📋</span>
                                    Transaction Details
                                </h3>
                                <div class=""detail-card"">
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Reference:</span>
                                        <span class=""detail-value"">{Reference}</span>
                                    </div>
                                </div>
                            </div>
            
                            <div class=""section"">
                                <h3>
                                    <span class=""section-icon exception-icon"">⚠️</span>
                                    Exception Details
                                </h3>
                                <div class=""message-highlight"">
                                    <strong>Business Rule Message:</strong><br>
                                    {Ex_Message}
                                </div>
                            </div>
            
                            <div class=""section"">
                                <h3>
                                    <span class=""section-icon environment-icon"">🖥️</span>
                                    Environment Details
                                </h3>
                                <div class=""detail-card"">
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Machine:</span>
                                        <span class=""detail-value"">{Env_MachineName}</span>
                                    </div>
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">User:</span>
                                        <span class=""detail-value"">{Env_User}</span>
                                    </div>
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Date:</span>
                                        <span class=""detail-value"">{Env_Date}</span>
                                    </div>
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Time:</span>
                                        <span class=""detail-value"">{Env_Time}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
        
                        <div class=""footer"">
                            Generated by UiPath Automation Platform
                        </div>
                    </div>
                </body>
                </html>
                ";
            public const string SysEx =
                @"<!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>[{ProcessName}] System Exception - Transaction {Reference}</title>
                    <style>
                        * {
                            margin: 0;
                            padding: 0;
                            box-sizing: border-box;
                        }
        
                        body {
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            line-height: 1.6;
                            color: #333;
                            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
                            min-height: 100vh;
                            padding: 20px;
                        }
        
                        .container {
                            max-width: 900px;
                            margin: 0 auto;
                            background: white;
                            border-radius: 12px;
                            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
                            overflow: hidden;
                        }
        
                        .header {
                            background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
                            color: white;
                            padding: 30px;
                            text-align: center;
                        }
        
                        .header h1 {
                            font-size: 1.8em;
                            font-weight: 600;
                            margin-bottom: 10px;
                            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
                        }
        
                        .alert-icon {
                            font-size: 3em;
                            margin-bottom: 15px;
                            opacity: 0.9;
                        }
        
                        .content {
                            padding: 0;
                        }
        
                        .section {
                            padding: 25px 30px;
                            border-bottom: 1px solid #e9ecef;
                        }
        
                        .section:last-child {
                            border-bottom: none;
                        }
        
                        .section h3 {
                            color: #495057;
                            font-size: 1.3em;
                            margin-bottom: 15px;
                            display: flex;
                            align-items: center;
                            gap: 10px;
                        }
        
                        .section-icon {
                            font-size: 1.2em;
                            width: 30px;
                            height: 30px;
                            border-radius: 50%;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            color: white;
                        }
        
                        .transaction-icon {
                            background: #17a2b8;
                        }
        
                        .environment-icon {
                            background: #28a745;
                        }
        
                        .exception-icon {
                            background: #dc3545;
                        }
        
                        .detail-card {
                            background: #f8f9fa;
                            border-radius: 8px;
                            padding: 20px;
                            border-left: 4px solid #007bff;
                            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
                        }
        
                        .detail-item {
                            display: flex;
                            padding: 8px 0;
                            border-bottom: 1px solid #e9ecef;
                        }
        
                        .detail-item:last-child {
                            border-bottom: none;
                            padding-bottom: 0;
                        }
        
                        .detail-item:first-child {
                            padding-top: 0;
                        }
        
                        .detail-label {
                            font-weight: 600;
                            color: #495057;
                            min-width: 120px;
                            flex-shrink: 0;
                        }
        
                        .detail-value {
                            color: #6c757d;
                            word-break: break-all;
                            font-family: 'Consolas', 'Monaco', monospace;
                        }
        
                        .stack-trace {
                            background: #f1f3f4;
                            padding: 15px;
                            border-radius: 6px;
                            font-family: 'Consolas', 'Monaco', monospace;
                            font-size: 0.9em;
                            white-space: pre-wrap;
                            max-height: 300px;
                            overflow-y: auto;
                            border: 1px solid #dee2e6;
                        }
        
                        .footer {
                            background: #f8f9fa;
                            padding: 20px 30px;
                            text-align: center;
                            color: #6c757d;
                            font-size: 0.9em;
                        }
        
                        @media (max-width: 768px) {
                            .container {
                                margin: 10px;
                                border-radius: 8px;
                            }
            
                            .header {
                                padding: 20px;
                            }
            
                            .section {
                                padding: 20px;
                            }
            
                            .detail-item {
                                flex-direction: column;
                                gap: 8px;
                                padding: 8px 0;
                                border-bottom: 1px solid #dee2e6;
                            }
            
                            .detail-item:last-child {
                                border-bottom: none;
                            }
            
                            .detail-label {
                                min-width: auto;
                            }
                        }
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <div class=""alert-icon"">⚠️</div>
                            <h1>System Exception Detected</h1>
                            <p>{ProcessName} encountered an unexpected error while processing transaction '{Reference}'.<br>Manual intervention required.</p>
                        </div>
        
                        <div class=""content"">
                            <div class=""section"">
                                <h3>
                                    <span class=""section-icon transaction-icon"">📋</span>
                                    Transaction Details
                                </h3>
                                <div class=""detail-card"">
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Reference:</span>
                                        <span class=""detail-value"">{Reference}</span>
                                    </div>
                                </div>
                            </div>
            
                            <div class=""section"">
                                <h3>
                                    <span class=""section-icon environment-icon"">🖥️</span>
                                    Environment Details
                                </h3>
                                <div class=""detail-card"">
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Machine:</span>
                                        <span class=""detail-value"">{Env_MachineName}</span>
                                    </div>
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">User:</span>
                                        <span class=""detail-value"">{Env_User}</span>
                                    </div>
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Date:</span>
                                        <span class=""detail-value"">{Env_Date}</span>
                                    </div>
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Time:</span>
                                        <span class=""detail-value"">{Env_Time}</span>
                                    </div>
                                </div>
                            </div>
            
                            <div class=""section"">
                                <h3>
                                    <span class=""section-icon exception-icon"">🚨</span>
                                    Exception Details
                                </h3>
                                <div class=""detail-card"">
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Message:</span>
                                        <span class=""detail-value"">{Ex_Message}</span>
                                    </div>
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Source:</span>
                                        <span class=""detail-value"">{Ex_Source}</span>
                                    </div>
                                </div>
                                <div style=""margin-top: 15px;"">
                                    <div class=""detail-label"" style=""margin-bottom: 8px;"">Stack Trace:</div>
                                    <div class=""stack-trace"">{Ex_Stack}</div>
                                </div>
                            </div>
                        </div>
        
                        <div class=""footer"">
                            Generated by UiPath Automation Platform
                        </div>
                    </div>
                </body>
                </html>";
            public const string FrameEx =
                @"<!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>[{ProcessName}] Critical Framework Exception</title>
                    <style>
                        * {
                            margin: 0;
                            padding: 0;
                            box-sizing: border-box;
                        }
        
                        body {
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            line-height: 1.6;
                            color: #333;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            min-height: 100vh;
                            padding: 20px;
                        }
        
                        .container {
                            max-width: 900px;
                            margin: 0 auto;
                            background: white;
                            border-radius: 12px;
                            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
                            overflow: hidden;
                        }
        
                        .header {
                            background: linear-gradient(135deg, #6f42c1 0%, #5a2d81 100%);
                            color: white;
                            padding: 30px;
                            text-align: center;
                        }
        
                        .header h1 {
                            font-size: 2em;
                            font-weight: 600;
                            margin-bottom: 10px;
                            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
                        }
        
                        .alert-icon {
                            font-size: 3.5em;
                            margin-bottom: 15px;
                            opacity: 0.9;
                            animation: pulse 2s infinite;
                        }
        
                        @keyframes pulse {
                            0% { transform: scale(1); }
                            50% { transform: scale(1.05); }
                            100% { transform: scale(1); }
                        }
        
                        .content {
                            padding: 0;
                        }
        
                        .section {
                            padding: 25px 30px;
                            border-bottom: 1px solid #e9ecef;
                        }
        
                        .section:last-child {
                            border-bottom: none;
                        }
        
                        .section h3 {
                            color: #495057;
                            font-size: 1.3em;
                            margin-bottom: 15px;
                            display: flex;
                            align-items: center;
                            gap: 10px;
                        }
        
                        .section-icon {
                            font-size: 1.2em;
                            width: 30px;
                            height: 30px;
                            border-radius: 50%;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            color: white;
                        }
        
                        .environment-icon {
                            background: #28a745;
                        }
        
                        .exception-icon {
                            background: #6f42c1;
                        }
        
                        .detail-card {
                            background: #f8f9fa;
                            border-radius: 8px;
                            padding: 20px;
                            border-left: 4px solid #6f42c1;
                            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
                        }
        
                        .detail-item {
                            display: flex;
                            padding: 8px 0;
                            border-bottom: 1px solid #e9ecef;
                        }
        
                        .detail-item:last-child {
                            border-bottom: none;
                            padding-bottom: 0;
                        }
        
                        .detail-item:first-child {
                            padding-top: 0;
                        }
        
                        .detail-label {
                            font-weight: 600;
                            color: #495057;
                            min-width: 120px;
                            flex-shrink: 0;
                        }
        
                        .detail-value {
                            color: #6c757d;
                            word-break: break-all;
                            font-family: 'Consolas', 'Monaco', monospace;
                        }
        
                        .stack-trace {
                            background: #f1f3f4;
                            padding: 15px;
                            border-radius: 6px;
                            font-family: 'Consolas', 'Monaco', monospace;
                            font-size: 0.9em;
                            white-space: pre-wrap;
                            max-height: 300px;
                            overflow-y: auto;
                            border: 1px solid #dee2e6;
                        }
        
                        .critical-notice {
                            background: linear-gradient(135deg, #ff6b6b 0%, #ee5a52 100%);
                            color: white;
                            padding: 20px;
                            margin: 20px 30px;
                            border-radius: 8px;
                            text-align: center;
                            font-weight: 600;
                            box-shadow: 0 4px 15px rgba(255, 107, 107, 0.3);
                        }
        
                        .footer {
                            background: #f8f9fa;
                            padding: 20px 30px;
                            text-align: center;
                            color: #6c757d;
                            font-size: 0.9em;
                        }
        
                        @media (max-width: 768px) {
                            .container {
                                margin: 10px;
                                border-radius: 8px;
                            }
            
                            .header {
                                padding: 20px;
                            }
            
                            .section {
                                padding: 20px;
                            }
            
                            .critical-notice {
                                margin: 20px;
                            }
            
                            .detail-item {
                                flex-direction: column;
                                gap: 8px;
                                padding: 8px 0;
                                border-bottom: 1px solid #dee2e6;
                            }
            
                            .detail-item:last-child {
                                border-bottom: none;
                            }
            
                            .detail-label {
                                min-width: auto;
                            }
                        }
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <div class=""alert-icon"">🛑</div>
                            <h1>Critical Framework Exception</h1>
                            <p>{ProcessName} has encountered an unexpected critical error</p>
                        </div>
        
                        <div class=""critical-notice"">
                            ⚠️ CRITICAL: This is a framework-level exception that may require immediate attention
                        </div>
        
                        <div class=""content"">
                            <div class=""section"">
                                <h3>
                                    <span class=""section-icon environment-icon"">🖥️</span>
                                    Environment Details
                                </h3>
                                <div class=""detail-card"">
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Machine:</span>
                                        <span class=""detail-value"">{Env_MachineName}</span>
                                    </div>
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">User:</span>
                                        <span class=""detail-value"">{Env_User}</span>
                                    </div>
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Date:</span>
                                        <span class=""detail-value"">{Env_Date}</span>
                                    </div>
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Time:</span>
                                        <span class=""detail-value"">{Env_Time}</span>
                                    </div>
                                </div>
                            </div>
            
                            <div class=""section"">
                                <h3>
                                    <span class=""section-icon exception-icon"">💥</span>
                                    Exception Details
                                </h3>
                                <div class=""detail-card"">
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Message:</span>
                                        <span class=""detail-value"">{Ex_Message}</span>
                                    </div>
                                    <div class=""detail-item"">
                                        <span class=""detail-label"">Source:</span>
                                        <span class=""detail-value"">{Ex_Source}</span>
                                    </div>
                                </div>
                                <div style=""margin-top: 15px;"">
                                    <div class=""detail-label"" style=""margin-bottom: 8px;"">Stack Trace:</div>
                                    <div class=""stack-trace"">{Ex_Stack}</div>
                                </div>
                            </div>
                        </div>
        
                        <div class=""footer"">
                            Generated by UiPath Automation Platform
                        </div>
                    </div>
                </body>
                </html>
";
            public const string Report =
                @"<!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>[{ProcessName}] Process Report - {Today}</title>
                    <style>
                        * {
                            margin: 0;
                            padding: 0;
                            box-sizing: border-box;
                        }
        
                        body {
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            line-height: 1.6;
                            color: #333;
                            background: linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%);
                            min-height: 100vh;
                            padding: 20px;
                        }
        
                        .container {
                            max-width: 900px;
                            margin: 0 auto;
                            background: white;
                            border-radius: 12px;
                            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
                            overflow: hidden;
                        }
        
                        .header {
                            background: linear-gradient(135deg, #2196f3 0%, #1976d2 100%);
                            color: white;
                            padding: 30px;
                            text-align: center;
                        }
        
                        .header h1 {
                            font-size: 2em;
                            font-weight: 600;
                            margin-bottom: 10px;
                            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
                        }
        
                        .report-icon {
                            font-size: 3em;
                            margin-bottom: 15px;
                            opacity: 0.9;
                        }
        
                        .content {
                            padding: 0;
                        }
        
                        .section {
                            padding: 25px 30px;
                            border-bottom: 1px solid #e9ecef;
                        }
        
                        .section:last-child {
                            border-bottom: none;
                        }
        
                        .section h3 {
                            color: #495057;
                            font-size: 1.3em;
                            margin-bottom: 15px;
                            display: flex;
                            align-items: center;
                            gap: 10px;
                        }
        
                        .section-icon {
                            font-size: 1.2em;
                            width: 30px;
                            height: 30px;
                            border-radius: 50%;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            color: white;
                        }
        
                        .overview-icon {
                            background: #2196f3;
                        }
        
                        .summary-icon {
                            background: #4caf50;
                        }
        
                        .detail-card {
                            background: #f8f9fa;
                            border-radius: 8px;
                            padding: 20px;
                            border-left: 4px solid #2196f3;
                            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
                        }
        
                        .intro-text {
                            color: #6c757d;
                            font-size: 1.1em;
                            margin-bottom: 20px;
                            line-height: 1.7;
                        }
        
                        .stats-grid {
                            display: grid;
                            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
                            gap: 15px;
                            margin-top: 15px;
                        }
        
                        .stat-item {
                            background: white;
                            padding: 20px;
                            border-radius: 8px;
                            text-align: center;
                            border: 1px solid #e9ecef;
                            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
                            transition: transform 0.2s ease;
                        }
        
                        .stat-item:hover {
                            transform: translateY(-2px);
                            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                        }
        
                        .stat-number {
                            font-size: 2em;
                            font-weight: 700;
                            color: #2196f3;
                            display: block;
                            margin-bottom: 5px;
                        }
        
                        .stat-label {
                            color: #6c757d;
                            font-weight: 500;
                            font-size: 0.9em;
                            text-transform: uppercase;
                            letter-spacing: 0.5px;
                        }
        
                        .success {
                            color: #28a745 !important;
                        }
        
                        .failed {
                            color: #dc3545 !important;
                        }
        
                        .summary-table-container {
                            background: #f8f9fa;
                            border-radius: 8px;
                            padding: 20px;
                            border-left: 4px solid #4caf50;
                            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
                            overflow-x: auto;
                        }
        
                        .summary-table-container table {
                            width: 100%;
                            border-collapse: collapse;
                            background: white;
                            border-radius: 6px;
                            overflow: hidden;
                            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
                        }
        
                        .summary-table-container th {
                            background: #2196f3;
                            color: white;
                            padding: 12px;
                            text-align: left;
                            font-weight: 600;
                        }
        
                        .summary-table-container td {
                            padding: 12px;
                            border-bottom: 1px solid #e9ecef;
                        }
        
                        .summary-table-container tr:hover {
                            background: #f8f9fa;
                        }
        
                        .footer {
                            background: #f8f9fa;
                            padding: 20px 30px;
                            text-align: center;
                            color: #6c757d;
                            font-size: 0.9em;
                        }
        
                        @media (max-width: 768px) {
                            .container {
                                margin: 10px;
                                border-radius: 8px;
                            }
            
                            .header {
                                padding: 20px;
                            }
            
                            .section {
                                padding: 20px;
                            }
            
                            .stats-grid {
                                grid-template-columns: 1fr;
                            }
            
                            .summary-table-container {
                                padding: 15px;
                            }
                        }
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <div class=""report-icon"">📊</div>
                            <h1>Process Report</h1>
                            <p>{ProcessName} - {Today}</p>
                        </div>
        
                        <div class=""content"">
                            <div class=""section"">
                                <h3>
                                    <span class=""section-icon overview-icon"">📋</span>
                                    Process Overview
                                </h3>
                                <div class=""detail-card"">
                                    <p class=""intro-text"">
                                        Hello, please find below the comprehensive report for {Today}. Here is a quick summary of the automation execution:
                                    </p>
                    
                                    <div class=""stats-grid"">
                                        <div class=""stat-item"">
                                            <span class=""stat-number"">{TotalCount}</span>
                                            <span class=""stat-label"">Total Transactions</span>
                                        </div>
                                        <div class=""stat-item"">
                                            <span class=""stat-number success"">{SuccessCount}</span>
                                            <span class=""stat-label"">Successful</span>
                                        </div>
                                        <div class=""stat-item"">
                                            <span class=""stat-number failed"">{FailedCount}</span>
                                            <span class=""stat-label"">Failed</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
            
                            <div class=""section"">
                                <h3>
                                    <span class=""section-icon summary-icon"">📈</span>
                                    Detailed Summary
                                </h3>
                                <div class=""summary-table-container"">
                                    {SummaryTable}
                                </div>
                            </div>
                        </div>
        
                        <div class=""footer"">
                            Generated by UiPath Automation Platform
                        </div>
                    </div>
                </body>
                </html>";
        }
        public void DumpEmailTemplates(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            File.WriteAllText(Path.Combine(folderPath, "BusEx.html"), DefaultEmailTemplates.BusEx);
            File.WriteAllText(Path.Combine(folderPath, "SysEx.html"), DefaultEmailTemplates.SysEx);
            File.WriteAllText(Path.Combine(folderPath, "FrameEx.html"), DefaultEmailTemplates.FrameEx);
            File.WriteAllText(Path.Combine(folderPath, "Report.html"), DefaultEmailTemplates.Report);
            Log($"Email templates dumped to {folderPath}");
        }

        public (string body, string subject) PrepareEmailTemplate(string bodyTemplate, Dictionary<string, object> data)
        {
            var subject = ExtractSubjectFromHtml(bodyTemplate);
            var emailService = new EmailHelperService(Log);

            foreach (var kv in data)
            {
                if (kv.Value is DataTable)
                {
                    bodyTemplate = bodyTemplate.Replace("{" + kv.Key + "}", emailService.ToHtmlTable((DataTable)kv.Value));

                }
                else
                {
                    subject = subject.Replace("{" + kv.Key + "}", kv.Value?.ToString() ?? string.Empty);
                    bodyTemplate = bodyTemplate.Replace("{" + kv.Key + "}", kv.Value?.ToString() ?? string.Empty);
                }
            }
            Log($"Prepared email body from template with {data.Count} replacements.");
            return (subject, bodyTemplate);
        }
        public string ExtractSubjectFromHtml(string htmlBody)
        {
            try
            {
                // Use regex to extract the content of the <title> tag
                var titleMatch = Regex.Match(htmlBody, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (titleMatch.Success)
                {
                    // Get the title content and decode any HTML entities
                    string subject = titleMatch.Groups[1].Value.Trim();

                    // Decode common HTML entities
                    subject = subject.Replace("&lt;", "<")
                                   .Replace("&gt;", ">")
                                   .Replace("&amp;", "&")
                                   .Replace("&quot;", "\"")
                                   .Replace("&#39;", "'");

                    Log($"Extracted email subject from HTML title: {subject}");
                    return subject;
                }
                else
                {
                    // Fallback subject if no title tag found
                    string fallbackSubject = "Automated Process Notification";
                    Log($"No title tag found in HTML body. Using fallback subject: {fallbackSubject}");
                    return fallbackSubject;
                }
            }
            catch (Exception ex)
            {
                Log($"Error extracting subject from HTML: {ex.Message}. Using fallback subject.");
                return "Automated Process Notification";
            }
        }
    }
}
