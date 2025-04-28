using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using LibraryOpenKnowledge.Tools;

namespace ConsoleKnowledge.I18n;

public class I18nService
{
    private static I18nService? _instance;
    private static readonly object LockObject = new object();
    
    private Dictionary<string, Dictionary<string, string>> _translations = new();
    private string _currentLanguage = "en";

    public static I18nService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (LockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new I18nService();
                    }
                }
            }
            return _instance;
        }
    }

    private I18nService()
    {
        LoadDefaultTranslations();
    }

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_translations.ContainsKey(value))
            {
                _currentLanguage = value;
            }
            else
            {
                throw new KeyNotFoundException($"Language '{value}' is not supported");
            }
        }
    }

    public string GetText(string key, params object[] args)
    {
        if (_translations.TryGetValue(_currentLanguage, out var languageDict) &&
            languageDict.TryGetValue(key, out var translation))
        {
            return args.Length > 0 ? string.Format(translation, args) : translation;
        }

        // Fallback to English if current language doesn't have the translation
        if (_currentLanguage != "en" && _translations.TryGetValue("en", out var englishDict) &&
            englishDict.TryGetValue(key, out var englishTranslation))
        {
            return args.Length > 0 ? string.Format(englishTranslation, args) : englishTranslation;
        }

        // Return the key itself if no translation found
        return key;
    }

    public bool IsLanguageSupported(string language)
    {
        return _translations.ContainsKey(language);
    }

    public IEnumerable<string> GetSupportedLanguages()
    {
        return _translations.Keys;
    }

    private void LoadDefaultTranslations()
    {
        // Load built-in translations
        AddTranslation("en", new Dictionary<string, string>
        {
            // Common
            {"app.title", "Exam System"},
            {"app.press_any_key", "Press any key to continue..."},
            {"app.back_to_menu", "Back to Main Menu"},
            
            // Main Menu
            {"menu.title", "What would you like to do?"},
            {"menu.browse", "Browse Exam Sections and Questions"},
            {"menu.answer", "Answer Questions"},
            {"menu.save", "Save Exam"},
            {"menu.stats", "View Statistics"},
            {"menu.exit", "Exit"},
            
            // Exam
            {"exam.title", "Title"},
            {"exam.description", "Description"},
            {"exam.subject", "Subject"},
            {"exam.total_score", "Total Score"},
            {"exam.no_sections", "No sections found in this examination."},
            
            // Section
            {"section.select", "Select a section to browse:"},
            {"section.title", "Section: {0}"},
            {"section.no_questions", "No questions found in this section."},
            
            // Question
            {"question.table.id", "ID"},
            {"question.table.type", "Type"},
            {"question.table.question", "Question"},
            {"question.table.answered", "Answered"},
            {"question.answered_yes", "Yes"},
            {"question.answered_no", "No"},
            {"question.select", "Select a question to view (or 0 to go back):"},
            {"question.type", "Type"},
            {"question.score", "Score"},
            {"question.question", "Question"},
            {"question.options", "Options"},
            {"question.your_answer", "Your Answer"},
            {"question.not_answered", "Not answered yet"},
            
            // Answer
            {"answer.section_select", "Select a section to answer questions:"},
            {"answer.question", "Question {0}/{1}"},
            {"answer.reference", "Reference Materials"},
            {"answer.previous", "Previous Answer"},
            {"answer.modify", "Would you like to modify your answer?"},
            {"answer.modify_yes", "Yes"},
            {"answer.modify_no", "No"},
            {"answer.modify_skip", "Skip"},
            {"answer.your_answer", "Your answer:"},
            {"answer.select_answer", "Select your answer:"},
            {"answer.multiple_answers", "Your answers (comma separated):"},
            {"answer.select_answers", "Select your answers:"},
            {"answer.essay_prompt", "Enter your essay (press Enter on an empty line to finish):"},
            {"answer.recorded", "Answer recorded! Press any key to continue..."},
            
            // AI Grading
            {"grading.title", "Grading with AI..."},
            {"grading.processing", "Processing..."},
            {"grading.submitting", "Submitting to AI for grading..."},
            {"grading.complete", "AI grading complete!"},
            {"grading.result", "AI Grading Result:"},
            
            // Save
            {"save.path", "Save path (press Enter to use original path):"},
            {"save.success", "Examination saved successfully to {0}"},
            {"save.failure", "Failed to save examination."},
            
            // Statistics
            {"stats.title", "Examination Statistics"},
            {"stats.total_questions", "Total Questions"},
            {"stats.answered_questions", "Answered Questions"},
            {"stats.completion_rate", "Completion Rate"}
        });
        
        AddTranslation("zh-CN", new Dictionary<string, string>
        {
            // Common
            {"app.title", "考试系统"},
            {"app.press_any_key", "按任意键继续..."},
            {"app.back_to_menu", "返回主菜单"},
            
            // Main Menu
            {"menu.title", "您想做什么？"},
            {"menu.browse", "浏览考试章节和问题"},
            {"menu.answer", "回答问题"},
            {"menu.save", "保存考试"},
            {"menu.stats", "查看统计信息"},
            {"menu.exit", "退出"},
            
            // Exam
            {"exam.title", "标题"},
            {"exam.description", "描述"},
            {"exam.subject", "科目"},
            {"exam.total_score", "总分"},
            {"exam.no_sections", "本考试中未找到章节。"},
            
            // Section
            {"section.select", "选择要浏览的章节:"},
            {"section.title", "章节: {0}"},
            {"section.no_questions", "本章节中未找到问题。"},
            
            // Question
            {"question.table.id", "编号"},
            {"question.table.type", "类型"},
            {"question.table.question", "问题"},
            {"question.table.answered", "已答"},
            {"question.answered_yes", "是"},
            {"question.answered_no", "否"},
            {"question.select", "选择要查看的问题（或输入0返回）:"},
            {"question.type", "类型"},
            {"question.score", "分数"},
            {"question.question", "问题"},
            {"question.options", "选项"},
            {"question.your_answer", "您的答案"},
            {"question.not_answered", "尚未回答"},
            
            // Answer
            {"answer.section_select", "选择要回答问题的章节:"},
            {"answer.question", "问题 {0}/{1}"},
            {"answer.reference", "参考资料"},
            {"answer.previous", "之前的答案"},
            {"answer.modify", "您想修改您的答案吗？"},
            {"answer.modify_yes", "是"},
            {"answer.modify_no", "否"},
            {"answer.modify_skip", "跳过"},
            {"answer.your_answer", "您的答案:"},
            {"answer.select_answer", "选择您的答案:"},
            {"answer.multiple_answers", "您的答案（用逗号分隔）:"},
            {"answer.select_answers", "选择您的答案:"},
            {"answer.essay_prompt", "输入您的论述（在空行上按回车键完成）:"},
            {"answer.recorded", "答案已记录！按任意键继续..."},
            
            // AI Grading
            {"grading.title", "AI评分中..."},
            {"grading.processing", "处理中..."},
            {"grading.submitting", "提交给AI进行评分..."},
            {"grading.complete", "AI评分完成！"},
            {"grading.result", "AI评分结果:"},
            
            // Save
            {"save.path", "保存路径（按回车键使用原路径）:"},
            {"save.success", "考试成功保存到 {0}"},
            {"save.failure", "保存考试失败。"},
            
            // Statistics
            {"stats.title", "考试统计"},
            {"stats.total_questions", "总题数"},
            {"stats.answered_questions", "已答题数"},
            {"stats.completion_rate", "完成率"}
        });
        
        // Try to load user translations
        try
        {
            string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
            string translationsDir = Path.Combine(configDir, "open-knowledge", "translations");
            
            if (Directory.Exists(translationsDir))
            {
                foreach (var file in Directory.GetFiles(translationsDir, "*.json"))
                {
                    try
                    {
                        string languageCode = Path.GetFileNameWithoutExtension(file);
                        string json = File.ReadAllText(file);
                        var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                        if (translations != null)
                        {
                            AddTranslation(languageCode, translations);
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore errors in user translation files
                    }
                }
            }
        }
        catch (Exception)
        {
            // Ignore errors loading user translations
        }
    }

    public void AddTranslation(string language, Dictionary<string, string> translations)
    {
        if (_translations.ContainsKey(language))
        {
            // Merge with existing translations
            foreach (var kvp in translations)
            {
                _translations[language][kvp.Key] = kvp.Value;
            }
        }
        else
        {
            _translations[language] = new Dictionary<string, string>(translations);
        }
    }
}
