using System;
using System.IO;
using System.Text;
using LibraryOpenKnowledge.Models;
using Newtonsoft.Json;

namespace LibraryOpenKnowledge.Tools
{
    public class ExaminationSerializer
    {
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

                // Convert to JSON with formatting for readability
                string jsonData = JsonConvert.SerializeObject(examToSave, Formatting.Indented);
                
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
                
                // Deserialize the JSON data
                Examination? examination = JsonConvert.DeserializeObject<Examination>(jsonData);
                
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

                // Convert to JSON with formatting for readability
                return JsonConvert.SerializeObject(examToSave, Formatting.Indented);
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
                // Deserialize the JSON data
                Examination? examination = JsonConvert.DeserializeObject<Examination>(jsonData);
                
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
            // Create a deep copy using JSON serialization
            string jsonData = JsonConvert.SerializeObject(original);
            Examination copy = JsonConvert.DeserializeObject<Examination>(jsonData)!;
            
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
}
