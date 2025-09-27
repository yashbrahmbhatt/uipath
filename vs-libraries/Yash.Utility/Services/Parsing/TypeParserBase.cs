using System;
using System.Globalization;

namespace Yash.Utility.Services.Parsing
{
    /// <summary>
    /// Base class for type parsers that provides common functionality.
    /// </summary>
    /// <typeparam name="T">The target type this parser handles.</typeparam>
    public abstract class TypeParserBase<T> : ITypeParser<T>
    {
        /// <inheritdoc/>
        public Type TargetType => typeof(T);

        /// <inheritdoc/>
        public virtual bool SupportsNullable => true;

        /// <inheritdoc/>
        public abstract bool TryParse(string value, CultureInfo? culture, out T result);

        /// <inheritdoc/>
        public virtual T Parse(string value, CultureInfo? culture = null)
        {
            if (TryParse(value, culture, out var result))
            {
                return result;
            }
            throw new FormatException($"Cannot parse '{value}' as {typeof(T).Name}");
        }

        /// <inheritdoc/>
        bool ITypeParser.TryParse(string value, CultureInfo? culture, out object? result)
        {
            if (TryParse(value, culture, out var typedResult))
            {
                result = typedResult;
                return true;
            }
            result = null;
            return false;
        }

        /// <inheritdoc/>
        object ITypeParser.Parse(string value, CultureInfo? culture)
        {
            return Parse(value, culture)!;
        }
    }

    /// <summary>
    /// A type parser that wraps a parsing function for simple parser registration.
    /// </summary>
    /// <typeparam name="T">The target type this parser handles.</typeparam>
    public class FunctionalTypeParser<T> : TypeParserBase<T>
    {
        private readonly Func<string, CultureInfo?, T> _parseFunction;
        private readonly bool _supportsNullable;

        /// <summary>
        /// Creates a new FunctionalTypeParser.
        /// </summary>
        /// <param name="parseFunction">The function to use for parsing.</param>
        /// <param name="supportsNullable">Whether this parser supports nullable versions.</param>
        public FunctionalTypeParser(Func<string, CultureInfo?, T> parseFunction, bool supportsNullable = true)
        {
            _parseFunction = parseFunction ?? throw new ArgumentNullException(nameof(parseFunction));
            _supportsNullable = supportsNullable;
        }

        /// <inheritdoc/>
        public override bool SupportsNullable => _supportsNullable;

        /// <inheritdoc/>
        public override bool TryParse(string value, CultureInfo? culture, out T result)
        {
            try
            {
                result = _parseFunction(value, culture);
                return true;
            }
            catch
            {
                result = default(T)!;
                return false;
            }
        }

        /// <inheritdoc/>
        public override T Parse(string value, CultureInfo? culture = null)
        {
            return _parseFunction(value, culture);
        }
    }

    /// <summary>
    /// A wrapper parser that handles nullable versions of value types.
    /// </summary>
    internal class NullableTypeParser : ITypeParser
    {
        private readonly ITypeParser _innerParser;
        private readonly Type _nullableType;

        public NullableTypeParser(ITypeParser innerParser)
        {
            _innerParser = innerParser ?? throw new ArgumentNullException(nameof(innerParser));
            _nullableType = typeof(Nullable<>).MakeGenericType(innerParser.TargetType);
        }

        public Type TargetType => _nullableType;

        public bool SupportsNullable => false; // Nullable parsers don't need nested nullable support

        public bool TryParse(string value, CultureInfo? culture, out object? result)
        {
            result = null;

            // Handle null/empty values
            if (string.IsNullOrEmpty(value) || string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
            {
                result = null;
                return true;
            }

            // Delegate to inner parser
            if (_innerParser.TryParse(value, culture, out var innerResult))
            {
                result = innerResult;
                return true;
            }

            return false;
        }

        public object Parse(string value, CultureInfo? culture = null)
        {
            if (TryParse(value, culture, out var result))
            {
                return result!;
            }
            throw new FormatException($"Cannot parse '{value}' as {TargetType.Name}");
        }
    }
}