using System.Collections.Generic;

namespace Yash.Utility.Services.CodeGeneration.Models
{
    /// <summary>
    /// Represents a namespace in generated C# code
    /// </summary>
    public class CodeGenerationNamespace
    {
        /// <summary>
        /// Name of the namespace
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Using statements for the namespace
        /// </summary>
        public List<CodeGenerationUsing> Usings { get; set; } = new();

        /// <summary>
        /// Classes defined in the namespace
        /// </summary>
        public List<CodeGenerationClass> Classes { get; set; } = new();

        /// <summary>
        /// XML documentation for the namespace
        /// </summary>
        public string? Documentation { get; set; }

        /// <summary>
        /// Whether to use file-scoped namespace (C# 10+)
        /// </summary>
        public bool UseFileScoped { get; set; }
    }
}