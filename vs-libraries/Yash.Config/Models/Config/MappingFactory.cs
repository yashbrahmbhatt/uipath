using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;


namespace Yash.Config.Models.Config
{
    /// <summary>
    /// Factory class for mapping data structures to and from custom dictionary-based objects.
    /// </summary>
    public static class MappingFactory
    {
        /// <summary>
        /// Converts a DataTable into a list of DictionaryWithMembers-derived objects.
        /// </summary>
        /// <typeparam name="T">The derived type of DictionaryWithMembers.</typeparam>
        /// <param name="dt">The DataTable to convert.</param>
        /// <returns>A list of instances of the derived type populated with DataTable values.</returns>
        public static List<T> FromDataTable<T>(DataTable dt) where T : Configuration, new()
        {
            var list = new List<T>();

            var dictList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(JsonConvert.SerializeObject(dt));
            foreach (var dict in dictList)
            {
                list.Add(ConfigFactory.FromDictionary<T>(dict));
            }

            return list;
        }

        /// <summary>
        /// Creates a deep copy of a list of DictionaryWithMembers-derived objects.
        /// </summary>
        /// <typeparam name="T">The derived type of DictionaryWithMembers.</typeparam>
        /// <param name="original">The original list to copy.</param>
        /// <returns>A deep copy of the list.</returns>
        public static List<T> DeepCopy<T>(List<T> original) where T : Configuration, new()
        {
            var copy = new List<T>();
            foreach (T item in original)
            {
                var copied = ConfigFactory.FromDictionary<T>(item.Data);
                copy.Add(copied);
            }
            return copy;
        }
    }
}