using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;

namespace Yash.Config.Models
{
    /// <summary>
    /// Provides a set of type parsers for converting string values into various primitive and collection types.
    /// </summary>
    public static class TypeParsers
    {
        /// <summary>
        /// Dictionary mapping types to their corresponding parsing functions.
        /// </summary>
        public static readonly Dictionary<Type, Func<string, object>> Parsers = new()
    {
        { typeof(string), value => value },
        { typeof(int), value => ParsePrimitive<int>(value) },
        { typeof(double), value => ParsePrimitive<double>(value) },
        { typeof(bool), value => bool.Parse(value) },
        { typeof(DateTime), value => DateTime.Parse(value) },
        { typeof(TimeSpan), value => TimeSpan.Parse(value) },
        { typeof(List<string>), value => ParseList<string>(value) },
        { typeof(List<int>), value => ParseList<int>(value) },
        { typeof(List<double>), value => ParseList<double>(value) },
        { typeof(List<bool>), value => ParseList<bool>(value) },
        { typeof(List<DateTime>), value => ParseList<DateTime>(value) },
        { typeof(List<TimeSpan>), value => ParseList<TimeSpan>(value) },
        { typeof(string[]), value => ParseArray<string>(value) },
        { typeof(int[]), value => ParseArray<int>(value) },
        { typeof(double[]), value => ParseArray<double>(value) },
        { typeof(bool[]), value => ParseArray<bool>(value) },
        { typeof(DateTime[]), value => ParseArray<DateTime>(value) },
        { typeof(TimeSpan[]), value => ParseArray<TimeSpan>(value) },
    };

        /// <summary>
        /// Parses a string value into the specified primitive type.
        /// </summary>
        /// <typeparam name="T">The type to parse the value into.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <returns>The parsed value of type T.</returns>
        private static T ParsePrimitive<T>(string value)
        {
            if (typeof(T) == typeof(DateTime))
            {
                return (T)(object)DateTime.Parse(value);  // Specific handling for DateTime
            }
            if (typeof(T) == typeof(TimeSpan))
            {
                return (T)(object)TimeSpan.Parse(value);  // Specific handling for TimeSpan
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value)); // Fallback to JsonConvert for other types
        }

        /// <summary>
        /// Parses a comma-separated string into a list of the specified type.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="value">The comma-separated string to parse.</param>
        /// <returns>A list of parsed values.</returns>
        private static List<T> ParseList<T>(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? new List<T>()
                : value.Split(',').Select(v => ParsePrimitive<T>(v)).ToList();
        }

        /// <summary>
        /// Parses a comma-separated string into an array of the specified type.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="value">The comma-separated string to parse.</param>
        /// <returns>An array of parsed values.</returns>
        private static T[] ParseArray<T>(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? new T[0]
                : value.Split(',').Select(v => ParsePrimitive<T>(v)).ToArray();
        }
    }
}