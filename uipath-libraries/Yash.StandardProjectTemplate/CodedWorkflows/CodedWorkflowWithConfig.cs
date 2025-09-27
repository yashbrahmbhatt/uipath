using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UiPath.Activities.Api.Base;
using Yash.Config;
using Yash.Orchestrator;
using Yash.StandardProject.Configs;

namespace Yash.StandardProject.CodedWorkflows
{
    /// <summary>
    /// Abstract base class for coded workflows that require configuration support.
    /// Extends BaseCodedWorkflow with configuration loading functionality.
    /// </summary>
    public abstract class CodedWorkflowWithConfig : CodedWorkflow
    {
        #region Abstract Properties
        
        /// <summary>
        /// Path to the configuration file.
        /// </summary>
        public abstract string ConfigPath { get; set; }
        
        /// <summary>
        /// Configuration scopes to load (e.g., "Shared", "Dispatcher", "Performer", "Reporter").
        /// </summary>
        public abstract string[] ConfigScopes { get; set; }
        
        /// <summary>
        /// Test identifier for workflows that need to implement test-specific behaviors.
        /// Empty string indicates normal execution mode.
        /// </summary>
        public virtual string TestId { get; set; } = "";
        
        #endregion

        #region Protected Properties
        
        /// <summary>
        /// Loaded shared configuration.
        /// </summary>
        protected SharedConfig SharedConfig { get; private set; }
        
        /// <summary>
        /// Loaded performer configuration.
        /// </summary>
        protected PerformerConfig PerformerConfig { get; private set; }
        
        /// <summary>
        /// Loaded dispatcher configuration.
        /// </summary>
        protected DispatcherConfig DispatcherConfig { get; private set; }
        
        /// <summary>
        /// Loaded reporter configuration.
        /// </summary>
        protected ReporterConfig ReporterConfig { get; private set; }
        public struct AllConfigs{
            public SharedConfig? shared;
            public PerformerConfig? performer;
            public DispatcherConfig? dispatcher;
            public ReporterConfig? reporter;
        }
        #endregion

        
        #region Configuration Loading
        
        /// <summary>
        /// Loads configuration for the specified scopes from the given path.
        /// </summary>
        /// <param name="path">Path to the configuration file</param>
        /// <param name="scopes">Configuration scopes to load</param>
        /// <returns>Tuple containing loaded configurations</returns>
        public async  Task<AllConfigs>
        LoadConfig(string path,
                   string[] scopes)
        {
            var access = services.Container.Resolve<IAccessProvider>();
            var orch = new OrchestratorService(access, Log, TraceEventType.Information);
            await orch.InitializeAsync();
            var all = new AllConfigs();
            
            foreach (var scope in scopes)
            {
                switch (scope)
                {
                    case "Dispatcher":
                        all.dispatcher = await ConfigService.TryLoadStrictConfigAsync<DispatcherConfig>(path, orch, "Dispatcher", Log, TraceEventType.Verbose);
                        break;
                    case "Shared":
                        all.shared = await ConfigService.TryLoadStrictConfigAsync<SharedConfig>(path, orch, "Shared", Log, TraceEventType.Verbose);
                        break;
                    case "Performer":
                        all.performer = await ConfigService.TryLoadStrictConfigAsync<PerformerConfig>(path, orch, "Performer", Log, TraceEventType.Verbose);
                        break;
                    case "Reporter":
                        all.reporter = await ConfigService.TryLoadStrictConfigAsync<ReporterConfig>(path, orch, "Reporter", Log, TraceEventType.Verbose);
                        break;
                    default:
                        break;
                }
            }
            return all;
        }

        /// <summary>
        /// Initializes the configuration by loading the specified scopes.
        /// This method should be called before executing the workflow.
        /// </summary>
        public async virtual void InitializeConfiguration()
        {
            Log($"🔧 Loading configuration from: {ConfigPath}");
            Log($"🔍 Configuration scopes: {string.Join(", ", ConfigScopes)}");
            
            var all = await LoadConfig(ConfigPath, ConfigScopes);
            
            SharedConfig = all.shared;
            PerformerConfig = all.performer;
            DispatcherConfig = all.dispatcher;
            ReporterConfig = all.reporter;
            
            Log("✅ Configuration loaded successfully");
        }
        
        #endregion

        
        public virtual void Execute(string configPath, string[] configScopes, string testId = ""){
            ConfigPath = configPath;
            ConfigScopes = configScopes;
            TestId = testId;
        }

        #region Utility Methods
        
        /// <summary>
        /// Validates that required configuration scopes are loaded.
        /// </summary>
        /// <param name="requiredScopes">Array of required scope names</param>
        protected void ValidateRequiredConfigurations(string[] requiredScopes)
        {
            foreach (var scope in requiredScopes)
            {
                switch (scope)
                {
                    case "Shared":
                        if (SharedConfig == null)
                            throw new InvalidOperationException($"Required configuration scope '{scope}' was not loaded.");
                        break;
                    case "Performer":
                        if (PerformerConfig == null)
                            throw new InvalidOperationException($"Required configuration scope '{scope}' was not loaded.");
                        break;
                    case "Dispatcher":
                        if (DispatcherConfig == null)
                            throw new InvalidOperationException($"Required configuration scope '{scope}' was not loaded.");
                        break;
                    case "Reporter":
                        if (ReporterConfig == null)
                            throw new InvalidOperationException($"Required configuration scope '{scope}' was not loaded.");
                        break;
                }
            }
        }
        
        #endregion
    }
}
