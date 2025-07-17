using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace Yash.Config.Activities.Models
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
        /// <returns>An instance of TDerived populated with values from the dictionary.</returns>
        public static TDerived FromDictionary<TDerived>(Dictionary<string, object> dictionary) where TDerived : Config, new()
        {
            TDerived instance = new();

            // Retrieve all members (properties and fields)
            var members = typeof(TDerived).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field);

            foreach (var member in members)
            {
                object parsedValue;

                // Check if the dictionary contains a matching key
                if (!dictionary.TryGetValue(member.Name, out var rawValue))
                    continue;

                if (rawValue is string stringValue)
                {
                    // Get the type of the member
                    var memberType = member is PropertyInfo prop ? prop.PropertyType : ((FieldInfo)member).FieldType;

                    // Parse the value
                    if (TypeParsers.Parsers.TryGetValue(memberType, out var parser))
                    {
                        parsedValue = parser(stringValue);
                    }
                    else
                    {
                        try
                        {
                            return (TDerived)JsonConvert.DeserializeObject(stringValue, memberType);
                        }
                        catch (JsonException)
                        {
                            throw new NotSupportedException(
                                $"Type '{memberType.Name}' is not supported for '{member.Name}' in '{typeof(TDerived).Name}'.");
                        }
                    }
                }
                else
                {
                    // Directly map non-string values
                    parsedValue = rawValue;
                }

                // Set the value
                if (member is PropertyInfo property)
                {
                    property.SetValue(instance, parsedValue);
                }
                else if (member is FieldInfo field)
                {
                    field.SetValue(instance, parsedValue);
                }

                // Add to the instance dictionary
                instance[member.Name] = parsedValue;
            }

            return instance;
        }
    }
}