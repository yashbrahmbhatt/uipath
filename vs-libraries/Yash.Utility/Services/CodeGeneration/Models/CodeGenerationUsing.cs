namespace Yash.Utility.Services.CodeGeneration.Models
{
    /// <summary>
    /// Represents a using statement in generated C# code
    /// </summary>
    public class CodeGenerationUsing
    {
        /// <summary>
        /// Namespace or type being used
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// Alias for the using statement (e.g., "using MyAlias = System.Collections.Generic")
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Whether this is a static using
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Whether this is a global using (C# 10+)
        /// </summary>
        public bool IsGlobal { get; set; }

        /// <summary>
        /// Creates a simple using statement
        /// </summary>
        /// <param name="namespace">Namespace to use</param>
        /// <returns>CodeGenerationUsing instance</returns>
        public static CodeGenerationUsing Create(string @namespace)
        {
            return new CodeGenerationUsing { Namespace = @namespace };
        }

        /// <summary>
        /// Creates a using statement with alias
        /// </summary>
        /// <param name="alias">Alias name</param>
        /// <param name="namespace">Namespace to use</param>
        /// <returns>CodeGenerationUsing instance</returns>
        public static CodeGenerationUsing CreateWithAlias(string alias, string @namespace)
        {
            return new CodeGenerationUsing { Alias = alias, Namespace = @namespace };
        }

        /// <summary>
        /// Creates a static using statement
        /// </summary>
        /// <param name="namespace">Namespace to use statically</param>
        /// <returns>CodeGenerationUsing instance</returns>
        public static CodeGenerationUsing CreateStatic(string @namespace)
        {
            return new CodeGenerationUsing { Namespace = @namespace, IsStatic = true };
        }
    }
}