using System;
using System.Collections.Generic;

namespace Yash.Utility.Services.CodeGeneration.Models
{
    /// <summary>
    /// Represents a property in generated C# code
    /// </summary>
    public class CodeGenerationProperty
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of the property (e.g., "string", "int", "List<MyClass>")
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Access modifier (public, private, protected, internal)
        /// </summary>
        public string AccessModifier { get; set; } = "public";

        /// <summary>
        /// Whether the property is static
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Whether the property is virtual
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Whether the property is override
        /// </summary>
        public bool IsOverride { get; set; }

        /// <summary>
        /// Whether the property is abstract
        /// </summary>
        public bool IsAbstract { get; set; }

        /// <summary>
        /// Whether the property has a getter
        /// </summary>
        public bool HasGetter { get; set; } = true;

        /// <summary>
        /// Whether the property has a setter
        /// </summary>
        public bool HasSetter { get; set; } = true;

        /// <summary>
        /// Default value for the property
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// XML documentation for the property
        /// </summary>
        public string? Documentation { get; set; }

        /// <summary>
        /// Attributes applied to the property
        /// </summary>
        public List<string> Attributes { get; set; } = new();

        /// <summary>
        /// Whether the property is readonly
        /// </summary>
        public bool IsReadonly { get; set; }

        /// <summary>
        /// Custom getter implementation
        /// </summary>
        public string? CustomGetter { get; set; }

        /// <summary>
        /// Custom setter implementation
        /// </summary>
        public string? CustomSetter { get; set; }
    }
}