namespace LibraryOpenKnowledge.Models;

[Serializable]
public class SystemConfig
{
    public string? OpenAiApiUrl { get; set; } = null; // https://example.com/v1
    public string? OpenAiApiKey { get; set; } = null; // sk-*****************************
    public string? OpenAiModel { get; set; } = null; // deepseek-chat
    public double? OpenAiModelTemperature { get; set; } = 0.7;
}