using System.Data;
using System.Diagnostics;
using Yash.Config.ConfigurationFile;
using Yash.Config.ConfigurationService;

namespace Yash.Config.Services
{
    /// <summary>
    /// Interface for configuration services that handle loading, parsing, and generating configuration classes
    /// from Excel files with support for Orchestrator integration and professional code generation.
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// Gets or sets the file path to the configuration Excel file.
        /// </summary>
        string FilePath { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether the current configuration file is valid.
        /// </summary>
        bool IsValid { get; }
        
        /// <summary>
        /// Gets metadata information about the configuration file.
        /// </summary>
        ConfigFileMetadata Metadata { get; }

        /// <summary>
        /// Determines the configuration type based on the provided data table structure.
        /// </summary>
        string DetermineConfigType(DataTable table, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information);
        
        /// <summary>
        /// Escapes XML special characters in the input string.
        /// </summary>
        string EscapeXml(string input);
        
        /// <summary>
        /// Generates configuration class files using the modern code generation service.
        /// </summary>
        string GenerateClassFiles(string outputDir, string ns, string usings, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information);
        
        /// <summary>
        /// Generates a configuration class string using the professional BaseCodeGenerationService.
        /// </summary>
        string GenerateClassString(string outputClassName, string outputFolder, string namespaceName, string additionalUsings = "", Action<string, TraceEventType>? log = null);
        
        /// <summary>
        /// Checks if the data table has property headers that match the specified type.
        /// </summary>
        bool HasPropertyHeaders(DataTable table, Type type, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information);
        
        /// <summary>
        /// Determines if the specified file path points to an Excel file.
        /// </summary>
        bool IsExcelFile(string _FilePath);
        
        /// <summary>
        /// Validates if the specified type is a valid configuration type.
        /// </summary>
        bool IsValidConfigType(Type type);
        
        /// <summary>
        /// Loads configuration asynchronously with optional scope filtering and Orchestrator integration.
        /// </summary>
        Task<LoadConfigResult> LoadConfigAsync(string? scope = null);
        
        /// <summary>
        /// Parses a specific configuration file item asynchronously.
        /// </summary>
        Task<object> ParseConfigFileItemAsync(string _FilePath, string type, string name, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information);
        
        /// <summary>
        /// Sanitizes names to be valid C# identifiers.
        /// </summary>
        string SanitizeName(string rawName);
        
        /// <summary>
        /// Validates the current configuration file structure and content.
        /// </summary>
        void ValidateConfigFile();
        
        /// <summary>
        /// Validates the configuration file against a specific type definition.
        /// </summary>
        List<string> ValidateConfigFileToType(Type type, ConfigFile configFile);
    }
}