using System;
using System.Collections.Generic;
using System.Globalization;
using TraceEventType = System.Diagnostics.TraceEventType;

namespace Yash.Utility.Services.Parsing
{
    /// <summary>
    /// Service that provides extensible type parsing capabilities.
    /// Wraps TypeParserRegistry to provide a cleaner service interface.
    /// </summary>
    public class TypeParsingService : ITypeParsingService
    {
        private readonly TypeParserRegistry _registry;

        /// <summary>
        /// Creates a new TypeParsingService with built-in parsers registered.
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic output.</param>
        public TypeParsingService(Action<string, TraceEventType>? logger = null)
        {
            _registry = new TypeParserRegistry(logger);
        }

        /// <summary>
        /// Creates a TypeParsingService with a custom registry.
        /// </summary>
        /// <param name="registry">The type parser registry to use.</param>
        public TypeParsingService(TypeParserRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <inheritdoc/>
        public void RegisterParser(ITypeParser parser)
        {
            _registry.RegisterParser(parser);
        }

        /// <inheritdoc/>
        public void RegisterParser<T>(Func<string, CultureInfo?, T> parseFunction, bool supportsNullable = true)
        {
            _registry.RegisterParser(parseFunction, supportsNullable);
        }

        /// <inheritdoc/>
        public bool UnregisterParser(Type type)
        {
            return _registry.UnregisterParser(type);
        }

        /// <inheritdoc/>
        public bool HasParser(Type type)
        {
            return _registry.HasParser(type);
        }

        /// <inheritdoc/>
        public IEnumerable<Type> GetRegisteredTypes()
        {
            return _registry.GetRegisteredTypes();
        }

        /// <inheritdoc/>
        public bool TryParse(string value, Type targetType, CultureInfo? culture, out object? result)
        {
            return _registry.TryParse(value, targetType, culture, out result);
        }

        /// <inheritdoc/>
        public bool TryParse<T>(string value, CultureInfo? culture, out T result)
        {
            return _registry.TryParse(value, culture, out result);
        }

        /// <inheritdoc/>
        public object Parse(string value, Type targetType, CultureInfo? culture = null)
        {
            return _registry.Parse(value, targetType, culture);
        }

        /// <inheritdoc/>
        public T Parse<T>(string value, CultureInfo? culture = null)
        {
            return _registry.Parse<T>(value, culture);
        }

        /// <inheritdoc/>
        public Dictionary<Type, Func<string, object>> GetParserDictionary()
        {
            return _registry.GetParserDictionary();
        }

        /// <summary>
        /// Gets the underlying TypeParserRegistry for advanced operations.
        /// </summary>
        /// <returns>The TypeParserRegistry instance.</returns>
        public TypeParserRegistry GetRegistry()
        {
            return _registry;
        }
    }
}