using System.Collections.Generic;

namespace Yash.Utility.Services.CodeGeneration.Models
{
    /// <summary>
    /// Represents a method in generated C# code
    /// </summary>
    public class CodeGenerationMethod
    {
        /// <summary>
        /// Name of the method
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Return type of the method (e.g., "void", "string", "Task<int>")
        /// </summary>
        public string ReturnType { get; set; } = "void";

        /// <summary>
        /// Access modifier (public, private, protected, internal)
        /// </summary>
        public string AccessModifier { get; set; } = "public";

        /// <summary>
        /// Whether the method is static
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Whether the method is virtual
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Whether the method is override
        /// </summary>
        public bool IsOverride { get; set; }

        /// <summary>
        /// Whether the method is abstract
        /// </summary>
        public bool IsAbstract { get; set; }

        /// <summary>
        /// Whether the method is async
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// Parameters of the method
        /// </summary>
        public List<CodeGenerationParameter> Parameters { get; set; } = new();

        /// <summary>
        /// Generic type parameters (e.g., "T", "TKey, TValue")
        /// </summary>
        public List<string> GenericParameters { get; set; } = new();

        /// <summary>
        /// Generic constraints (e.g., "where T : class")
        /// </summary>
        public List<string> GenericConstraints { get; set; } = new();

        /// <summary>
        /// XML documentation for the method
        /// </summary>
        public string? Documentation { get; set; }

        /// <summary>
        /// Attributes applied to the method
        /// </summary>
        public List<string> Attributes { get; set; } = new();

        /// <summary>
        /// Body/implementation of the method
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// Whether this is a constructor
        /// </summary>
        public bool IsConstructor { get; set; }

        /// <summary>
        /// Base constructor call (for constructors)
        /// </summary>
        public string? BaseConstructorCall { get; set; }
    }

    /// <summary>
    /// Represents a parameter in a method
    /// </summary>
    public class CodeGenerationParameter
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Default value for the parameter
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Whether the parameter is out
        /// </summary>
        public bool IsOut { get; set; }

        /// <summary>
        /// Whether the parameter is ref
        /// </summary>
        public bool IsRef { get; set; }

        /// <summary>
        /// Whether the parameter is params
        /// </summary>
        public bool IsParams { get; set; }

        /// <summary>
        /// Attributes applied to the parameter
        /// </summary>
        public List<string> Attributes { get; set; } = new();
    }
}