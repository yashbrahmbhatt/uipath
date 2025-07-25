using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace CLIToGitHubWrapper
{
    public class RunShellCommand : CodedWorkflow
    {
        [Workflow]
        public string Execute(string command, string workingDirectory = "")
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var shell = isWindows ? "cmd.exe" : "/bin/bash";
            var shellArgs = isWindows ? $"/c \"{command}\"" : $"-c \"{command}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = shellArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            startInfo.Environment["PATH"] += ";C:\\tools\\uipcli\\tools";
            Log($"Shell: {shellArgs}");
            if (!string.IsNullOrWhiteSpace(workingDirectory))
                startInfo.WorkingDirectory = workingDirectory;

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            Log($"Result: {output.Trim()}");
            return output.Trim();
        }
    }
}