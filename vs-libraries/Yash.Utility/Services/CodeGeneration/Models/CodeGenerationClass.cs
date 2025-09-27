using System.Collections.Generic;

namespace Yash.Utility.Services.CodeGeneration.Models
{
    /// <summary>
    /// Represents a class in generated C# code
    /// </summary>
    public class CodeGenerationClass
    {
        /// <summary>
        /// Name of the class
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Access modifier (public, private, protected, internal)
        /// </summary>
        public string AccessModifier { get; set; } = "public";

        /// <summary>
        /// Whether the class is static
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Whether the class is abstract
        /// </summary>
        public bool IsAbstract { get; set; }

        /// <summary>
        /// Whether the class is sealed
        /// </summary>
        public bool IsSealed { get; set; }

        /// <summary>
        /// Whether the class is partial
        /// </summary>
        public bool IsPartial { get; set; }

        /// <summary>
        /// Base class that this class inherits from
        /// </summary>
        public string? BaseClass { get; set; }

        /// <summary>
        /// Interfaces that this class implements
        /// </summary>
        public List<string> Interfaces { get; set; } = new();

        /// <summary>
        /// Generic type parameters (e.g., "T", "TKey, TValue")
        /// </summary>
        public List<string> GenericParameters { get; set; } = new();

        /// <summary>
        /// Generic constraints (e.g., "where T : class")
        /// </summary>
        public List<string> GenericConstraints { get; set; } = new();

        /// <summary>
        /// XML documentation for the class
        /// </summary>
        public string? Documentation { get; set; }

        /// <summary>
        /// Attributes applied to the class
        /// </summary>
        public List<string> Attributes { get; set; } = new();

        /// <summary>
        /// Properties of the class
        /// </summary>
        public List<CodeGenerationProperty> Properties { get; set; } = new();

        /// <summary>
        /// Methods of the class
        /// </summary>
        public List<CodeGenerationMethod> Methods { get; set; } = new();

        /// <summary>
        /// Fields of the class
        /// </summary>
        public List<CodeGenerationField> Fields { get; set; } = new();

        /// <summary>
        /// Nested classes
        /// </summary>
        public List<CodeGenerationClass> NestedClasses { get; set; } = new();

        /// <summary>
        /// Type of class (class, interface, struct, enum, record)
        /// </summary>
        public ClassType Type { get; set; } = ClassType.Class;
    }

    /// <summary>
    /// Represents a field in generated C# code
    /// </summary>
    public class CodeGenerationField
    {
        /// <summary>
        /// Name of the field
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of the field
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Access modifier (public, private, protected, internal)
        /// </summary>
        public string AccessModifier { get; set; } = "private";

        /// <summary>
        /// Whether the field is static
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Whether the field is readonly
        /// </summary>
        public bool IsReadonly { get; set; }

        /// <summary>
        /// Whether the field is const
        /// </summary>
        public bool IsConst { get; set; }

        /// <summary>
        /// Default value for the field
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// XML documentation for the field
        /// </summary>
        public string? Documentation { get; set; }

        /// <summary>
        /// Attributes applied to the field
        /// </summary>
        public List<string> Attributes { get; set; } = new();
    }

    /// <summary>
    /// Types of classes that can be generated
    /// </summary>
    public enum ClassType
    {
        Class,
        Interface,
        Struct,
        Enum,
        Record
    }
}