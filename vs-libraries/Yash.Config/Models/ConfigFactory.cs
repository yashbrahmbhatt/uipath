using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using DocumentFormat.OpenXml.Drawing.Charts;
using Newtonsoft.Json;
using TraceEventType = System.Diagnostics.TraceEventType;

namespace Yash.Config.Models
{
    /// <summary>
    /// Factory class for creating instances of DictionaryWithMembers-derived types from dictionaries.
    /// </summary>
    public static class ConfigFactory
    {
        /// <summary>
        /// Creates an instance of a derived DictionaryWithMembers type from a dictionary of values.
        /// </summary>
        /// <typeparam name="TDerived">The derived type of DictionaryWithMembers to instantiate.</typeparam>
        /// <param name="dictionary">The dictionary containing property values.</param>
        /// <param name="Log">Optional logger for diagnostic output.</param>
        /// <returns>An instance of TDerived populated with values from the dictionary.</returns>
        public static TDerived FromDictionary<TDerived>(
            Dictionary<string, object> dictionary
        ) where TDerived : Config, new()
        {
            TDerived instance = new();
            Action<string, TraceEventType> Log = null;
            var members = typeof(TDerived).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field);

            foreach (var member in members)
            {
                if (!dictionary.TryGetValue(member.Name, out var rawValue))
                {
                    Log?.Invoke($"[FromDictionary] Skipped '{member.Name}' – no matching key in dictionary.", TraceEventType.Verbose);
                    continue;
                }

                object parsedValue;

                try
                {
                    if (rawValue is string stringValue)
                    {
                        var memberType = member is PropertyInfo prop ? prop.PropertyType : ((FieldInfo)member).FieldType;

                        if (TypeParsers.Parsers.TryGetValue(memberType, out var parser))
                        {
                            parsedValue = parser(stringValue);
                            Log?.Invoke($"[FromDictionary] Parsed '{member.Name}' as '{memberType.Name}' from string value.", TraceEventType.Verbose);
                        }
                        else
                        {
                            parsedValue = JsonConvert.DeserializeObject(stringValue, memberType) ?? throw new JsonException();
                            Log?.Invoke($"[FromDictionary] Deserialized '{member.Name}' to type '{memberType.Name}'.", TraceEventType.Verbose);
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
    }
}
