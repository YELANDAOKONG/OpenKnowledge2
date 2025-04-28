using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
namespace LibraryOpenKnowledge.Models;
[Serializable]
public class ScoreRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public double TotalScore { get; set; } = 0;
    public double ObtainedScore { get; set; } = 0;
    public Dictionary<string, double> SectionScores { get; set; } = new();
    public Dictionary<string, Dictionary<string, QuestionScore>> QuestionScores { get; set; } = new();
    
    public void CalculateScores(Examination examination)
    {
        if (examination?.ExaminationSections == null)
            return;
            
        TotalScore = examination.ExaminationMetadata.TotalScore;
        ObtainedScore = 0;
        SectionScores.Clear();
        QuestionScores.Clear();
        
        foreach (var section in examination.ExaminationSections)
        {
            if (section?.Questions == null)
                continue;
                
            double sectionScore = 0;
            string sectionId = section.SectionId ?? section.Title;
            
            // 为每个章节创建问题分数字典
            var sectionQuestionScores = new Dictionary<string, QuestionScore>();
            QuestionScores[sectionId] = sectionQuestionScores;
            
            foreach (var question in section.Questions)
            {
                string questionId = question.QuestionId ?? Guid.NewGuid().ToString();
                double questionScore = 0;
                
                // For AI judged questions, the score is set by the AI
                if (question.IsAiJudge)
                {
                    // Get score from question if it was set by AI
                    questionScore = question.Score;
                }
                else
                {
                    // For automatically graded questions
                    bool isCorrect = IsAnswerCorrect(question);
                    questionScore = isCorrect ? question.Score : 0;
                }
                
                // Add to question scores dictionary for this section
                sectionQuestionScores[questionId] = new QuestionScore
                {
                    QuestionId = questionId,
                    MaxScore = question.Score,
                    ObtainedScore = questionScore,
                    IsCorrect = Math.Abs(questionScore - question.Score) < 0.001
                };
                
                // Add to section score
                sectionScore += questionScore;
            }
            
            // Add to section scores dictionary
            SectionScores[sectionId] = sectionScore;
            
            // Add to total score
            ObtainedScore += sectionScore;
        }
    }
    
    /// <summary>
    /// Gets the question score for a specific question
    /// </summary>
    /// <param name="questionId">The question ID</param>
    /// <returns>The question score or null if not found</returns>
    public QuestionScore? GetQuestionScore(string questionId)
    {
        foreach (var sectionScores in QuestionScores.Values)
        {
            if (sectionScores.TryGetValue(questionId, out var score))
            {
                return score;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Gets all question scores as a flattened dictionary
    /// </summary>
    /// <returns>Dictionary with question IDs as keys and scores as values</returns>
    public Dictionary<string, QuestionScore> GetAllQuestionScores()
    {
        var result = new Dictionary<string, QuestionScore>();
        
        foreach (var sectionScores in QuestionScores.Values)
        {
            foreach (var score in sectionScores)
            {
                result[score.Key] = score.Value;
            }
        }
        
        return result;
    }
    
    private bool IsAnswerCorrect(Question question)
    {
        if (question.UserAnswer == null || question.UserAnswer.Length == 0 || question.Answer == null || question.Answer.Length == 0)
            return false;
            
        switch (question.Type)
        {
            case QuestionTypes.SingleChoice:
                return question.UserAnswer[0].Trim().Equals(question.Answer[0].Trim(), StringComparison.OrdinalIgnoreCase);
                
            case QuestionTypes.MultipleChoice:
                // All selected options must match exactly
                if (question.UserAnswer.Length != question.Answer.Length)
                    return false;
                    
                var userOptions = new HashSet<string>(question.UserAnswer.Select(a => a.Trim()), StringComparer.OrdinalIgnoreCase);
                var correctOptions = new HashSet<string>(question.Answer.Select(a => a.Trim()), StringComparer.OrdinalIgnoreCase);
                return userOptions.SetEquals(correctOptions);
                
            case QuestionTypes.Judgment:
                return question.UserAnswer[0].Trim().Equals(question.Answer[0].Trim(), StringComparison.OrdinalIgnoreCase);
                
            case QuestionTypes.FillInTheBlank:
                return question.UserAnswer[0].Trim().Equals(question.Answer[0].Trim(), StringComparison.OrdinalIgnoreCase);
                
            default:
                return false; // Complex types require AI judgment
        }
    }
}
