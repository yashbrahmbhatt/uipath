using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TraceEventType = System.Diagnostics.TraceEventType;

namespace Yash.Utility.Services.Parsing
{
    /// <summary>
    /// Registry for type parsers that provides extensible string-to-type conversion capabilities.
    /// Supports registration of custom parsers and automatic handling of nullable types.
    /// </summary>
    public class TypeParserRegistry
    {
        private readonly Dictionary<Type, ITypeParser> _parsers = new();
        private readonly Action<string, TraceEventType>? _logger;

        /// <summary>
        /// Creates a new TypeParserRegistry instance.
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic output.</param>
        public TypeParserRegistry(Action<string, TraceEventType>? logger = null)
        {
            _logger = logger;
            RegisterDefaultParsers();
        }

        /// <summary>
        /// Registers a type parser for a specific type.
        /// </summary>
        /// <param name="parser">The parser to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when parser is null.</exception>
        public void RegisterParser(ITypeParser parser)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            _parsers[parser.TargetType] = parser;
            Log($"Registered parser for type {parser.TargetType.Name}", TraceEventType.Verbose);

            // Auto-register nullable version if the parser supports it
            if (parser.SupportsNullable && parser.TargetType.IsValueType)
            {
                var nullableType = typeof(Nullable<>).MakeGenericType(parser.TargetType);
                _parsers[nullableType] = new NullableTypeParser(parser);
                Log($"Auto-registered nullable parser for type {nullableType.Name}", TraceEventType.Verbose);
            }
        }

        /// <summary>
        /// Registers a type parser using a simple parsing function.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="parseFunction">Function that converts string to T.</param>
        /// <param name="supportsNullable">Whether this parser supports nullable versions.</param>
        public void RegisterParser<T>(Func<string, CultureInfo?, T> parseFunction, bool supportsNullable = true)
        {
            RegisterParser(new FunctionalTypeParser<T>(parseFunction, supportsNullable));
        }

        /// <summary>
        /// Unregisters a parser for the specified type.
        /// </summary>
        /// <param name="type">The type to unregister.</param>
        /// <returns>True if a parser was removed, false if no parser was registered for the type.</returns>
        public bool UnregisterParser(Type type)
        {
            var removed = _parsers.Remove(type);
            if (removed)
            {
                Log($"Unregistered parser for type {type.Name}", TraceEventType.Verbose);
                
                // Also remove nullable version if it exists
                if (type.IsValueType)
                {
                    var nullableType = typeof(Nullable<>).MakeGenericType(type);
                    if (_parsers.Remove(nullableType))
                    {
                        Log($"Unregistered nullable parser for type {nullableType.Name}", TraceEventType.Verbose);
                    }
                }
            }
            return removed;
        }

        /// <summary>
        /// Checks if a parser is registered for the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if a parser is registered, false otherwise.</returns>
        public bool HasParser(Type type)
        {
            return _parsers.ContainsKey(type);
        }

        /// <summary>
        /// Gets all registered types.
        /// </summary>
        /// <returns>Collection of types that have registered parsers.</returns>
        public IEnumerable<Type> GetRegisteredTypes()
        {
            return _parsers.Keys.ToList();
        }

        /// <summary>
        /// Attempts to parse a string value to the specified type.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="targetType">The target type to parse to.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <param name="result">The parsed result if successful.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public bool TryParse(string value, Type targetType, CultureInfo? culture, out object? result)
        {
            result = null;

            if (string.IsNullOrEmpty(value) && IsNullableType(targetType))
            {
                result = null;
                return true;
            }

            if (_parsers.TryGetValue(targetType, out var parser))
            {
                try
                {
                    return parser.TryParse(value, culture, out result);
                }
                catch (Exception ex)
                {
                    Log($"Parser for {targetType.Name} threw exception: {ex.Message}", TraceEventType.Warning);
                    return false;
                }
            }

            Log($"No parser registered for type {targetType.Name}", TraceEventType.Warning);
            return false;
        }

        /// <summary>
        /// Attempts to parse a string value to the specified type.
        /// </summary>
        /// <typeparam name="T">The target type to parse to.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <param name="result">The parsed result if successful.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public bool TryParse<T>(string value, CultureInfo? culture, out T result)
        {
            result = default(T)!;

            if (TryParse(value, typeof(T), culture, out var objResult))
            {
                if (objResult is T typedResult)
                {
                    result = typedResult;
                    return true;
                }
                else if (objResult == null && IsNullableType(typeof(T)))
                {
                    result = default(T)!;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Parses a string value to the specified type, throwing an exception if parsing fails.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="targetType">The target type to parse to.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">Thrown when the value cannot be parsed.</exception>
        /// <exception cref="NotSupportedException">Thrown when no parser is registered for the target type.</exception>
        public object Parse(string value, Type targetType, CultureInfo? culture = null)
        {
            if (TryParse(value, targetType, culture, out var result))
            {
                return result!;
            }

            if (!_parsers.ContainsKey(targetType))
            {
                throw new NotSupportedException($"No parser registered for type {targetType.Name}");
            }

            throw new FormatException($"Cannot parse '{value}' as {targetType.Name}");
        }

        /// <summary>
        /// Parses a string value to the specified type.
        /// </summary>
        /// <typeparam name="T">The target type to parse to.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <param name="culture">The culture to use for parsing (optional).</param>
        /// <returns>The parsed value of type T.</returns>
        /// <exception cref="FormatException">Thrown when the value cannot be parsed.</exception>
        /// <exception cref="NotSupportedException">Thrown when no parser is registered for the target type.</exception>
        public T Parse<T>(string value, CultureInfo? culture = null)
        {
            return (T)Parse(value, typeof(T), culture);
        }

        /// <summary>
        /// Creates a parser function compatible with the original TypeParsers.Parsers dictionary format.
        /// </summary>
        /// <param name="type">The target type.</param>
        /// <returns>A parsing function that takes a string and returns an object, or null if no parser is registered.</returns>
        public Func<string, object>? GetParserFunction(Type type)
        {
            if (_parsers.TryGetValue(type, out var parser))
            {
                return value => parser.Parse(value, CultureInfo.CurrentCulture);
            }
            return null;
        }

        /// <summary>
        /// Gets a dictionary compatible with the original TypeParsers.Parsers format.
        /// </summary>
        /// <returns>Dictionary mapping types to parsing functions.</returns>
        public Dictionary<Type, Func<string, object>> GetParserDictionary()
        {
            var result = new Dictionary<Type, Func<string, object>>();
            
            foreach (var kvp in _parsers)
            {
                var parser = kvp.Value;
                result[kvp.Key] = value => parser.Parse(value, CultureInfo.CurrentCulture);
            }
            
            return result;
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private void Log(string message, TraceEventType level = TraceEventType.Information)
        {
            _logger?.Invoke($"[TypeParserRegistry] {message}", level);
        }

        /// <summary>
        /// Registers the default set of built-in parsers.
        /// </summary>
        private void RegisterDefaultParsers()
        {
            // Register primitive type parsers
            RegisterParser(new BuiltInParsers.StringParser());
            RegisterParser(new BuiltInParsers.IntParser());
            RegisterParser(new BuiltInParsers.DoubleParser());
            RegisterParser(new BuiltInParsers.BoolParser());
            RegisterParser(new BuiltInParsers.DateTimeParser());
            RegisterParser(new BuiltInParsers.TimeSpanParser());
            RegisterParser(new BuiltInParsers.DecimalParser());
            RegisterParser(new BuiltInParsers.FloatParser());
            RegisterParser(new BuiltInParsers.GuidParser());

            // Register JSON parsers
            RegisterParser(new BuiltInParsers.JObjectParser());
            
            // Register collection parsers (these need to be registered after primitives since they depend on them)
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateStringListParser(this));
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateIntListParser(this));
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateDoubleListParser(this));
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateBoolListParser(this));
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateDateTimeListParser(this));
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateTimeSpanListParser(this));
            
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateStringArrayParser(this));
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateIntArrayParser(this));
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateDoubleArrayParser(this));
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateBoolArrayParser(this));
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateDateTimeArrayParser(this));
            RegisterParser(BuiltInParsers.CollectionParserFactory.CreateTimeSpanArrayParser(this));

            // Register JObject list parser
            RegisterParser(new BuiltInParsers.JObjectListParser(this));

            Log($"Registered {_parsers.Count} built-in parsers", TraceEventType.Information);
        }
    }
}