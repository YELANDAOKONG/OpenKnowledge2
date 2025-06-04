using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LibraryOpenKnowledge.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibraryOpenKnowledge.Tools
{
    public class ExaminationSerializer
    {
        // Static JsonSerializerSettings to ensure consistent settings across all serialization operations
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter> { new TupleListJsonConverter() },
            NullValueHandling = NullValueHandling.Include
        };

        /// <summary>
        /// Serializes an examination to a file
        /// </summary>
        /// <param name="examination">The examination to serialize</param>
        /// <param name="filePath">Target file path</param>
        /// <param name="includeUserAnswers">Whether to include user answers, default is true</param>
        /// <returns>Whether serialization was successful</returns>
        public static bool SerializeToFile(Examination examination, string filePath, bool includeUserAnswers = true)
        {
            try
            {
                // If user answers should not be included, create a deep copy and clear them
                Examination examToSave = includeUserAnswers ? 
                    examination : CreateCopyWithoutUserAnswers(examination);

                // Convert to JSON with custom settings
                string jsonData = JsonConvert.SerializeObject(examToSave, _serializerSettings);
                
                // Write to file
                File.WriteAllText(filePath, jsonData, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to serialize examination: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deserializes an examination from a file
        /// </summary>
        /// <param name="filePath">Source file path</param>
        /// <param name="includeUserAnswers">Whether to include user answers, default is true</param>
        /// <returns>The deserialized examination object, or null if failed</returns>
        public static Examination? DeserializeFromFile(string filePath, bool includeUserAnswers = true)
        {
            try
            {
                // Read all text from file
                string jsonData = File.ReadAllText(filePath, Encoding.UTF8);
                
                // Deserialize the JSON data with custom settings
                Examination? examination = JsonConvert.DeserializeObject<Examination>(jsonData, _serializerSettings);
                
                // If user answers should not be included, clear them
                if (!includeUserAnswers && examination != null)
                {
                    ClearUserAnswers(examination);
                }
                
                return examination;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize examination: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Serializes an examination to a JSON string
        /// </summary>
        /// <param name="examination">The examination to serialize</param>
        /// <param name="includeUserAnswers">Whether to include user answers, default is true</param>
        /// <returns>JSON string representation of the examination</returns>
        public static string? SerializeToJson(Examination examination, bool includeUserAnswers = true)
        {
            try
            {
                // If user answers should not be included, create a deep copy and clear them
                Examination examToSave = includeUserAnswers ? 
                    examination : CreateCopyWithoutUserAnswers(examination);

                // Convert to JSON with custom settings
                return JsonConvert.SerializeObject(examToSave, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to serialize examination to JSON: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deserializes an examination from a JSON string
        /// </summary>
        /// <param name="jsonData">JSON string containing the serialized examination</param>
        /// <param name="includeUserAnswers">Whether to include user answers, default is true</param>
        /// <returns>The deserialized examination object, or null if failed</returns>
        public static Examination? DeserializeFromJson(string jsonData, bool includeUserAnswers = true)
        {
            try
            {
                // Deserialize the JSON data with custom settings
                Examination? examination = JsonConvert.DeserializeObject<Examination>(jsonData, _serializerSettings);
                
                // If user answers should not be included, clear them
                if (!includeUserAnswers && examination != null)
                {
                    ClearUserAnswers(examination);
                }
                
                return examination;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize examination from JSON: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a deep copy of an examination without user answers
        /// </summary>
        private static Examination CreateCopyWithoutUserAnswers(Examination original)
        {
            // Create a deep copy using JSON serialization with custom settings
            string jsonData = JsonConvert.SerializeObject(original, _serializerSettings);
            Examination copy = JsonConvert.DeserializeObject<Examination>(jsonData, _serializerSettings)!;
            
            // Clear all user answers
            ClearUserAnswers(copy);
            
            return copy;
        }

        /// <summary>
        /// Clears all user answers from an examination
        /// </summary>
        private static void ClearUserAnswers(Examination examination)
        {
            if (examination?.ExaminationSections == null)
                return;

            foreach (var section in examination.ExaminationSections)
            {
                if (section?.Questions == null)
                    continue;

                foreach (var question in section.Questions)
                {
                    question.UserAnswer = null;
                }
            }
        }
    }

    /// <summary>
    /// Custom JSON converter for List<(string, string)> to properly handle serialization of tuples
    /// </summary>
    public class TupleListJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // Check if the type is List<(string, string)>
            return objectType == typeof(List<(string, string)>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var result = new List<(string, string)>();
            
            // Handle null or empty array
            if (reader.TokenType == JsonToken.Null)
                return result;
                    
            // Make sure we're reading an array
            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException("Expected start of array");

            // Read array using JArray
            JArray array = JArray.Load(reader);
            
            // Process each item in the array
            foreach (JToken token in array)
            {
                if (token.Type == JTokenType.Object)
                {
                    // Extract Item1 and Item2 from the object
                    string item1 = token["Item1"]?.ToString() ?? string.Empty;
                    string item2 = token["Item2"]?.ToString() ?? string.Empty;
                    
                    result.Add((item1, item2));
                }
            }
            
            return result;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var tupleList = (List<(string, string)>)value;
            
            writer.WriteStartArray();
            
            foreach (var tuple in tupleList)
            {
                writer.WriteStartObject();
                
                writer.WritePropertyName("Item1");
                writer.WriteValue(tuple.Item1);
                
                writer.WritePropertyName("Item2");
                writer.WriteValue(tuple.Item2);
                
                writer.WriteEndObject();
            }
            
            writer.WriteEndArray();
        }
    }
    
    public class OptionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Option);
        }
        
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            JObject jsonObject = JObject.Load(reader);
            var option = new Option();
            
            if (jsonObject["Id"] != null && jsonObject["Text"] != null)
            {
                option.Id = jsonObject["Id"]!.ToString();
                option.Text = jsonObject["Text"]!.ToString();
            }
            else if (jsonObject["Item1"] != null && jsonObject["Item2"] != null)
            {
                option.Id = jsonObject["Item1"]!.ToString();
                option.Text = jsonObject["Item2"]!.ToString();
            }
            else
            {
                option.Id = string.Empty;
                option.Text = string.Empty;
            }
            
            return option;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var option = (Option)value;
            
            writer.WriteStartObject();
            
            // writer.WritePropertyName("Id");
            // writer.WriteValue(option.Id);
            // writer.WritePropertyName("Text");
            // writer.WriteValue(option.Text);
            
            writer.WritePropertyName("Item1");
            writer.WriteValue(option.Id);
            writer.WritePropertyName("Item2");
            writer.WriteValue(option.Text);
            
            writer.WriteEndObject();
        }
    }

}
