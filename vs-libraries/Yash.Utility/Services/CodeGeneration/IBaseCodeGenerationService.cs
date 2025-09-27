using System.Collections.Generic;
using Yash.Utility.Services.CodeGeneration.Models;

namespace Yash.Utility.Services.CodeGeneration
{
    /// <summary>
    /// Base interface for generating C# code from code generation models
    /// </summary>
    public interface IBaseCodeGenerationService
    {
        /// <summary>
        /// Generates a complete C# file from a CodeGenerationFile model
        /// </summary>
        /// <param name="file">The file model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        string GenerateFile(CodeGenerationFile file, CodeGenerationOptions? options = null);

        /// <summary>
        /// Generates a namespace declaration with its contents
        /// </summary>
        /// <param name="namespaceModel">The namespace model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        string GenerateNamespace(CodeGenerationNamespace namespaceModel, CodeGenerationOptions? options = null);

        /// <summary>
        /// Generates a class declaration with its contents
        /// </summary>
        /// <param name="classModel">The class model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        string GenerateClass(CodeGenerationClass classModel, CodeGenerationOptions? options = null);

        /// <summary>
        /// Generates a property declaration
        /// </summary>
        /// <param name="property">The property model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        string GenerateProperty(CodeGenerationProperty property, CodeGenerationOptions? options = null);

        /// <summary>
        /// Generates a method declaration
        /// </summary>
        /// <param name="method">The method model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        string GenerateMethod(CodeGenerationMethod method, CodeGenerationOptions? options = null);

        /// <summary>
        /// Generates a field declaration
        /// </summary>
        /// <param name="field">The field model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        string GenerateField(CodeGenerationField field, CodeGenerationOptions? options = null);

        /// <summary>
        /// Generates using statements
        /// </summary>
        /// <param name="usings">The using statements to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        string GenerateUsings(List<CodeGenerationUsing> usings, CodeGenerationOptions? options = null);

        /// <summary>
        /// Generates XML documentation from a documentation string
        /// </summary>
        /// <param name="documentation">The documentation text</param>
        /// <param name="indentLevel">Indentation level</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated XML documentation as string</returns>
        string GenerateDocumentation(string documentation, int indentLevel = 0, CodeGenerationOptions? options = null);

        /// <summary>
        /// Generates attributes
        /// </summary>
        /// <param name="attributes">The attributes to generate</param>
        /// <param name="indentLevel">Indentation level</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        string GenerateAttributes(List<string> attributes, int indentLevel = 0, CodeGenerationOptions? options = null);
    }
}