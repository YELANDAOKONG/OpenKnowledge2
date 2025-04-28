using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LibraryOpenKnowledge.Models;

[Serializable]
public class ScoreRecord : ISerializable
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
    public Dictionary<string, QuestionScore> QuestionScores { get; set; } = new();
    
    #region ISerializable
    
    public ScoreRecord() { }
    
    protected ScoreRecord(SerializationInfo info, StreamingContext context)
    {
        Id = info.GetString("Id") ?? Guid.NewGuid().ToString();
        ExamId = info.GetString("ExamId") ?? string.Empty;
        ExamTitle = info.GetString("ExamTitle") ?? string.Empty;
        UserId = info.GetString("UserId") ?? string.Empty;
        UserName = info.GetString("UserName") ?? string.Empty;
        Timestamp = info.GetDateTime("Timestamp");
        TotalScore = info.GetDouble("TotalScore");
        ObtainedScore = info.GetDouble("ObtainedScore");
        SectionScores = (Dictionary<string, double>)info.GetValue("SectionScores", typeof(Dictionary<string, double>))!;
        QuestionScores = (Dictionary<string, QuestionScore>)info.GetValue("QuestionScores", typeof(Dictionary<string, QuestionScore>))!;
    }
    
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Id", Id);
        info.AddValue("ExamId", ExamId);
        info.AddValue("ExamTitle", ExamTitle);
        info.AddValue("UserId", UserId);
        info.AddValue("UserName", UserName);
        info.AddValue("Timestamp", Timestamp);
        info.AddValue("TotalScore", TotalScore);
        info.AddValue("ObtainedScore", ObtainedScore);
        info.AddValue("SectionScores", SectionScores);
        info.AddValue("QuestionScores", QuestionScores);
    }
    
    #endregion
    
    public void CalculateScores(Examination examination)
    {
        if (examination?.ExaminationSections == null)
            return;
            
        TotalScore = examination.ExaminationMetadata.TotalScore;
        ObtainedScore = 0;
        SectionScores.Clear();
        
        foreach (var section in examination.ExaminationSections)
        {
            if (section?.Questions == null)
                continue;
                
            double sectionScore = 0;
            
            foreach (var question in section.Questions)
            {
                if (question.QuestionId == null) 
                    continue;
                    
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
                
                // Add to question scores dictionary
                QuestionScores[question.QuestionId] = new QuestionScore
                {
                    QuestionId = question.QuestionId,
                    MaxScore = question.Score,
                    ObtainedScore = questionScore,
                    IsCorrect = Math.Abs(questionScore - question.Score) < 0.001
                };
                
                // Add to section score
                sectionScore += questionScore;
            }
            
            // Add to section scores dictionary
            if (section.SectionId != null)
            {
                SectionScores[section.SectionId] = sectionScore;
            }
            
            // Add to total score
            ObtainedScore += sectionScore;
        }
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

