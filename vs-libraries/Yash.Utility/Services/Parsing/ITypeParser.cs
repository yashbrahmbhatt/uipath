using System;
using System.Globalization;

namespace Yash.Utility.Services.Parsing
{
    /// <summary>
    /// Interface for type parsers that convert string values to specific types.
    /// </summary>
    public interface ITypeParser
    {
        /// <summary>
        /// The target type this parser handles.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Indicates whether this parser can handle nullable versions of its target type.
        /// </summary>
        bool SupportsNullable { get; }

        /// <summary>
        /// Attempts to parse the string value to the target type.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <param name="result">The parsed result if successful.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        bool TryParse(string value, CultureInfo? culture, out object? result);

        /// <summary>
        /// Parses the string value to the target type, throwing an exception if parsing fails.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">Thrown when the value cannot be parsed.</exception>
        object Parse(string value, CultureInfo? culture = null);
    }

    /// <summary>
    /// Generic interface for strongly-typed type parsers.
    /// </summary>
    /// <typeparam name="T">The target type to parse to.</typeparam>
    public interface ITypeParser<T> : ITypeParser
    {
        /// <summary>
        /// Attempts to parse the string value to the target type.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <param name="result">The parsed result if successful.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        bool TryParse(string value, CultureInfo? culture, out T result);

        /// <summary>
        /// Parses the string value to the target type.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <returns>The parsed value of type T.</returns>
        new T Parse(string value, CultureInfo? culture = null);
    }
}