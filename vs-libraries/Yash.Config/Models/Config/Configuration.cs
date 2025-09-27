using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Yash.Config.Models.Config
{
    /// <summary>
    /// A dynamic dictionary-like class that supports property-based access
    /// and serialization using ISerializable and JSON.
    /// </summary>
    [Serializable] // Allows the class to be serialized using binary serialization.
    public class Configuration : DynamicObject, ISerializable
    {
        /// <summary>
        /// Stores dynamic data as key-value pairs.
        /// </summary>
        [JsonProperty("Data")] // Ensures this property is serialized as "Data" in JSON output.
        public virtual Dictionary<string, object> Data { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the DictionaryWithMembers class.
        /// </summary>
        public Configuration() { }

        /// <summary>
        /// Initializes a new instance of the DictionaryWithMembers class with a specified dictionary.
        /// </summary>
        public Configuration(Dictionary<string, object> data)
        {
            // Initialize the Data dictionary with the provided data
            Data = data ?? throw new ArgumentNullException(nameof(data), "Data cannot be null.");
        }

        /// <summary>
        /// Initializes a new instance of the DictionaryWithMembers class from serialized data.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected Configuration(SerializationInfo info, StreamingContext context)
        {
            // Deserialize the "Data" dictionary from the serialization stream
            Data = (Dictionary<string, object>)info.GetValue("Data", typeof(Dictionary<string, object>));
        }

        /// <summary>
        /// Serializes the object data.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize the "Data" dictionary
            info.AddValue("Data", Data);
        }

        /// <summary>
        /// Attempts to retrieve a member dynamically.
        /// </summary>
        /// <param name="binder">The binder containing member information.</param>
        /// <param name="result">The retrieved value.</param>
        /// <returns>True if the member was found; otherwise, false.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string propertyName = binder.Name;
            var property = GetType().GetProperty(propertyName);

            // Ensure the requested property exists in the class definition
            if (property == null)
                throw new InvalidOperationException($"Property '{propertyName}' does not exist on type '{GetType().Name}'.");

            // If the property type is another DictionaryWithMembers, initialize it lazily
            if (typeof(Configuration).IsAssignableFrom(property.PropertyType))
            {
                if (!Data.ContainsKey(propertyName))
                {
                    var nestedObject = Activator.CreateInstance(property.PropertyType) as Configuration;
                    Data[propertyName] = nestedObject;
                }
                result = Data[propertyName];
                return true;
            }

            // Retrieve value if it exists in the dictionary
            if (Data.TryGetValue(propertyName, out var value))
            {
                result = value;
                return true;
            }

            // If the key is missing, throw an exception to indicate an unresolved property
            throw new KeyNotFoundException($"Key '{propertyName}' not found in the dictionary.");
        }

        /// <summary>
        /// Attempts to set a member dynamically.
        /// </summary>
        /// <param name="binder">The binder containing member information.</param>
        /// <param name="value">The value to assign.</param>
        /// <returns>True if the value was set; otherwise, false.</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string propertyName = binder.Name;
            var property = GetType().GetProperty(propertyName);

            // Ensure the property exists before setting its value
            if (property == null)
                throw new InvalidOperationException($"Property '{propertyName}' does not exist on type '{GetType().Name}'.");

            // If assigning a nested DictionaryWithMembers, enforce type safety
            if (typeof(Configuration).IsAssignableFrom(property.PropertyType))
            {
                if (value is Configuration nestedObject)
                {
                    Data[propertyName] = nestedObject;
                    return true;
                }
                else
                {
                    throw new InvalidCastException($"Expected a DictionaryWithMembers type for property '{propertyName}'.");
                }
            }

            // Assign value normally for other types
            Data[propertyName] = value;
            return true;
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        public object this[string key]
        {
            get => Data.TryGetValue(key, out var value) ? value : throw new KeyNotFoundException($"Key '{key}' not found.");
            set => Data[key] = value;
        }

        /// <summary>
        /// Converts the stored dictionary data to a formatted JSON string.
        /// </summary>
        /// <returns>A formatted JSON string representation of the data.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(Data, Formatting.Indented);
        }
    }
}