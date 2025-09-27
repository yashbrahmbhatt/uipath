using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Yash.Utility.Services.Parsing;
using TraceEventType = System.Diagnostics.TraceEventType;

namespace Yash.Config.Models.Config
{
    /// <summary>
    /// Factory class for creating instances of DictionaryWithMembers-derived types from dictionaries.
    /// </summary>
    public static class ConfigFactory
    {
        // Shared type parsing service instance for all ConfigFactory operations
        private static readonly Lazy<ITypeParsingService> _typeParsingService = new(() => new TypeParsingService());
        
        /// <summary>
        /// Gets the type parsing service used by ConfigFactory.
        /// </summary>
        public static ITypeParsingService TypeParsingService => _typeParsingService.Value;
        /// <summary>
        /// Creates an instance of a derived DictionaryWithMembers type from a dictionary of values.
        /// Uses the extensible type parsing system from Yash.Utility.Services.Parsing for robust type conversion.
        /// </summary>
        /// <typeparam name="TDerived">The derived type of DictionaryWithMembers to instantiate.</typeparam>
        /// <param name="dictionary">The dictionary containing property values.</param>
        /// <param name="Log">Optional logger for diagnostic output.</param>
        /// <returns>An instance of TDerived populated with values from the dictionary.</returns>
        public static TDerived FromDictionary<TDerived>(
            Dictionary<string, object> dictionary,
            Action<string, TraceEventType>? Log = null
        ) where TDerived : Configuration, new()
        {
            TDerived instance = new();
            var members = typeof(TDerived).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field);

            foreach (var member in members)
            {
                if (!dictionary.TryGetValue(member.Name, out var rawValue))
                {
                   Log($"[FromDictionary] Skipped '{member.Name}' � no matching key in dictionary.", TraceEventType.Verbose);
                    continue;
                }

                object parsedValue;

                try
                {
                    if (rawValue is string stringValue)
                    {
                        var memberType = member is PropertyInfo prop ? prop.PropertyType : ((FieldInfo)member).FieldType;

                        if (TypeParsingService.TryParse(stringValue, memberType, CultureInfo.CurrentCulture, out var parsedTypeValue))
                        {
                            parsedValue = parsedTypeValue!;
                           Log?.Invoke($"[FromDictionary] Parsed '{member.Name}' as '{memberType.Name}' from string value using type parser.", TraceEventType.Verbose);
                        }
                        else
                        {
                            parsedValue = JsonConvert.DeserializeObject(stringValue, memberType) ?? throw new JsonException();
                           Log?.Invoke($"[FromDictionary] Deserialized '{member.Name}' to type '{memberType.Name}' using JSON fallback.", TraceEventType.Verbose);
                        }
                    }
                    else
                    {
                        parsedValue = rawValue;
                       Log?.Invoke($"[FromDictionary] Mapped '{member.Name}' directly with type '{rawValue.GetType().Name}'.", TraceEventType.Verbose);
                    }

                    if (member is PropertyInfo property)
                    {
                        property.SetValue(instance, parsedValue);
                    }
                    else if (member is FieldInfo field)
                    {
                        field.SetValue(instance, parsedValue);
                    }

                    instance[member.Name] = parsedValue;
                }
                catch (Exception ex)
                {
                   Log?.Invoke($"[FromDictionary] Failed to process '{member.Name}': {ex.Message}", TraceEventType.Error);
                    throw new NotSupportedException(
                        $"Failed to parse or assign member '{member.Name}' in '{typeof(TDerived).Name}': {ex.Message}", ex);
                }
            }

           Log?.Invoke($"[FromDictionary] Successfully created instance of '{typeof(TDerived).Name}'.", TraceEventType.Information);
            return instance;
        }

        public static object FromDictionary(
            Type derivedType,
            Dictionary<string, object> dictionary,
            Action<string, TraceEventType>? Log = null
        )
        {
            var method = typeof(ConfigFactory).GetMethod(nameof(FromDictionary), BindingFlags.Public | BindingFlags.Static);
            if (method == null)
                throw new InvalidOperationException("Could not find FromDictionary method.");
            var genericMethod = method.MakeGenericMethod(derivedType);
            return genericMethod.Invoke(null, new object[] { dictionary })!;
        }
    }
}
