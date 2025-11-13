using System.Text.Json;
using OpenKnowledge.Models;

namespace OpenKnowledge.Utilities.Models;

public class AIGradingResult
{
    // 基本评分信息
    public bool IsCorrect { get; set; }
    public double Score { get; set; }
    public double MaxScore { get; set; }
    public double ConfidenceLevel { get; set; }
    public string Feedback { get; set; } = string.Empty;
    
    // 维度评分（用于作文和简答题）
    public List<ScoringDimension> Dimensions { get; set; } = new();
    
    // 原始JSON
    public string RawJson { get; private set; } = string.Empty;
    
    // 解析是否成功
    public bool ParseSuccess { get; private set; } = false;
    public string? ParseError { get; private set; } = null;
    
    // 从JSON字符串解析结果
    public static AIGradingResult FromJson(string json)
    {
        var result = new AIGradingResult
        {
            RawJson = json
        };
        
        try
        {
            // 清理JSON字符串（移除可能的围绕代码块的标记）
            var cleanJson = CleanJsonString(json);
            
            // 使用System.Text.Json解析
            using JsonDocument document = JsonDocument.Parse(cleanJson);
            JsonElement root = document.RootElement;
            
            // 解析基本字段
            if (root.TryGetProperty("isCorrect", out JsonElement isCorrectElement))
                result.IsCorrect = isCorrectElement.GetBoolean();
            
            if (root.TryGetProperty("score", out JsonElement scoreElement))
                result.Score = scoreElement.GetDouble();
            
            if (root.TryGetProperty("maxScore", out JsonElement maxScoreElement))
                result.MaxScore = maxScoreElement.GetDouble();
            
            if (root.TryGetProperty("confidenceLevel", out JsonElement confidenceElement))
                result.ConfidenceLevel = confidenceElement.GetDouble();
            
            if (root.TryGetProperty("feedback", out JsonElement feedbackElement))
                result.Feedback = ExtractStringValue(feedbackElement);
            
            // 解析维度评分（如果存在）
            if (root.TryGetProperty("dimensions", out JsonElement dimensionsElement) && 
                dimensionsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement dimension in dimensionsElement.EnumerateArray())
                {
                    var scoringDimension = new ScoringDimension();
                    
                    if (dimension.TryGetProperty("name", out JsonElement nameElement))
                        scoringDimension.Name = ExtractStringValue(nameElement);
                    
                    if (dimension.TryGetProperty("score", out JsonElement dimScoreElement))
                        scoringDimension.Score = dimScoreElement.GetDouble();
                    
                    if (dimension.TryGetProperty("maxScore", out JsonElement dimMaxScoreElement))
                        scoringDimension.MaxScore = dimMaxScoreElement.GetDouble();
                    
                    result.Dimensions.Add(scoringDimension);
                }
            }
            
            result.ParseSuccess = true;
        }
        catch (Exception ex)
        {
            result.ParseSuccess = false;
            result.ParseError = ex.Message;
        }
        
        return result;
    }
    
      public static AIGradingResult FromJsonDefault(string json)
    {
        var result = new AIGradingResult
        {
            RawJson = json
        };
        
        try
        {
            // 清理JSON字符串（移除可能的围绕代码块的标记）
            var cleanJson = CleanJsonString(json);
            
            // 使用System.Text.Json解析
            using (JsonDocument document = JsonDocument.Parse(cleanJson))
            {
                JsonElement root = document.RootElement;
                
                // 解析基本字段
                if (root.TryGetProperty("isCorrect", out JsonElement isCorrectElement))
                    result.IsCorrect = isCorrectElement.GetBoolean();
                
                if (root.TryGetProperty("score", out JsonElement scoreElement))
                    result.Score = scoreElement.GetDouble();
                
                if (root.TryGetProperty("maxScore", out JsonElement maxScoreElement))
                    result.MaxScore = maxScoreElement.GetDouble();
                
                if (root.TryGetProperty("confidenceLevel", out JsonElement confidenceElement))
                    result.ConfidenceLevel = confidenceElement.GetDouble();
                
                if (root.TryGetProperty("feedback", out JsonElement feedbackElement))
                    result.Feedback = feedbackElement.GetString() ?? string.Empty;
                
                // 解析维度评分（如果存在）
                if (root.TryGetProperty("dimensions", out JsonElement dimensionsElement) && 
                    dimensionsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement dimension in dimensionsElement.EnumerateArray())
                    {
                        var scoringDimension = new ScoringDimension();
                        
                        if (dimension.TryGetProperty("name", out JsonElement nameElement))
                            scoringDimension.Name = nameElement.GetString() ?? string.Empty;
                        
                        if (dimension.TryGetProperty("score", out JsonElement dimScoreElement))
                            scoringDimension.Score = dimScoreElement.GetDouble();
                        
                        if (dimension.TryGetProperty("maxScore", out JsonElement dimMaxScoreElement))
                            scoringDimension.MaxScore = dimMaxScoreElement.GetDouble();
                        
                        result.Dimensions.Add(scoringDimension);
                    }
                }
            }
            
            result.ParseSuccess = true;
        }
        catch (Exception ex)
        {
            result.ParseSuccess = false;
            result.ParseError = ex.Message;
        }
        
        return result;
    }

    // 提取字符串值，处理可能的字符串数组
    private static string ExtractStringValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
            return element.GetString() ?? string.Empty;
        
        if (element.ValueKind == JsonValueKind.Array)
        {
            // 处理字符串数组，将其合并为带换行符的单个字符串
            var stringList = new List<string>();
            
            foreach (JsonElement item in element.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    string? value = item.GetString();
                    if (value != null)
                        stringList.Add(value);
                }
            }
            
            return string.Join("\n", stringList);
        }
        
        return string.Empty;
    }
    
    // 清理JSON字符串，移除可能的Markdown代码块标记等
    private static string CleanJsonString(string input)
    {
        // 移除可能的Markdown代码块标记
        string output = input.Trim();
        
        // 移除开始的```json标记
        if (output.StartsWith("```json"))
            output = output.Substring("```json".Length);
        else if (output.StartsWith("```"))
            output = output.Substring("```".Length);
        
        // 移除结尾的```标记
        if (output.EndsWith("```"))
            output = output.Substring(0, output.Length - "```".Length);
        
        return output.Trim();
    }
    
    // 将评分结果应用到问题对象
    public void ApplyToQuestion(Question question)
    {
        if (!ParseSuccess)
            return;
            
        // 设置得分
        question.Score = Score;
        
        // 可以添加其他需要更新的字段
    }
    
    // 生成评分报告
    public string GenerateReport()
    {
        if (!ParseSuccess)
            return $"Failed to parse AI grading result: {ParseError}";
            
        var report = new System.Text.StringBuilder();
        report.AppendLine("AI Grading Report");
        report.AppendLine("=================");
        report.AppendLine($"Result: {(IsCorrect ? "Correct" : "Incorrect")}");
        report.AppendLine($"Score: {Score}/{MaxScore} ({(MaxScore > 0 ? (Score / MaxScore * 100).ToString("F1") : "0")}%)");
        report.AppendLine($"Confidence Level: {(ConfidenceLevel * 100).ToString("F1")}%");
        
        if (Dimensions.Count > 0)
        {
            report.AppendLine("\nScoring Dimensions:");
            foreach (var dimension in Dimensions)
            {
                report.AppendLine($"- {dimension.Name}: {dimension.Score}/{dimension.MaxScore}");
            }
        }
        
        report.AppendLine("\nFeedback:");
        report.AppendLine(Feedback);
        
        return report.ToString();
    }
}
