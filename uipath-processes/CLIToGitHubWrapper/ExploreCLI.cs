using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UiPath.CodedWorkflows;

namespace CLIToGitHubWrapper
{
    public class ExploreCLI : CodedWorkflow
    {
        [Workflow]
        public async Task Execute(string baseCommand = "uipath", string outputFolder = @"cli-docs")
        {
            ResetOutputFolder(outputFolder);
            Log($"Starting CLI exploration from base command: {baseCommand}");
            var visited = new HashSet<string>();
            await ExploreRecursive(baseCommand, visited, outputFolder);
            Log("CLI exploration completed.");
        }

        private void ResetOutputFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
                Log($"Deleted existing folder: {folderPath}");
            }

            Directory.CreateDirectory(folderPath);
            Log($"Created fresh folder: {folderPath}");
        }

        private async Task ExploreRecursive(string command, HashSet<string> visited, string outputFolder)
        {
            if (visited.Contains(command))
            {
                Log($"Already visited: {command}, skipping.");
                return;
            }

            Log($"Exploring command: {command}");
            visited.Add(command);

            string output;
            try
            {
                output = await RunShellCommandAsync($"{command} --help", "");
                Log($"Successfully ran '{command} --help'");
            }
            catch (Exception ex)
            {
                Log($"Failed to run '{command} --help': {ex.Message}");
                return;
            }

            var subcommands = ParseSubcommands(output);
            Log($"Found {subcommands.Count} subcommand(s) for: {command}");

            
            // Always save help output (non-leaf and leaf)
            await SaveCommandHelpAsync(command, output, outputFolder, subcommands.Count == 0);
            
            foreach (var sub in subcommands)
            {
                await ExploreRecursive($"{command} {sub}", visited, outputFolder);
            }
        }

        private static List<string> ParseSubcommands(string helpText)
        {
            var commands = new List<string>();
            var lines = helpText.Split('\n');
            var inCommandsSection = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd();
                var trimmed = line.Trim();

                // Detect start of commands section
                if (!inCommandsSection)
                {
                    if (Regex.IsMatch(trimmed, @"^(COMMANDS|AVAILABLE COMMANDS|SUBCOMMANDS)[:\s]*$", RegexOptions.IgnoreCase))
                    {
                        inCommandsSection = true;
                    }
                    continue;
                }

                // Exit on blank line or new section
                if (string.IsNullOrWhiteSpace(trimmed) || Regex.IsMatch(trimmed, @"^[A-Z][A-Z\s-]+:\s*$"))
                {
                    break;
                }

                // Only consider lines that start with a potential subcommand
                var commandMatch = Regex.Match(trimmed, @"^([a-zA-Z0-9_-]+)\s{2,}");
                if (commandMatch.Success)
                {
                    commands.Add(commandMatch.Groups[1].Value);
                }
            }

            return commands;
        }

        private static async Task SaveCommandHelpAsync(string fullCommand, string helpText, string outputFolder, bool isCommand)
        {
            var parts = fullCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var fileName = (isCommand ? "command_" : "help_") + fullCommand.Replace(" --help", "") + ".txt";
            var folderPath = Path.Combine(outputFolder, Path.Combine(parts[..^1]));
            Directory.CreateDirectory(folderPath);
            var fullPath = Path.Combine(folderPath, fileName);

            await File.WriteAllTextAsync(fullPath, helpText);
            Console.WriteLine($"Saved: {fullPath}");
        }

        // Stub — replace this with the actual async shell runner you have
        private async Task<string> RunShellCommandAsync(string command, string workingDir)
        {
            return await Task.Run(() => workflows.RunShellCommand(command, workingDir));
        }
    }
}
