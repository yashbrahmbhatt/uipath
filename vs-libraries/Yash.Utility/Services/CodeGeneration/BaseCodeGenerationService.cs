using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yash.Utility.Services.Base;
using Yash.Utility.Services.CodeGeneration.Models;

namespace Yash.Utility.Services.CodeGeneration
{
    /// <summary>
    /// Base abstract class for generating C# code from code generation models
    /// </summary>
    public abstract class BaseCodeGenerationService : BaseService, IBaseCodeGenerationService
    {
        /// <summary>
        /// Default code generation options
        /// </summary>
        protected virtual CodeGenerationOptions DefaultOptions => new CodeGenerationOptions();

        /// <summary>
        /// Generates a complete C# file from a CodeGenerationFile model
        /// </summary>
        /// <param name="file">The file model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        public virtual string GenerateFile(CodeGenerationFile file, CodeGenerationOptions? options = null)
        {
            options ??= DefaultOptions;
            var sb = new StringBuilder();

            // Add header comments
            foreach (var comment in file.HeaderComments)
            {
                sb.AppendLine($"// {comment}");
            }

            if (file.HeaderComments.Any())
            {
                sb.AppendLine();
            }

            // Add using statements
            if (file.Usings.Any())
            {
                sb.Append(GenerateUsings(file.Usings, options));
                sb.AppendLine();
            }

            // Add namespace or classes directly
            if (file.Namespace != null)
            {
                sb.Append(GenerateNamespace(file.Namespace, options));
            }
            else
            {
                // Generate classes without namespace
                foreach (var classModel in file.Classes)
                {
                    sb.Append(GenerateClass(classModel, options));
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a namespace declaration with its contents
        /// </summary>
        /// <param name="namespaceModel">The namespace model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        public virtual string GenerateNamespace(CodeGenerationNamespace namespaceModel, CodeGenerationOptions? options = null)
        {
            options ??= DefaultOptions;
            var sb = new StringBuilder();

            // Add namespace documentation
            if (!string.IsNullOrEmpty(namespaceModel.Documentation) && options.GenerateDocumentation)
            {
                sb.Append(GenerateDocumentation(namespaceModel.Documentation, 0, options));
            }

            if (namespaceModel.UseFileScoped || options.UseFileScopedNamespaces)
            {
                // File-scoped namespace (C# 10+)
                sb.AppendLine($"namespace {namespaceModel.Name};");
                sb.AppendLine();

                // Add namespace-level usings
                if (namespaceModel.Usings.Any())
                {
                    sb.Append(GenerateUsings(namespaceModel.Usings, options));
                    sb.AppendLine();
                }

                // Add classes
                foreach (var classModel in namespaceModel.Classes)
                {
                    sb.Append(GenerateClass(classModel, options));
                    if (classModel != namespaceModel.Classes.Last())
                    {
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                // Traditional namespace with braces
                sb.AppendLine($"namespace {namespaceModel.Name}");
                sb.AppendLine("{");

                // Add namespace-level usings (indented)
                if (namespaceModel.Usings.Any())
                {
                    foreach (var usingStatement in namespaceModel.Usings)
                    {
                        sb.AppendLine($"{options.Indentation}{GenerateUsing(usingStatement)}");
                    }
                    sb.AppendLine();
                }

                // Add classes (indented)
                for (int i = 0; i < namespaceModel.Classes.Count; i++)
                {
                    var classCode = GenerateClass(namespaceModel.Classes[i], options);
                    var indentedClassCode = IndentCode(classCode, 1, options);
                    sb.Append(indentedClassCode);

                    if (i < namespaceModel.Classes.Count - 1)
                    {
                        sb.AppendLine();
                    }
                }

                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a class declaration with its contents
        /// </summary>
        /// <param name="classModel">The class model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        public virtual string GenerateClass(CodeGenerationClass classModel, CodeGenerationOptions? options = null)
        {
            options ??= DefaultOptions;
            var sb = new StringBuilder();

            // Add class documentation
            if (!string.IsNullOrEmpty(classModel.Documentation) && options.GenerateDocumentation)
            {
                sb.Append(GenerateDocumentation(classModel.Documentation));
            }

            // Add attributes
            if (classModel.Attributes.Any())
            {
                sb.Append(GenerateAttributes(classModel.Attributes, 0, options));
            }

            // Build class declaration
            var declaration = new StringBuilder();
            declaration.Append(classModel.AccessModifier);

            if (classModel.IsStatic) declaration.Append(" static");
            if (classModel.IsAbstract) declaration.Append(" abstract");
            if (classModel.IsSealed) declaration.Append(" sealed");
            if (classModel.IsPartial) declaration.Append(" partial");

            declaration.Append($" {classModel.Type.ToString().ToLower()} {classModel.Name}");

            // Add generic parameters
            if (classModel.GenericParameters.Any())
            {
                declaration.Append($"<{string.Join(", ", classModel.GenericParameters)}>");
            }

            // Add inheritance
            var inheritance = new List<string>();
            if (!string.IsNullOrEmpty(classModel.BaseClass))
            {
                inheritance.Add(classModel.BaseClass);
            }
            inheritance.AddRange(classModel.Interfaces);

            if (inheritance.Any())
            {
                declaration.Append($" : {string.Join(", ", inheritance)}");
            }

            sb.AppendLine(declaration.ToString());

            // Add generic constraints
            foreach (var constraint in classModel.GenericConstraints)
            {
                sb.AppendLine($"{options.Indentation}{constraint}");
            }

            sb.AppendLine("{");

            // Add fields
            if (classModel.Fields.Any())
            {
                foreach (var field in classModel.Fields)
                {
                    var fieldCode = GenerateField(field, options);
                    sb.Append(IndentCode(fieldCode, 1, options));
                }

                // Add a blank line after fields if we have properties or methods
                if (classModel.Properties.Any() || classModel.Methods.Any())
                {
                    sb.AppendLine();
                }
            }

            // Add properties
            if (classModel.Properties.Any())
            {
                foreach (var property in classModel.Properties)
                {
                    var propertyCode = GenerateProperty(property, options);
                    sb.Append(IndentCode(propertyCode, 1, options));
                }

                // Add a blank line after properties if we have methods
                if (classModel.Methods.Any())
                {
                    sb.AppendLine();
                }
            }

            // Add methods
            if (classModel.Methods.Any())
            {
                for (int i = 0; i < classModel.Methods.Count; i++)
                {
                    var methodCode = GenerateMethod(classModel.Methods[i], options);
                    sb.Append(IndentCode(methodCode, 1, options));

                    // Add a single blank line between methods, but not after the last method
                    if (i < classModel.Methods.Count - 1)
                    {
                        sb.AppendLine();
                    }
                }

                // Add a blank line after all methods if there are nested classes
                if (classModel.NestedClasses.Any())
                {
                    sb.AppendLine();
                }
            }

            // Add nested classes
            if (classModel.NestedClasses.Any())
            {
                if (classModel.Methods.Any() || classModel.Properties.Any() || classModel.Fields.Any())
                {
                    sb.AppendLine();
                }

                for (int i = 0; i < classModel.NestedClasses.Count; i++)
                {
                    var nestedClassCode = GenerateClass(classModel.NestedClasses[i], options);
                    sb.Append(IndentCode(nestedClassCode, 1, options));

                    if (i < classModel.NestedClasses.Count - 1)
                    {
                        sb.AppendLine();
                    }
                }
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Generates a property declaration
        /// </summary>
        /// <param name="property">The property model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        public virtual string GenerateProperty(CodeGenerationProperty property, CodeGenerationOptions? options = null)
        {
            options ??= DefaultOptions;
            var sb = new StringBuilder();

            // Add property documentation
            if (!string.IsNullOrEmpty(property.Documentation) && options.GenerateDocumentation)
            {
                sb.Append(GenerateDocumentation(property.Documentation));
            }

            // Add attributes
            if (property.Attributes.Any())
            {
                sb.Append(GenerateAttributes(property.Attributes, 0, options));
            }

            // Build property declaration
            var declaration = new StringBuilder();
            declaration.Append(property.AccessModifier);

            if (property.IsStatic) declaration.Append(" static");
            if (property.IsVirtual) declaration.Append(" virtual");
            if (property.IsOverride) declaration.Append(" override");
            if (property.IsAbstract) declaration.Append(" abstract");
            if (property.IsReadonly) declaration.Append(" readonly");

            declaration.Append($" {property.Type} {property.Name} ");

            // Add accessors
            if (property.IsAbstract)
            {
                var accessors = new List<string>();
                if (property.HasGetter) accessors.Add("get;");
                if (property.HasSetter) accessors.Add("set;");
                declaration.Append($"{{ {string.Join(" ", accessors)} }}");
            }
            else if (!string.IsNullOrEmpty(property.CustomGetter) || !string.IsNullOrEmpty(property.CustomSetter))
            {
                // Custom getter/setter implementation
                declaration.AppendLine();
                declaration.Append("{");

                if (property.HasGetter)
                {
                    if (!string.IsNullOrEmpty(property.CustomGetter))
                    {
                        declaration.AppendLine();
                        declaration.Append($"{options.Indentation}get");
                        declaration.AppendLine();
                        declaration.Append($"{options.Indentation}{{");
                        declaration.AppendLine();
                        declaration.Append(IndentCode(property.CustomGetter, 2, options));
                        declaration.AppendLine();
                        declaration.Append($"{options.Indentation}}}");
                    }
                    else
                    {
                        declaration.Append(" get;");
                    }
                }

                if (property.HasSetter)
                {
                    if (!string.IsNullOrEmpty(property.CustomSetter))
                    {
                        declaration.AppendLine();
                        declaration.Append($"{options.Indentation}set");
                        declaration.AppendLine();
                        declaration.Append($"{options.Indentation}{{");
                        declaration.AppendLine();
                        declaration.Append(IndentCode(property.CustomSetter, 2, options));
                        declaration.AppendLine();
                        declaration.Append($"{options.Indentation}}}");
                    }
                    else
                    {
                        declaration.Append(" set;");
                    }
                }

                declaration.AppendLine();
                declaration.Append("}");
            }
            else
            {
                // Auto-properties
                var accessors = new List<string>();
                if (property.HasGetter) accessors.Add("get;");
                if (property.HasSetter) accessors.Add("set;");
                declaration.Append($"{{ {string.Join(" ", accessors)} }}");
            }

            // Add default value
            if (!string.IsNullOrEmpty(property.DefaultValue))
            {
                declaration.Append($" = {property.DefaultValue};");
            }

            sb.Append(declaration.ToString());
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Generates a method declaration
        /// </summary>
        /// <param name="method">The method model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        public virtual string GenerateMethod(CodeGenerationMethod method, CodeGenerationOptions? options = null)
        {
            options ??= DefaultOptions;
            var sb = new StringBuilder();

            // Add method documentation
            if (!string.IsNullOrEmpty(method.Documentation) && options.GenerateDocumentation)
            {
                sb.Append(GenerateDocumentation(method.Documentation));
            }

            // Add attributes
            if (method.Attributes.Any())
            {
                sb.Append(GenerateAttributes(method.Attributes, 0, options));
            }

            // Build method declaration
            var declaration = new StringBuilder();
            declaration.Append(method.AccessModifier);

            if (method.IsStatic) declaration.Append(" static");
            if (method.IsVirtual) declaration.Append(" virtual");
            if (method.IsOverride) declaration.Append(" override");
            if (method.IsAbstract) declaration.Append(" abstract");
            if (method.IsAsync) declaration.Append(" async");

            if (method.IsConstructor)
            {
                declaration.Append($" {method.Name}");
            }
            else
            {
                declaration.Append($" {method.ReturnType} {method.Name}");
            }

            // Add generic parameters
            if (method.GenericParameters.Any())
            {
                declaration.Append($"<{string.Join(", ", method.GenericParameters)}>");
            }

            // Add parameters
            declaration.Append("(");
            var parameterStrings = new List<string>();
            foreach (var param in method.Parameters)
            {
                var paramStr = new StringBuilder();

                if (param.Attributes.Any())
                {
                    paramStr.Append($"[{string.Join(", ", param.Attributes)}] ");
                }

                if (param.IsOut) paramStr.Append("out ");
                if (param.IsRef) paramStr.Append("ref ");
                if (param.IsParams) paramStr.Append("params ");

                paramStr.Append($"{param.Type} {param.Name}");

                if (!string.IsNullOrEmpty(param.DefaultValue))
                {
                    paramStr.Append($" = {param.DefaultValue}");
                }

                parameterStrings.Add(paramStr.ToString());
            }
            declaration.Append(string.Join(", ", parameterStrings));
            declaration.Append(")");

            // Add base constructor call for constructors
            if (method.IsConstructor && !string.IsNullOrEmpty(method.BaseConstructorCall))
            {
                declaration.Append($" : {method.BaseConstructorCall}");
            }

            sb.Append(declaration.ToString());

            // Add generic constraints
            foreach (var constraint in method.GenericConstraints)
            {
                sb.AppendLine();
                sb.Append($"{options.Indentation}{constraint}");
            }

            if (method.IsAbstract)
            {
                sb.AppendLine(";");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("{");

                if (!string.IsNullOrEmpty(method.Body))
                {
                    sb.Append(IndentCode(method.Body, 1, options));
                    sb.AppendLine();
                }

                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a field declaration
        /// </summary>
        /// <param name="field">The field model to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        public virtual string GenerateField(CodeGenerationField field, CodeGenerationOptions? options = null)
        {
            options ??= DefaultOptions;
            var sb = new StringBuilder();

            // Add field documentation
            if (!string.IsNullOrEmpty(field.Documentation) && options.GenerateDocumentation)
            {
                sb.Append(GenerateDocumentation(field.Documentation));
            }

            // Add attributes
            if (field.Attributes.Any())
            {
                sb.Append(GenerateAttributes(field.Attributes, 0, options));
            }

            // Build field declaration
            var declaration = new StringBuilder();
            declaration.Append(field.AccessModifier);

            if (field.IsStatic) declaration.Append(" static");
            if (field.IsReadonly) declaration.Append(" readonly");
            if (field.IsConst) declaration.Append(" const");

            declaration.Append($" {field.Type} {field.Name}");

            // Add default value
            if (!string.IsNullOrEmpty(field.DefaultValue))
            {
                declaration.Append($" = {field.DefaultValue}");
            }

            declaration.Append(";");

            sb.AppendLine(declaration.ToString());

            return sb.ToString();
        }

        /// <summary>
        /// Generates using statements
        /// </summary>
        /// <param name="usings">The using statements to generate</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        public virtual string GenerateUsings(List<CodeGenerationUsing> usings, CodeGenerationOptions? options = null)
        {
            options ??= DefaultOptions;
            var sb = new StringBuilder();

            var usingsList = usings.ToList();

            if (options.SortUsings)
            {
                usingsList = usingsList.OrderBy(u => u.IsGlobal ? 0 : 1)
                                     .ThenBy(u => u.IsStatic ? 1 : 0)
                                     .ThenBy(u => u.Namespace)
                                     .ToList();
            }

            if (options.GroupUsings)
            {
                var groups = usingsList.GroupBy(u => new { u.IsGlobal, u.IsStatic })
                                      .OrderBy(g => g.Key.IsGlobal ? 0 : 1)
                                      .ThenBy(g => g.Key.IsStatic ? 1 : 0);

                bool firstGroup = true;
                foreach (var group in groups)
                {
                    if (!firstGroup)
                    {
                        sb.AppendLine();
                    }

                    foreach (var usingStatement in group)
                    {
                        sb.AppendLine(GenerateUsing(usingStatement));
                    }

                    firstGroup = false;
                }
            }
            else
            {
                foreach (var usingStatement in usingsList)
                {
                    sb.AppendLine(GenerateUsing(usingStatement));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a single using statement
        /// </summary>
        /// <param name="usingStatement">The using statement to generate</param>
        /// <returns>Generated C# code as string</returns>
        protected virtual string GenerateUsing(CodeGenerationUsing usingStatement)
        {
            var sb = new StringBuilder();

            if (usingStatement.IsGlobal) sb.Append("global ");

            sb.Append("using ");

            if (usingStatement.IsStatic) sb.Append("static ");

            if (!string.IsNullOrEmpty(usingStatement.Alias))
            {
                sb.Append($"{usingStatement.Alias} = ");
            }

            sb.Append($"{usingStatement.Namespace};");

            return sb.ToString();
        }

        /// <summary>
        /// Generates XML documentation from a documentation string
        /// </summary>
        /// <param name="documentation">The documentation text</param>
        /// <param name="indentLevel">Indentation level</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated XML documentation as string</returns>
        public virtual string GenerateDocumentation(string documentation, int indentLevel = 0, CodeGenerationOptions? options = null)
        {
            options ??= DefaultOptions;
            var sb = new StringBuilder();
            var indent = string.Concat(Enumerable.Repeat(options.Indentation, indentLevel));

            // Clean up the documentation string more aggressively
            if (string.IsNullOrWhiteSpace(documentation))
            {
                return string.Empty;
            }

            // Replace multiple consecutive whitespace/newlines with single spaces
            var cleanedDoc = System.Text.RegularExpressions.Regex.Replace(documentation.Trim(), @"\s+", " ");

            // Since we cleaned to one line, just output it directly
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// {cleanedDoc}");
            sb.AppendLine($"{indent}/// </summary>");

            return sb.ToString();
        }

        /// <summary>
        /// Generates attributes
        /// </summary>
        /// <param name="attributes">The attributes to generate</param>
        /// <param name="indentLevel">Indentation level</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Generated C# code as string</returns>
        public virtual string GenerateAttributes(List<string> attributes, int indentLevel = 0, CodeGenerationOptions? options = null)
        {
            options ??= DefaultOptions;
            var sb = new StringBuilder();
            var indent = string.Concat(Enumerable.Repeat(options.Indentation, indentLevel));

            foreach (var attribute in attributes)
            {
                sb.AppendLine($"{indent}[{attribute}]");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Indents code by the specified number of levels
        /// </summary>
        /// <param name="code">Code to indent</param>
        /// <param name="levels">Number of indentation levels</param>
        /// <param name="options">Code generation options</param>
        /// <returns>Indented code</returns>
        protected virtual string IndentCode(string code, int levels, CodeGenerationOptions? options = null)
        {
            options ??= DefaultOptions;
            var indent = string.Concat(Enumerable.Repeat(options.Indentation, levels));
            var lines = code.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

            var sb = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.Append(indent);
                }
                sb.Append(line);

                if (i < lines.Length - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}