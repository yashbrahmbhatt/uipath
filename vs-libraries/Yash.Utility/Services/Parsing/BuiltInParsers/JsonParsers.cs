using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yash.Utility.Services.Parsing.BuiltInParsers
{
    /// <summary>
    /// Type parser for JObject values with JSON deserialization.
    /// </summary>
    public class JObjectParser : TypeParserBase<JObject>
    {
        public override bool TryParse(string value, CultureInfo? culture, out JObject result)
        {
            result = new JObject();

            if (string.IsNullOrWhiteSpace(value))
            {
                return true; // Empty JObject for empty input
            }

            try
            {
                var parsed = JsonConvert.DeserializeObject<JObject>(value);
                result = parsed ?? new JObject();
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override JObject Parse(string value, CultureInfo? culture = null)
        {
            if (TryParse(value, culture, out var result))
            {
                return result;
            }
            throw new FormatException($"Cannot parse '{value}' as JObject: invalid JSON format");
        }
    }

    /// <summary>
    /// Type parser for List&lt;JObject&gt; collections.
    /// </summary>
    public class JObjectListParser : TypeParserBase<List<JObject>>
    {
        private readonly TypeParserRegistry _registry;

        public JObjectListParser(TypeParserRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public override bool TryParse(string value, CultureInfo? culture, out List<JObject> result)
        {
            result = new List<JObject>();

            if (string.IsNullOrWhiteSpace(value))
            {
                return true; // Empty list for empty input
            }

            try
            {
                // Try parsing as JSON array first
                var jsonArray = JsonConvert.DeserializeObject<JArray>(value);
                if (jsonArray != null)
                {
                    foreach (var item in jsonArray)
                    {
                        if (item is JObject jobj)
                        {
                            result.Add(jobj);
                        }
                        else
                        {
                            // Convert other JToken types to JObject if possible
                            var converted = new JObject { ["value"] = item };
                            result.Add(converted);
                        }
                    }
                    return true;
                }

                // If not a JSON array, try parsing as comma-separated JSON objects
                var elements = value.Split(',');
                foreach (var element in elements)
                {
                    var trimmed = element.Trim();
                    if (_registry.TryParse<JObject>(trimmed, culture, out var jobject))
                    {
                        result.Add(jobject);
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Generic JSON parser that can deserialize to any type using JsonConvert.
    /// </summary>
    /// <typeparam name="T">The target type for JSON deserialization.</typeparam>
    public class JsonParser<T> : TypeParserBase<T>
    {
        private readonly JsonSerializerSettings? _settings;

        public JsonParser(JsonSerializerSettings? settings = null)
        {
            _settings = settings;
        }

        public override bool TryParse(string value, CultureInfo? culture, out T result)
        {
            result = default(T)!;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            try
            {
                var parsed = JsonConvert.DeserializeObject<T>(value, _settings);
                if (parsed != null)
                {
                    result = parsed;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public override T Parse(string value, CultureInfo? culture = null)
        {
            if (TryParse(value, culture, out var result))
            {
                return result;
            }
            throw new FormatException($"Cannot deserialize '{value}' as {typeof(T).Name}: invalid JSON format");
        }
    }
}