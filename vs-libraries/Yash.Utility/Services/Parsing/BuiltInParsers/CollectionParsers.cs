using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Yash.Utility.Services.Parsing.BuiltInParsers
{
    /// <summary>
    /// Base class for collection parsers that parse comma-separated values.
    /// </summary>
    /// <typeparam name="TCollection">The collection type (List&lt;T&gt; or T[]).</typeparam>
    /// <typeparam name="TElement">The element type.</typeparam>
    public abstract class CollectionParserBase<TCollection, TElement> : TypeParserBase<TCollection>
    {
        protected readonly TypeParserRegistry _registry;
        protected readonly string _delimiter;

        protected CollectionParserBase(TypeParserRegistry registry, string delimiter = ",")
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _delimiter = delimiter;
        }

        public override bool TryParse(string value, CultureInfo? culture, out TCollection result)
        {
            result = default(TCollection)!;

            if (string.IsNullOrWhiteSpace(value))
            {
                result = CreateEmptyCollection();
                return true;
            }

            try
            {
                var elements = value.Split(new[] { _delimiter }, StringSplitOptions.None)
                                   .Select(v => v.Trim())
                                   .ToArray();

                var parsedElements = new List<TElement>();

                foreach (var element in elements)
                {
                    if (_registry.TryParse<TElement>(element, culture, out var parsedElement))
                    {
                        parsedElements.Add(parsedElement);
                    }
                    else
                    {
                        return false; // Failed to parse an element
                    }
                }

                result = CreateCollection(parsedElements);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected abstract TCollection CreateEmptyCollection();
        protected abstract TCollection CreateCollection(IEnumerable<TElement> elements);
    }

    /// <summary>
    /// Type parser for List&lt;T&gt; collections.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public class ListParser<T> : CollectionParserBase<List<T>, T>
    {
        public ListParser(TypeParserRegistry registry, string delimiter = ",") : base(registry, delimiter) { }

        protected override List<T> CreateEmptyCollection()
        {
            return new List<T>();
        }

        protected override List<T> CreateCollection(IEnumerable<T> elements)
        {
            return elements.ToList();
        }
    }

    /// <summary>
    /// Type parser for T[] arrays.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public class ArrayParser<T> : CollectionParserBase<T[], T>
    {
        public ArrayParser(TypeParserRegistry registry, string delimiter = ",") : base(registry, delimiter) { }

        protected override T[] CreateEmptyCollection()
        {
            return new T[0];
        }

        protected override T[] CreateCollection(IEnumerable<T> elements)
        {
            return elements.ToArray();
        }
    }

    /// <summary>
    /// Factory for creating collection parsers with proper registry dependencies.
    /// </summary>
    public static class CollectionParserFactory
    {
        /// <summary>
        /// Creates a List&lt;string&gt; parser.
        /// </summary>
        public static ListParser<string> CreateStringListParser(TypeParserRegistry registry)
        {
            return new ListParser<string>(registry);
        }

        /// <summary>
        /// Creates a List&lt;int&gt; parser.
        /// </summary>
        public static ListParser<int> CreateIntListParser(TypeParserRegistry registry)
        {
            return new ListParser<int>(registry);
        }

        /// <summary>
        /// Creates a List&lt;double&gt; parser.
        /// </summary>
        public static ListParser<double> CreateDoubleListParser(TypeParserRegistry registry)
        {
            return new ListParser<double>(registry);
        }

        /// <summary>
        /// Creates a List&lt;bool&gt; parser.
        /// </summary>
        public static ListParser<bool> CreateBoolListParser(TypeParserRegistry registry)
        {
            return new ListParser<bool>(registry);
        }

        /// <summary>
        /// Creates a List&lt;DateTime&gt; parser.
        /// </summary>
        public static ListParser<DateTime> CreateDateTimeListParser(TypeParserRegistry registry)
        {
            return new ListParser<DateTime>(registry);
        }

        /// <summary>
        /// Creates a List&lt;TimeSpan&gt; parser.
        /// </summary>
        public static ListParser<TimeSpan> CreateTimeSpanListParser(TypeParserRegistry registry)
        {
            return new ListParser<TimeSpan>(registry);
        }

        /// <summary>
        /// Creates a string[] parser.
        /// </summary>
        public static ArrayParser<string> CreateStringArrayParser(TypeParserRegistry registry)
        {
            return new ArrayParser<string>(registry);
        }

        /// <summary>
        /// Creates an int[] parser.
        /// </summary>
        public static ArrayParser<int> CreateIntArrayParser(TypeParserRegistry registry)
        {
            return new ArrayParser<int>(registry);
        }

        /// <summary>
        /// Creates a double[] parser.
        /// </summary>
        public static ArrayParser<double> CreateDoubleArrayParser(TypeParserRegistry registry)
        {
            return new ArrayParser<double>(registry);
        }

        /// <summary>
        /// Creates a bool[] parser.
        /// </summary>
        public static ArrayParser<bool> CreateBoolArrayParser(TypeParserRegistry registry)
        {
            return new ArrayParser<bool>(registry);
        }

        /// <summary>
        /// Creates a DateTime[] parser.
        /// </summary>
        public static ArrayParser<DateTime> CreateDateTimeArrayParser(TypeParserRegistry registry)
        {
            return new ArrayParser<DateTime>(registry);
        }

        /// <summary>
        /// Creates a TimeSpan[] parser.
        /// </summary>
        public static ArrayParser<TimeSpan> CreateTimeSpanArrayParser(TypeParserRegistry registry)
        {
            return new ArrayParser<TimeSpan>(registry);
        }
    }
}