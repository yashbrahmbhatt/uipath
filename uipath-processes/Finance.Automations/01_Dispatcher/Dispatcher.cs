using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Finance.Automations.Configs;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities;
using Yash.Config.Models;
using Yash.RBC.Activities;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Finance.Automations._01_Dispatcher
{
    public class Dispatcher : CodedWorkflow
    {
        public bool debug_SkipDownload = false;
        public string debug_SkipDownloadPath = @"C:\Users\eyash\Downloads\csv13541.csv";
        /// <summary>
        /// Executes the dispatcher workflow, processing queue observations based on configuration and dispatching them to the queue.
        /// </summary>
        /// <param name="ConfigPath">The path to the configuration file.</param>
        /// <remarks>
        /// This method performs the following steps:
        /// </remarks>
        [Workflow]
        public void Execute(
            string ConfigPath = "Data\\Config.xlsx",
            string TestId = ""
            )
        {
            Dictionary<string, object> dict_Shared = workflows.LoadConfig(ConfigPath, "Shared");
            Dictionary<string, object> dict_Dispatcher = workflows.LoadConfig(ConfigPath, "Dispatcher");
            var config_Shared = ConfigFactory.FromDictionary<SharedConfig>(dict_Shared);
            var config_Dispatcher = ConfigFactory.FromDictionary<DispatcherConfig>(dict_Dispatcher);

            try
            {
                Log("Starting dispatching", LogLevel.Info);
                if (TestId == "Dispatcher.Failure") throw new Exception(TestId);
                var added = 0;
                UiElement window_RBC = null;
                try
                {
                    yashRBCActivities.Terminate(window_RBC);
                }
                catch (Exception)
                {
                    foreach (var p in config_Dispatcher.ProcessesToKill)
                        workflows.Kill_Process(p);
                }
                string tempPath;
                if (!debug_SkipDownload)
                {

                    window_RBC = yashRBCActivities.Initialize(config_Shared.CredentialName_RBC, config_Shared.CredentialFolder_RBC);
                    tempPath = Path.GetTempFileName().Replace(".", "") + ".csv";
                    window_RBC = yashRBCActivities.DownloadTransactions(tempPath, TransactionScopes.All, AccountTypes.All, window_RBC);
                }
                else
                {
                    tempPath = debug_SkipDownloadPath;
                }
                var table = workflows.ReadCSVProxy(tempPath);
                table.Columns.Add(new DataColumn("Reference"));
                foreach(DataRow row in table.Rows){
                     // Create hash from actual field values, timestamp, and row index
                    string rowValues = string.Join("|", row.ItemArray.Select(field => field?.ToString() ?? ""));
                    var index = table.Rows.IndexOf(row);
                    string uniqueString = $"{rowValues}";
                    string reference = Hash(uniqueString);
                    row["Reference"] = reference;
                }
                
                system.BulkAddQueueItems(table, config_Shared.QueueName, config_Shared.QueueFolder, BulkAddQueueItems.CommitTypeEnum.ProcessAllIndependently,default);

                if (!debug_SkipDownload)
                    File.Delete(tempPath);
                
                Log($"Found {table.Rows.Count} rows");
            }
            catch (Exception ex)
            {
                var dict_Diagnostic = workflows.GenerateDiagnosticDictionary(ex, default);
                dict_Diagnostic["ProcessName"] = config_Dispatcher.ProcessName;
                workflows.SendEmail(config_Shared.SysEx_To, config_Shared.Email_FrameEx, default, dict_Diagnostic, default);
                throw;
            }

        }

        private string Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // SHA256 with 64-character hex output (32 bytes) - much more unique than CRC32
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Convert all 32 bytes to hex (64 characters) - well under 100 char limit
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }
    }
}