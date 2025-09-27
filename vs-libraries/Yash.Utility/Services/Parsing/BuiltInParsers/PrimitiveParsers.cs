using System;
using System.Globalization;

namespace Yash.Utility.Services.Parsing.BuiltInParsers
{
    /// <summary>
    /// Type parser for string values (pass-through parser).
    /// </summary>
    public class StringParser : TypeParserBase<string>
    {
        public override bool SupportsNullable => false; // string is already nullable

        public override bool TryParse(string value, CultureInfo? culture, out string result)
        {
            result = value ?? "";
            return true;
        }

        public override string Parse(string value, CultureInfo? culture = null)
        {
            return value ?? "";
        }
    }

    /// <summary>
    /// Type parser for integer values with culture-aware parsing.
    /// </summary>
    public class IntParser : TypeParserBase<int>
    {
        public override bool TryParse(string value, CultureInfo? culture, out int result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0;
                return false;
            }

            // Try direct int parsing first
            if (int.TryParse(value, NumberStyles.Integer, culture ?? CultureInfo.CurrentCulture, out result))
            {
                return true;
            }

            // Try parsing as double and convert to int (handles cases like "42.0")
            if (double.TryParse(value, NumberStyles.Float, culture ?? CultureInfo.CurrentCulture, out var doubleValue))
            {
                try
                {
                    result = Convert.ToInt32(doubleValue);
                    return true;
                }
                catch (OverflowException)
                {
                    result = 0;
                    return false;
                }
            }

            result = 0;
            return false;
        }
    }

    /// <summary>
    /// Type parser for double values with culture-aware parsing.
    /// </summary>
    public class DoubleParser : TypeParserBase<double>
    {
        public override bool TryParse(string value, CultureInfo? culture, out double result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0.0;
                return false;
            }

            return double.TryParse(value, NumberStyles.Float, culture ?? CultureInfo.CurrentCulture, out result);
        }
    }

    /// <summary>
    /// Type parser for boolean values with flexible input handling.
    /// </summary>
    public class BoolParser : TypeParserBase<bool>
    {
        public override bool TryParse(string value, CultureInfo? culture, out bool result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = false;
                return false;
            }

            var normalizedValue = value.Trim().ToLowerInvariant();

            // Handle standard boolean representations
            if (bool.TryParse(normalizedValue, out result))
            {
                return true;
            }

            // Handle additional boolean representations
            result = normalizedValue switch
            {
                "1" or "yes" or "y" or "on" => true,
                "0" or "no" or "n" or "off" => false,
                _ => false
            };

            return normalizedValue is "1" or "yes" or "y" or "on" or "0" or "no" or "n" or "off";
        }
    }

    /// <summary>
    /// Type parser for DateTime values with culture-aware parsing.
    /// </summary>
    public class DateTimeParser : TypeParserBase<DateTime>
    {
        public override bool TryParse(string value, CultureInfo? culture, out DateTime result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }

            // Try parsing with culture-specific format
            if (DateTime.TryParse(value, culture ?? CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
            {
                return true;
            }

            // Try parsing with invariant culture (ISO format)
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return true;
            }

            // Try parsing common ISO 8601 formats
            var isoFormats = new[]
            {
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss.fff",
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                "yyyy-MM-dd",
                "yyyy/MM/dd",
                "MM/dd/yyyy",
                "dd/MM/yyyy"
            };

            foreach (var format in isoFormats)
            {
                if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return true;
                }
            }

            result = default;
            return false;
        }
    }

    /// <summary>
    /// Type parser for TimeSpan values with flexible format support.
    /// </summary>
    public class TimeSpanParser : TypeParserBase<TimeSpan>
    {
        public override bool TryParse(string value, CultureInfo? culture, out TimeSpan result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }

            // Try standard TimeSpan parsing
            if (TimeSpan.TryParse(value, culture ?? CultureInfo.CurrentCulture, out result))
            {
                return true;
            }

            // Try parsing with invariant culture
            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out result))
            {
                return true;
            }

            // Try parsing common formats
            var formats = new[]
            {
                @"h\:mm\:ss",
                @"hh\:mm\:ss",
                @"d\.h\:mm\:ss",
                @"d\.hh\:mm\:ss",
                @"mm\:ss",
                @"h\:mm",
                @"hh\:mm"
            };

            foreach (var format in formats)
            {
                if (TimeSpan.TryParseExact(value, format, CultureInfo.InvariantCulture, out result))
                {
                    return true;
                }
            }

            result = default;
            return false;
        }
    }

    /// <summary>
    /// Type parser for decimal values with culture-aware parsing.
    /// </summary>
    public class DecimalParser : TypeParserBase<decimal>
    {
        public override bool TryParse(string value, CultureInfo? culture, out decimal result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0m;
                return false;
            }

            return decimal.TryParse(value, NumberStyles.Number, culture ?? CultureInfo.CurrentCulture, out result);
        }
    }

    /// <summary>
    /// Type parser for float values with culture-aware parsing.
    /// </summary>
    public class FloatParser : TypeParserBase<float>
    {
        public override bool TryParse(string value, CultureInfo? culture, out float result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0f;
                return false;
            }

            return float.TryParse(value, NumberStyles.Float, culture ?? CultureInfo.CurrentCulture, out result);
        }
    }

    /// <summary>
    /// Type parser for Guid values.
    /// </summary>
    public class GuidParser : TypeParserBase<Guid>
    {
        public override bool TryParse(string value, CultureInfo? culture, out Guid result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }

            return Guid.TryParse(value, out result);
        }
    }
}