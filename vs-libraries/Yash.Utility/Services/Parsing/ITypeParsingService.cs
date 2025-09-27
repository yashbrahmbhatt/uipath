using System;
using System.Collections.Generic;
using System.Globalization;

namespace Yash.Utility.Services.Parsing
{
    /// <summary>
    /// Interface for the type parsing service that provides extensible string-to-type conversion.
    /// </summary>
    public interface ITypeParsingService
    {
        /// <summary>
        /// Registers a custom type parser.
        /// </summary>
        /// <param name="parser">The parser to register.</param>
        void RegisterParser(ITypeParser parser);

        /// <summary>
        /// Registers a type parser using a simple parsing function.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="parseFunction">Function that converts string to T.</param>
        /// <param name="supportsNullable">Whether this parser supports nullable versions.</param>
        void RegisterParser<T>(Func<string, CultureInfo?, T> parseFunction, bool supportsNullable = true);

        /// <summary>
        /// Unregisters a parser for the specified type.
        /// </summary>
        /// <param name="type">The type to unregister.</param>
        /// <returns>True if a parser was removed, false if no parser was registered for the type.</returns>
        bool UnregisterParser(Type type);

        /// <summary>
        /// Checks if a parser is registered for the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if a parser is registered, false otherwise.</returns>
        bool HasParser(Type type);

        /// <summary>
        /// Gets all registered types.
        /// </summary>
        /// <returns>Collection of types that have registered parsers.</returns>
        IEnumerable<Type> GetRegisteredTypes();

        /// <summary>
        /// Attempts to parse a string value to the specified type.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="targetType">The target type to parse to.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <param name="result">The parsed result if successful.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        bool TryParse(string value, Type targetType, CultureInfo? culture, out object? result);

        /// <summary>
        /// Attempts to parse a string value to the specified type.
        /// </summary>
        /// <typeparam name="T">The target type to parse to.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <param name="result">The parsed result if successful.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        bool TryParse<T>(string value, CultureInfo? culture, out T result);

        /// <summary>
        /// Parses a string value to the specified type, throwing an exception if parsing fails.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="targetType">The target type to parse to.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">Thrown when the value cannot be parsed.</exception>
        /// <exception cref="NotSupportedException">Thrown when no parser is registered for the target type.</exception>
        object Parse(string value, Type targetType, CultureInfo? culture = null);

        /// <summary>
        /// Parses a string value to the specified type.
        /// </summary>
        /// <typeparam name="T">The target type to parse to.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <returns>The parsed value of type T.</returns>
        /// <exception cref="FormatException">Thrown when the value cannot be parsed.</exception>
        /// <exception cref="NotSupportedException">Thrown when no parser is registered for the target type.</exception>
        T Parse<T>(string value, CultureInfo? culture = null);

        /// <summary>
        /// Gets a dictionary compatible with the original TypeParsers.Parsers format.
        /// </summary>
        /// <returns>Dictionary mapping types to parsing functions.</returns>
        Dictionary<Type, Func<string, object>> GetParserDictionary();
    }
}