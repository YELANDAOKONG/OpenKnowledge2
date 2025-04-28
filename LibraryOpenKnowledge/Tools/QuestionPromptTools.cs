using System.Text;
using LibraryOpenKnowledge.Models;

namespace LibraryOpenKnowledge.Tools;

public class QuestionPromptTools
{
    // Basic information method (already provided)
    public static string GetQuestionInformation(Question question)
    {
        StringBuilder info = new StringBuilder();
        info.AppendLine($"@QuestionId: {question.QuestionId ?? "NULL"}");
        info.AppendLine($"@Type: {question.Type.ToString()}");
        info.AppendLine($"@Stem: {question.Stem}");
        info.AppendLine($"@Score: {question.Score}");
        info.AppendLine($"@UserAnswer: {string.Join(", ", question.UserAnswer ?? Array.Empty<string>())}");
        info.AppendLine($"@Answer: {string.Join(", ", question.Answer)}");
        info.AppendLine($"@ReferenceAnswer: {string.Join(", ", question.ReferenceAnswer ?? Array.Empty<string>())}");
        info.AppendLine($"@ReferenceMaterials: {string.Join(", ", question.ReferenceMaterials ?? Array.Empty<string>())}");
        return info.ToString();
    }
    
    // Method 1: Basic prompt for AI assessment (without grading specifics)
    public static string GetBasePrompt(Question question)
    {
        StringBuilder prompt = new StringBuilder();
        
        prompt.AppendLine("You are an educational assessment AI. Your task is to evaluate the student's answer to the following question.");
        prompt.AppendLine();
        
        // Question type specific instructions
        prompt.AppendLine($"Question Type: \"{GetQuestionTypeDescription(question.Type)}\"");
        prompt.AppendLine($"Question: ");
        prompt.AppendLine("\"\"\"");
        prompt.AppendLine(question.Stem);
        prompt.AppendLine("\"\"\"");
        
        // Add reference materials if available
        if (question.ReferenceMaterials != null && question.ReferenceMaterials.Length > 0)
        {
            prompt.AppendLine("\nReference Materials:");
            prompt.AppendLine("\"\"\"");
            foreach (var material in question.ReferenceMaterials)
            {
                prompt.AppendLine(material);
            }
            prompt.AppendLine("\"\"\"");
        }
        
        // Add user answer
        prompt.AppendLine("\nStudent's Answer:");
        if (question.UserAnswer != null && question.UserAnswer.Length > 0)
        {
            prompt.AppendLine("\"\"\"");
            foreach (var answer in question.UserAnswer)
            {
                prompt.AppendLine(answer);
            }
            prompt.AppendLine("\"\"\"");
        }
        else
        {
            prompt.AppendLine("[No answer provided]");
        }
        
        // Add correct answer
        prompt.AppendLine("\nCorrect Answer:");
        prompt.AppendLine("\"\"\"");
        foreach (var answer in question.Answer)
        {
            prompt.AppendLine(answer);
        }
        prompt.AppendLine("\"\"\"");
        
        // Add reference answer if available
        if (question.ReferenceAnswer != null && question.ReferenceAnswer.Length > 0)
        {
            prompt.AppendLine("\nReference Answer:");
            prompt.AppendLine("\"\"\"");
            foreach (var refAnswer in question.ReferenceAnswer)
            {
                prompt.AppendLine(refAnswer);
            }
            prompt.AppendLine("\"\"\"");
        }
        
        // Add custom instructions if available
        if (question.Commits != null && question.Commits.Length > 0)
        {
            prompt.AppendLine("\nSpecial Instructions:");
            prompt.AppendLine("\"\"\"");
            foreach (var commit in question.Commits)
            {
                prompt.AppendLine(commit);
            }
            prompt.AppendLine("\"\"\"");
        }
        
        return prompt.ToString();
    }
    
    // Method 2: Request structured JSON output for assessment
    public static string GetJsonGradingPrompt(Question question)
    {
        StringBuilder prompt = new StringBuilder();
        
        prompt.AppendLine(GetBasePrompt(question));
        prompt.AppendLine();
        prompt.AppendLine("Please provide your assessment in the following JSON format only:");
        prompt.AppendLine("```json");
        prompt.AppendLine("{");
        prompt.AppendLine("  \"isCorrect\": true/false,");
        prompt.AppendLine("  \"score\": X.X,");
        prompt.AppendLine("  \"maxScore\": " + question.Score + ",");
        prompt.AppendLine("  \"confidenceLevel\": 0.0-1.0,");
        
        // Add type-specific fields
        switch (question.Type)
        {
            case QuestionTypes.Essay:
            case QuestionTypes.ShortAnswer:
                prompt.AppendLine("  \"dimensions\": [");
                prompt.AppendLine("    {");
                prompt.AppendLine("      \"name\": \"Content\",");
                prompt.AppendLine("      \"score\": X.X,");
                prompt.AppendLine("      \"maxScore\": X.X");
                prompt.AppendLine("    },");
                prompt.AppendLine("    {");
                prompt.AppendLine("      \"name\": \"Structure\",");
                prompt.AppendLine("      \"score\": X.X,");
                prompt.AppendLine("      \"maxScore\": X.X");
                prompt.AppendLine("    }");
                prompt.AppendLine("  ],");
                break;
        }
        
        prompt.AppendLine("  \"feedback\": \"Brief feedback on the answer\"");
        prompt.AppendLine("}");
        prompt.AppendLine("```");
        prompt.AppendLine();
        prompt.AppendLine("Only respond with the JSON object, no other text.");
        
        return prompt.ToString();
    }
    
    // Method 3: Generate a prompt for detailed explanation
    public static string GetExplanationPrompt(Question question)
    {
        StringBuilder prompt = new StringBuilder();
        
        prompt.AppendLine(GetBasePrompt(question));
        prompt.AppendLine();
        prompt.AppendLine("Please provide a detailed explanation of the correct answer, including:");
        prompt.AppendLine("1. Why the correct answer is right");
        prompt.AppendLine("2. Common misconceptions related to this question");
        prompt.AppendLine("3. Key concepts that students should understand");
        
        if (question.Type == QuestionTypes.Math || question.Type == QuestionTypes.Calculation)
        {
            prompt.AppendLine("4. Step-by-step solution process");
            prompt.AppendLine("5. Alternative approaches to solve this problem");
        }
        
        return prompt.ToString();
    }
    
    // Helper method to get a description of question type
    private static string GetQuestionTypeDescription(QuestionTypes type)
    {
        return type switch
        {
            QuestionTypes.SingleChoice => "Single Choice Question",
            QuestionTypes.MultipleChoice => "Multiple Choice Question",
            QuestionTypes.Judgment => "True/False Question",
            QuestionTypes.FillInTheBlank => "Fill in the Blank Question",
            QuestionTypes.Math => "Mathematics Problem",
            QuestionTypes.Essay => "Essay Question",
            QuestionTypes.ShortAnswer => "Short Answer Question",
            QuestionTypes.Calculation => "Calculation Question",
            QuestionTypes.Complex => "Complex Question with Multiple Parts",
            QuestionTypes.Other => "Other Question Format",
            _ => "Unknown Question Type"
        };
    }
    
    public static AIGradingResult ParseAIResponse(string aiResponse)
    {
        // 尝试从AI响应中提取JSON部分
        int jsonStart = aiResponse.IndexOf('{');
        int jsonEnd = aiResponse.LastIndexOf('}');
    
        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            string jsonPart = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
            return AIGradingResult.FromJson(jsonPart);
        }
    
        // 如果找不到明确的JSON格式，尝试解析整个响应
        return AIGradingResult.FromJson(aiResponse);
    }

}
