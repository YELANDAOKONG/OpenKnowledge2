using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI;
using LibraryOpenKnowledge.Models;
using OpenAI.Chat;

namespace LibraryOpenKnowledge.Tools;

public class AiTools
{
    /// <summary>
    /// Creates an OpenAI client based on system configuration
    /// </summary>
    /// <param name="config">System configuration object</param>
    /// <returns>OpenAI client instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when required configuration parameters are null</exception>
    public static OpenAIClient CreateOpenAiClient(SystemConfig config)
    {
        if (string.IsNullOrEmpty(config.OpenAiApiKey))
        {
            throw new ArgumentNullException(nameof(config.OpenAiApiKey), "OpenAI API Key cannot be empty");
        }

        var options = new OpenAIClientOptions() { };

        // Set custom API URL if provided
        if (!string.IsNullOrEmpty(config.OpenAiApiUrl))
        {
            options.Endpoint = new Uri(config.OpenAiApiUrl);
        }
        
        ApiKeyCredential credential = new ApiKeyCredential(config.OpenAiApiKey);
        return new OpenAIClient(credential, options);
    }

    /// <summary>
    /// Loads configuration from file and creates an OpenAI client
    /// </summary>
    /// <param name="configFileName">Configuration file name, defaults to system_config.json</param>
    /// <returns>OpenAI client instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when configuration cannot be loaded or is invalid</exception>
    public static OpenAIClient CreateOpenAiClientFromConfig(string configFileName = "system_config.json")
    {
        var config = ConfigTools.LoadConfig<SystemConfig>(configFileName);
        
        if (config == null)
        {
            throw new InvalidOperationException($"Failed to load configuration from file {configFileName}");
        }

        return CreateOpenAiClient(config);
    }

    /// <summary>
    /// Sends a chat message to the OpenAI API
    /// </summary>
    /// <param name="client">OpenAI client</param>
    /// <param name="config">System configuration</param>
    /// <param name="message">User message content</param>
    /// <param name="systemMessage">System message content (optional)</param>
    /// <param name="temperature">Temperature parameter for the model (optional)</param>
    /// <param name="format">Response format (optional)</param>
    /// <param name="throwExceptions">Whether to throw exceptions on API errors</param>
    /// <returns>AI response text</returns>
    public static async Task<string?> SendChatMessageAsync(
        OpenAIClient client, 
        SystemConfig config, 
        string message, 
        string systemMessage = "You are a helpful AI assistant.",
        float temperature = 0,
        ChatResponseFormat? format = null,
        bool throwExceptions = false
    )
    {
        try
        {
            var chat = client.GetChatClient(string.IsNullOrEmpty(config.OpenAiModel) ? "gpt-3.5-turbo" : config.OpenAiModel);
            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateAssistantMessage(systemMessage),
                ChatMessage.CreateUserMessage(message)
            };
        
            var options = new ChatCompletionOptions();
            if (temperature == 0)
            {
                options.Temperature = config.OpenAiModelTemperature == null ? 0.7f : (float) config.OpenAiModelTemperature;
            }
            else
            {
                options.Temperature = temperature;
            }

            if (format != null)
            {
                options.ResponseFormat = format;
            }
        
        
            var result = await chat.CompleteChatAsync(messages);
        
            if (result.Value.Content != null)
            {
                return result.Value.Content.First().Text;
            }

            return null;
        }
        catch (Exception ex) when (!throwExceptions)
        {
            return null;
        }
    }

    /// <summary>
    /// Shortcut method to get or create configuration and create an OpenAI client
    /// </summary>
    /// <param name="configFileName">Configuration file name, defaults to system_config.json</param>
    /// <returns>OpenAI client instance</returns>
    public static OpenAIClient GetOrCreateOpenAiClient(string configFileName = "open-knowledge.json")
    {
        var config = ConfigTools.GetOrCreateSystemConfig(configFileName);
        return CreateOpenAiClient(config);
    }
}

