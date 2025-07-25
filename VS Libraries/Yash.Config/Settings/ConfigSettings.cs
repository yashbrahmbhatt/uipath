using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Settings;

namespace Yash.Config.Settings
{
    public static class Keys
    {
        public const string CategoryKey = "Yash.Config";
        public const string Section_Generation_Key = $"{CategoryKey}.AutoGeneration";

        public const string Setting_Generation_FilePath_Key = $"{Section_Generation_Key}.FilePath";
        public const string Setting_Generation_OutputDir_Key = $"{Section_Generation_Key}.OutputDirectory";
        public const string Setting_Generation_ClassName_Key = $"{Section_Generation_Key}.ClassName";
        public const string Setting_Generation_Namespace_Key = $"{Section_Generation_Key}.Namespace";
        public const string Setting_Generation_Usings_Key = $"{Section_Generation_Key}.AdditionalUsings";
    }

    public static class ConfigSettings
    {
        public static void CreateSettings(IWorkflowDesignApi workflowDesignApi)
        {
            var category = CreateConfigCategory(workflowDesignApi);
            var section = CreateAutogenSection(workflowDesignApi, category);

            CreateAutogenFilePathSetting(workflowDesignApi, section);
            CreateAutogenOutputDirSetting(workflowDesignApi, section);
            CreateAutogenClassNameSetting(workflowDesignApi, section);
            CreateAutogenNamespaceSetting(workflowDesignApi, section);
            CreateAutogenUsingsSetting(workflowDesignApi, section);
        }

        public static SettingsCategory CreateConfigCategory(IWorkflowDesignApi? api = null)
        {
            var category = new SettingsCategory()
            {
                Key = Keys.CategoryKey,
                Header = "Config",
                Description = "Settings for Config activities and wizards"
            };
            api?.Settings.AddCategory(category);
            return category;
        }

        public static SettingsSection CreateAutogenSection(IWorkflowDesignApi? api = null, SettingsCategory? category = null)
        {
            var section = new SettingsSection()
            {
                Key = Keys.Section_Generation_Key,
                Title = "Config Class Generation",
                Description = "Settings related to generation of config classes"
            };
            if (category != null) api?.Settings.AddSection(category, section);
            return section;
        }

        public static SingleValueEditorDescription<string> CreateAutogenFilePathSetting(IWorkflowDesignApi? api = null, SettingsSection? section = null)
        {
            var setting = new SingleValueEditorDescription<string>
            {
                Key = Keys.Setting_Generation_FilePath_Key,
                Label = "Config File Path",
                Description = "The Excel file path used to generate config classes.",
                DefaultValue = "",
                GetDisplayValue = v => string.IsNullOrWhiteSpace(v) ? "Not Set" : v,
                IsDesignTime = true,
                Validate = v => string.IsNullOrWhiteSpace(v)
                    ? "Config file path is required."
                    : !System.IO.File.Exists(v)
                        ? "File does not exist."
                        : null
            };
            if (section != null) api?.Settings.AddSetting(section, setting);
            return setting;
        }

        public static SingleValueEditorDescription<string> CreateAutogenOutputDirSetting(IWorkflowDesignApi? api = null, SettingsSection? section = null)
        {
            var setting = new SingleValueEditorDescription<string>
            {
                Key = Keys.Setting_Generation_OutputDir_Key,
                Label = "Output Directory",
                Description = "The folder where generated config class files will be saved.",
                DefaultValue = "",
                GetDisplayValue = v => string.IsNullOrWhiteSpace(v) ? "Not Set" : v,
                IsDesignTime = true,
                Validate = v => string.IsNullOrWhiteSpace(v)
                    ? "Output directory is required."
                    : !System.IO.Directory.Exists(v)
                        ? "Directory does not exist."
                        : null
            };
            if (section != null) api?.Settings.AddSetting(section, setting);
            return setting;
        }

        public static SingleValueEditorDescription<string> CreateAutogenClassNameSetting(IWorkflowDesignApi? api = null, SettingsSection? section = null)
        {
            var setting = new SingleValueEditorDescription<string>
            {
                Key = Keys.Setting_Generation_ClassName_Key,
                Label = "Class Name",
                Description = "The name of the generated class.",
                DefaultValue = "",
                GetDisplayValue = v => string.IsNullOrWhiteSpace(v) ? "Not Set" : v,
                IsDesignTime = true,
                Validate = v => string.IsNullOrWhiteSpace(v)
                    ? "Class name is required."
                    : !char.IsLetter(v[0])
                        ? "Class name must start with a letter."
                        : null
            };
            if (section != null) api?.Settings.AddSetting(section, setting);
            return setting;
        }

        public static SingleValueEditorDescription<string> CreateAutogenNamespaceSetting(IWorkflowDesignApi? api = null, SettingsSection? section = null)
        {
            var setting = new SingleValueEditorDescription<string>
            {
                Key = Keys.Setting_Generation_Namespace_Key,
                Label = "Namespace",
                Description = "Namespace for the generated class.",
                DefaultValue = "",
                GetDisplayValue = v => string.IsNullOrWhiteSpace(v) ? "Not Set" : v,
                IsDesignTime = true,
                Validate = v => string.IsNullOrWhiteSpace(v) ? "Namespace is required." : null
            };
            if (section != null) api?.Settings.AddSetting(section, setting);
            return setting;
        }

        public static SingleValueEditorDescription<string> CreateAutogenUsingsSetting(IWorkflowDesignApi? api = null, SettingsSection? section = null)
        {
            var setting = new SingleValueEditorDescription<string>
            {
                Key = Keys.Setting_Generation_Usings_Key,
                Label = "Additional Usings",
                Description = "Comma-separated list of additional using directives.",
                DefaultValue = "",
                GetDisplayValue = v => string.IsNullOrWhiteSpace(v) ? "None" : v,
                IsDesignTime = true
            };
            if (section != null) api?.Settings.AddSetting(section, setting);
            return setting;
        }
    }
}
