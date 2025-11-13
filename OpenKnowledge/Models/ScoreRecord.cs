using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace OpenKnowledge.Models;

[Serializable]
public class ScoreRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
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
                bool isCorrect = false;

                // 处理含有子问题的复合题
                if (question.Type == QuestionTypes.Complex && question.SubQuestions != null && question.SubQuestions.Count > 0)
                {
                    double subQuestionsTotal = 0;
                    double subQuestionsObtained = 0;
                    
                    // 计算每个子问题的分数
                    foreach (var subQuestion in question.SubQuestions)
                    {
                        double subQuestionScore = 0;
                        
                        // 对于AI评分的子问题，使用ObtainedScore
                        if (subQuestion.IsAiJudge)
                        {
                            // 从子问题的ObtainedScore获取分数
                            subQuestionScore = subQuestion.ObtainedScore ?? 0.0;
                        }
                        else
                        {
                            // 对于自动评分的子问题
                            bool subIsCorrect = IsAnswerCorrect(subQuestion);
                            subQuestionScore = subIsCorrect ? subQuestion.Score : 0;
                        }
                        
                        // 添加到子问题总分
                        subQuestionsTotal += subQuestion.Score;
                        subQuestionsObtained += subQuestionScore;
                        
                        // 在QuestionScores中保存子问题分数（可选）
                        string subQuestionId = subQuestion.QuestionId ?? $"{questionId}_sub_{subQuestion.GetHashCode()}";
                        sectionQuestionScores[subQuestionId] = new QuestionScore
                        {
                            QuestionId = subQuestionId,
                            MaxScore = subQuestion.Score,
                            ObtainedScore = subQuestionScore,
                            IsCorrect = Math.Abs(subQuestionScore - subQuestion.Score) < 0.001
                        };
                    }
                    
                    // 计算获得的子问题总分百分比
                    double percentage = subQuestionsTotal > 0 ? (subQuestionsObtained / subQuestionsTotal) : 0;
                    
                    // 将此百分比应用于主问题分数
                    questionScore = percentage * question.Score;
                    
                    // 如果得分至少为最高分的90%，则认为复合题是正确的
                    isCorrect = percentage >= 0.9;
                    
                    // 修复：如果复合题本身有答案字段，也应该单独评分
                    if (question.Answer != null && question.Answer.Length > 0)
                    {
                        // 复合题主题被视为一道独立题目
                        double mainQuestionScore = 0;
                        bool mainIsCorrect = false;
                        
                        if (question.IsAiJudge)
                        {
                            // AI评分复合题主题
                            mainQuestionScore = question.ObtainedScore ?? 0.0;
                            mainIsCorrect = Math.Abs(mainQuestionScore - question.Score) < 0.001;
                        }
                        else
                        {
                            // 自动评分复合题主题
                            mainIsCorrect = IsAnswerCorrect(question);
                            mainQuestionScore = mainIsCorrect ? question.Score : 0;
                        }
                        
                        // 更新总分（不更改之前计算的子问题总分）
                        questionScore = mainQuestionScore;
                        isCorrect = mainIsCorrect;
                    }
                }
                // 处理AI评分题
                else if (question.IsAiJudge)
                {
                    // 从问题的ObtainedScore获取分数
                    questionScore = question.ObtainedScore ?? 0.0;
                    isCorrect = Math.Abs(questionScore - question.Score) < 0.001;
                }
                // 处理自动评分题
                else
                {
                    isCorrect = IsAnswerCorrect(question);
                    questionScore = isCorrect ? question.Score : 0;
                }

                // 添加到问题分数字典
                sectionQuestionScores[questionId] = new QuestionScore
                {
                    QuestionId = questionId,
                    MaxScore = question.Score,
                    ObtainedScore = questionScore,
                    IsCorrect = isCorrect
                };

                // 添加到章节分数
                sectionScore += questionScore;
            }
            
            // 添加到章节分数字典
            SectionScores[sectionId] = sectionScore;
            
            // 添加到总分
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
        
        // 根据问题设置预处理文本
        Func<string, string> preprocessText = (text) => {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            
            string result = text;
            if (question.IgnoreSpace)
                result = result.Replace(" ", "");
            if (question.IgnoreLineBreak)
                result = result.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
            if (question.IgnoreCase)
                result = result.ToLowerInvariant();
            return result;
        };
        
        // 当任何问题类型禁用AI判题时，都应该回退到直接字符串比较，
        // 无需根据题目类型做不同处理
        if (!question.IsAiJudge)
        {
            return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
        }
        
        // 以下仅处理启用AI判题但尚未评分的情况
        switch (question.Type)
        {
            case QuestionTypes.SingleChoice:
                return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                
            case QuestionTypes.MultipleChoice:
                // 所有选项必须完全匹配
                if (question.UserAnswer.Length != question.Answer.Length)
                    return false;
                    
                var userOptions = new HashSet<string>(question.UserAnswer.Select(a => preprocessText(a)), StringComparer.Ordinal);
                var correctOptions = new HashSet<string>(question.Answer.Select(a => preprocessText(a)), StringComparer.Ordinal);
                return userOptions.SetEquals(correctOptions);
                
            case QuestionTypes.Judgment:
                return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                
            case QuestionTypes.FillInTheBlank:
                return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                
            case QuestionTypes.Math:
            case QuestionTypes.Calculation: 
            case QuestionTypes.Essay:
            case QuestionTypes.ShortAnswer:
            case QuestionTypes.Complex:
            default:
                // 对于需要AI判题的题目类型，返回false表示需要AI评分
                return false;
        }
    }


    
    private bool IsAnswerCorrectClassic(Question question)
    {
        if (question.UserAnswer == null || question.UserAnswer.Length == 0 || question.Answer == null || question.Answer.Length == 0)
            return false;
        
        // 根据问题设置预处理文本
        Func<string, string> preprocessText = (text) => {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            
            string result = text;
            if (question.IgnoreSpace)
                result = result.Replace(" ", "");
            if (question.IgnoreLineBreak)
                result = result.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
            if (question.IgnoreCase)
                result = result.ToLowerInvariant();
            return result;
        };
        
        switch (question.Type)
        {
            case QuestionTypes.SingleChoice:
                return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                
            case QuestionTypes.MultipleChoice:
                // 所有选项必须完全匹配
                if (question.UserAnswer.Length != question.Answer.Length)
                    return false;
                    
                var userOptions = new HashSet<string>(question.UserAnswer.Select(a => preprocessText(a)), StringComparer.Ordinal);
                var correctOptions = new HashSet<string>(question.Answer.Select(a => preprocessText(a)), StringComparer.Ordinal);
                return userOptions.SetEquals(correctOptions);
                
            case QuestionTypes.Judgment:
                return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                
            case QuestionTypes.FillInTheBlank:
                return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                
            case QuestionTypes.Math:
                if (!question.IsAiJudge)
                    return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                return false;
            
            case QuestionTypes.Calculation:
                // 对于数学和计算题，如果不使用AI判断，则使用直接比较
                if (!question.IsAiJudge)
                    return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                return false; // 默认需要AI判断
                
            case QuestionTypes.Essay:
                if (!question.IsAiJudge)
                    return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                return false;
            
            case QuestionTypes.ShortAnswer:
                // 对于作文和简答题，如果不使用AI判断，则使用直接比较
                if (!question.IsAiJudge)
                    return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                return false; // 默认需要AI判断
                
            case QuestionTypes.Complex:
                // 复合题通常需要分别评估子问题
                if (!question.IsAiJudge && question.UserAnswer.Length > 0 && question.Answer.Length > 0)
                    return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                return false;
                
            default:
                // 其他类型如果不使用AI判断，则使用直接比较
                if (!question.IsAiJudge && question.UserAnswer.Length > 0 && question.Answer.Length > 0)
                    return preprocessText(question.UserAnswer[0]).Equals(preprocessText(question.Answer[0]));
                return false;
        }
    }
    
}
