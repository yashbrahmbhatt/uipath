using System.Collections.Generic;

namespace Yash.Utility.Services.CodeGeneration.Models
{
    /// <summary>
    /// Represents a complete C# file with all its components
    /// </summary>
    public class CodeGenerationFile
    {
        /// <summary>
        /// File header comments
        /// </summary>
        public List<string> HeaderComments { get; set; } = new();

        /// <summary>
        /// Using statements at the file level
        /// </summary>
        public List<CodeGenerationUsing> Usings { get; set; } = new();

        /// <summary>
        /// Namespace for the file
        /// </summary>
        public CodeGenerationNamespace? Namespace { get; set; }

        /// <summary>
        /// Classes defined directly in the file (without namespace)
        /// </summary>
        public List<CodeGenerationClass> Classes { get; set; } = new();

        /// <summary>
        /// Whether to use file-scoped namespace
        /// </summary>
        public bool UseFileScoped { get; set; }
    }

    /// <summary>
    /// Options for code generation
    /// </summary>
    public class CodeGenerationOptions
    {
        /// <summary>
        /// Whether to use file-scoped namespaces (C# 10+)
        /// </summary>
        public bool UseFileScopedNamespaces { get; set; }

        /// <summary>
        /// Indentation string (default is 4 spaces)
        /// </summary>
        public string Indentation { get; set; } = "    ";

        /// <summary>
        /// Whether to generate XML documentation
        /// </summary>
        public bool GenerateDocumentation { get; set; } = true;

        /// <summary>
        /// Whether to sort using statements
        /// </summary>
        public bool SortUsings { get; set; } = true;

        /// <summary>
        /// Whether to group using statements by namespace
        /// </summary>
        public bool GroupUsings { get; set; } = true;
    }
}