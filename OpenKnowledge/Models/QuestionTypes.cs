namespace OpenKnowledge.Models;

public enum QuestionTypes
{
    Unknown = 0,
    SingleChoice = 1, // 单选题
    MultipleChoice = 2, // 多选题
    Judgment = 3, // 判断题
    FillInTheBlank = 4, // 填空题
    Math = 5, // 数学题
    Essay = 6,  // 作文题 
    ShortAnswer = 7, // 简答题
    Calculation = 8, // 计算题
    Complex = 9, // 复合题
    Other = 10, // 其他题（通用模板）
}