using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConsoleKnowledge.I18n;
using LibraryOpenKnowledge.Models;
using LibraryOpenKnowledge.Tools;
using OpenAI;
using Spectre.Console;

namespace ConsoleKnowledge.Core;

public class ExamManager
{
    private readonly string _filePath;
    private Examination _examination;
    private readonly SystemConfig _config;
    private readonly OpenAIClient _aiClient;
    private readonly I18nService _i18n;
    private ScoreRecord? _currentScore;
    
    public ExamManager(string filePath)
    {
        _filePath = filePath;
        _config = ConfigTools.GetOrCreateSystemConfig();
        _aiClient = AiTools.CreateOpenAiClient(_config);
        _i18n = I18nService.Instance;
        
        // Load the examination
        var examination = ExaminationSerializer.DeserializeFromFile(filePath);
        if (examination == null)
        {
            throw new FileNotFoundException($"Could not load examination from {filePath}");
        }
        
        _examination = examination;
        _currentScore = ScoreManager.Instance.CreateScoreFromExamination(_examination);
    }
    
    public async Task RunAsync()
    {
        DisplayWelcomeScreen();
        
        while (true)
        {
            var action = DisplayMainMenu();
            
            switch (action)
            {
                case "browse":
                    BrowseExam();
                    break;
                case "answer":
                    await AnswerQuestionsAsync();
                    break;
                case "save":
                    SaveExam();
                    break;
                case "stats":
                    DisplayStatistics();
                    break;
                case "exit":
                    return;
            }
        }
    }
    
    private void DisplayWelcomeScreen()
    {
        AnsiConsole.Clear();
        
        var title = new FigletText(_i18n.GetText("app.title"))
            .LeftJustified()
            .Color(Color.Blue);
        
        AnsiConsole.Write(title);
        AnsiConsole.WriteLine();
        
        var metadata = _examination.ExaminationMetadata;
        AnsiConsole.MarkupLine($"[yellow]{_i18n.GetText("exam.title")}:[/] {metadata.Title}");
        if (!string.IsNullOrEmpty(metadata.Description))
        {
            AnsiConsole.MarkupLine($"[yellow]{_i18n.GetText("exam.description")}:[/] {metadata.Description}");
        }
        if (!string.IsNullOrEmpty(metadata.Subject))
        {
            AnsiConsole.MarkupLine($"[yellow]{_i18n.GetText("exam.subject")}:[/] {metadata.Subject}");
        }
        AnsiConsole.MarkupLine($"[yellow]{_i18n.GetText("exam.total_score")}:[/] {metadata.TotalScore}");
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]{_i18n.GetText("app.press_any_key")}[/]");
        Console.ReadKey(true);
    }
    
    private string DisplayMainMenu()
    {
        AnsiConsole.Clear();
        
        AnsiConsole.MarkupLine($"[blue]== {_examination.ExaminationMetadata.Title} ==[/]");
        AnsiConsole.WriteLine();
        
        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_i18n.GetText("menu.title"))
                .PageSize(10)
                .AddChoices(new[] {
                    _i18n.GetText("menu.browse"),
                    _i18n.GetText("menu.answer"),
                    _i18n.GetText("menu.save"),
                    _i18n.GetText("menu.stats"),
                    _i18n.GetText("menu.exit")
                }));
                
        if (option == _i18n.GetText("menu.browse")) return "browse";
        if (option == _i18n.GetText("menu.answer")) return "answer";
        if (option == _i18n.GetText("menu.save")) return "save";
        if (option == _i18n.GetText("menu.stats")) return "stats";
        if (option == _i18n.GetText("menu.exit")) return "exit";
        return "browse";
    }
    
    private void BrowseExam()
    {
        AnsiConsole.Clear();
        
        if (_examination.ExaminationSections == null || _examination.ExaminationSections.Length == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_i18n.GetText("exam.no_sections")}[/]");
            AnsiConsole.MarkupLine($"[green]{_i18n.GetText("app.press_any_key")}[/]");
            Console.ReadKey(true);
            return;
        }
        var sectionNames = _examination.ExaminationSections
            .Select(s => s.Title)
            .ToArray();
        
        var selectedSection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_i18n.GetText("section.select"))
                .PageSize(10)
                .AddChoices(sectionNames.Concat(new[] { _i18n.GetText("app.back_to_menu") })));
        
        if (selectedSection == _i18n.GetText("app.back_to_menu"))
        {
            return;
        }
        
        var section = _examination.ExaminationSections
            .First(s => s.Title == selectedSection);
        
        BrowseSection(section);
    }
    
    private void BrowseSection(ExaminationSection section)
    {
        AnsiConsole.Clear();
        
        AnsiConsole.MarkupLine($"[blue]== {_i18n.GetText("section.title", section.Title)} ==[/]");
        if (!string.IsNullOrEmpty(section.Description))
        {
            AnsiConsole.MarkupLine($"[yellow]{_i18n.GetText("exam.description")}:[/] {section.Description}");
        }
        AnsiConsole.WriteLine();
        
        if (section.Questions == null || section.Questions.Length == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_i18n.GetText("section.no_questions")}[/]");
            AnsiConsole.MarkupLine($"[green]{_i18n.GetText("app.press_any_key")}[/]");
            Console.ReadKey(true);
            return;
        }
        
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(_i18n.GetText("question.table.id"))
            .AddColumn(_i18n.GetText("question.table.type"))
            .AddColumn(_i18n.GetText("question.table.question"))
            .AddColumn(_i18n.GetText("question.table.answered"));
        
        for (int i = 0; i < section.Questions.Length; i++)
        {
            var question = section.Questions[i];
            table.AddRow(
                question.QuestionId ?? (i + 1).ToString(),
                question.Type.ToString(),
                question.Stem.Length > 50 ? question.Stem.Substring(0, 47) + "..." : question.Stem,
                (question.UserAnswer != null && question.UserAnswer.Length > 0) ? 
                    $"[green]{_i18n.GetText("question.answered_yes")}[/]" : 
                    $"[red]{_i18n.GetText("question.answered_no")}[/]"
            );
        }
        
        AnsiConsole.Write(table);
        
        // View individual question
        var questionIndices = Enumerable.Range(1, section.Questions.Length)
            .Select(i => i.ToString())
            .ToArray();
        
        var selectedQuestion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_i18n.GetText("question.select"))
                .PageSize(10)
                .AddChoices(questionIndices.Concat(new[] { "0" })));
        
        if (selectedQuestion == "0")
        {
            return;
        }
        
        int questionIndex = int.Parse(selectedQuestion) - 1;
        DisplayQuestion(section.Questions[questionIndex]);
        
        AnsiConsole.MarkupLine($"[green]{_i18n.GetText("app.press_any_key")}[/]");
        Console.ReadKey(true);
    }
    
    private void DisplayQuestion(Question question)
    {
        AnsiConsole.Clear();
        
        AnsiConsole.MarkupLine($"[blue]== {_i18n.GetText("question.question")} {question.QuestionId} ==[/]");
        AnsiConsole.MarkupLine($"[yellow]{_i18n.GetText("question.type")}:[/] {question.Type}");
        AnsiConsole.MarkupLine($"[yellow]{_i18n.GetText("question.score")}:[/] {question.Score}");
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine($"[green]{_i18n.GetText("question.question")}:[/]");
        AnsiConsole.WriteLine(question.Stem);
        AnsiConsole.WriteLine();
        
        // Display options for choice questions
        if ((question.Type == QuestionTypes.SingleChoice || question.Type == QuestionTypes.MultipleChoice) && 
            question.Options != null && question.Options.Count > 0)
        {
            AnsiConsole.MarkupLine($"[green]{_i18n.GetText("question.options")}:[/]");
            foreach (var option in question.Options)
            {
                AnsiConsole.MarkupLine($"[yellow]{option.Item1})[/] {option.Item2}");
            }
            AnsiConsole.WriteLine();
        }
        
        // Display user answer if available
        if (question.UserAnswer != null && question.UserAnswer.Length > 0)
        {
            AnsiConsole.MarkupLine($"[green]{_i18n.GetText("question.your_answer")}:[/]");
            foreach (var answer in question.UserAnswer)
            {
                AnsiConsole.WriteLine(answer);
            }
            AnsiConsole.WriteLine();
            
            // Display score if available
            if (_currentScore != null && question.QuestionId != null)
            {
                var questionScore = _currentScore.GetQuestionScore(question.QuestionId);
                if (questionScore != null)
                {
                    AnsiConsole.MarkupLine($"[yellow]Score:[/] {questionScore.ObtainedScore}/{questionScore.MaxScore}");
                    AnsiConsole.MarkupLine($"[yellow]Result:[/] {(questionScore.IsCorrect ? "[green]Correct[/]" : "[red]Incorrect[/]")}");
                    AnsiConsole.WriteLine();
                }
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]{_i18n.GetText("question.not_answered")}[/]");
            AnsiConsole.WriteLine();
        }
    }
    
    private async Task AnswerQuestionsAsync()
    {
        if (_examination.ExaminationSections == null || _examination.ExaminationSections.Length == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_i18n.GetText("exam.no_sections")}[/]");
            AnsiConsole.MarkupLine($"[green]{_i18n.GetText("app.press_any_key")}[/]");
            Console.ReadKey(true);
            return;
        }
        
        // Choose section
        var sectionNames = _examination.ExaminationSections
            .Select(s => s.Title)
            .ToArray();
        
        var selectedSection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_i18n.GetText("answer.section_select"))
                .PageSize(10)
                .AddChoices(sectionNames.Concat(new[] { _i18n.GetText("app.back_to_menu") })));
        
        if (selectedSection == _i18n.GetText("app.back_to_menu"))
        {
            return;
        }
        
        var section = _examination.ExaminationSections
            .First(s => s.Title == selectedSection);
        
        if (section.Questions == null || section.Questions.Length == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_i18n.GetText("section.no_questions")}[/]");
            AnsiConsole.MarkupLine($"[green]{_i18n.GetText("app.press_any_key")}[/]");
            Console.ReadKey(true);
            return;
        }
        
        // Answer questions in this section
        for (int i = 0; i < section.Questions.Length; i++)
        {
            await AnswerQuestionAsync(section.Questions[i], i + 1, section.Questions.Length);
        }
    }
    
    private async Task AnswerQuestionAsync(Question question, int current, int total)
    {
        AnsiConsole.Clear();
        
        AnsiConsole.MarkupLine($"[blue]== Question {current}/{total} ==[/]");
        AnsiConsole.MarkupLine($"[yellow]Type:[/] {question.Type}");
        AnsiConsole.MarkupLine($"[yellow]Score:[/] {question.Score}");
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[green]Question:[/]");
        AnsiConsole.WriteLine(question.Stem);
        AnsiConsole.WriteLine();
        
        // Display reference materials if available
        if (question.ReferenceMaterials != null)
        {
            AnsiConsole.MarkupLine("[green]Reference Materials:[/]");
            foreach (var referenceMaterial in question.ReferenceMaterials)
            {
                if (referenceMaterial != null && referenceMaterial.Materials.Length > 0)
                {
                    foreach (var material in referenceMaterial.Materials)
                    {
                        AnsiConsole.WriteLine(material);
                    }
                    
                }
            }
            AnsiConsole.WriteLine();
        }
        
        // Display options for choice questions
        if ((question.Type == QuestionTypes.SingleChoice || question.Type == QuestionTypes.MultipleChoice) && 
            question.Options != null && question.Options.Count > 0)
        {
            AnsiConsole.MarkupLine("[green]Options:[/]");
            foreach (var option in question.Options)
            {
                AnsiConsole.MarkupLine($"[yellow]{option.Item1})[/] {option.Item2}");
            }
            AnsiConsole.WriteLine();
        }
        
        // Show previous answer if available
        if (question.UserAnswer != null && question.UserAnswer.Length > 0)
        {
            AnsiConsole.MarkupLine($"[green]{_i18n.GetText("answer.previous")}:[/]");
            foreach (var answer in question.UserAnswer)
            {
                AnsiConsole.WriteLine(answer);
            }
            AnsiConsole.WriteLine();
            
            var modifyOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(_i18n.GetText("answer.modify"))
                    .AddChoices(new[] { 
                        _i18n.GetText("answer.modify_no"), 
                        _i18n.GetText("answer.modify_yes"), 
                        _i18n.GetText("answer.modify_skip")
                    }));
            
            if (modifyOption == _i18n.GetText("answer.modify_no") || modifyOption == _i18n.GetText("answer.modify_skip"))
            {
                return;
            }
        }
        
        // Get user answer based on question type
        string[] userAnswer;
        
        switch (question.Type)
        {
            case QuestionTypes.SingleChoice:
                userAnswer = new[] { GetSingleChoiceAnswer(question) };
                break;
            case QuestionTypes.MultipleChoice:
                userAnswer = GetMultipleChoiceAnswers(question);
                break;
            case QuestionTypes.Judgment:
                userAnswer = new[] { GetJudgmentAnswer() };
                break;
            case QuestionTypes.FillInTheBlank:
            case QuestionTypes.ShortAnswer:
            case QuestionTypes.Calculation:
            case QuestionTypes.Math:
                userAnswer = new[] { GetTextAnswer() };
                break;
            case QuestionTypes.Essay:
                userAnswer = new[] { GetEssayAnswer() };
                break;
            default:
                userAnswer = new[] { GetTextAnswer() };
                break;
        }
        
        // Set the user answer
        question.UserAnswer = userAnswer;
        
        // If AI judgment is required, use AI
        if (question.IsAiJudge && userAnswer.Length > 0 && !string.IsNullOrWhiteSpace(userAnswer[0]))
        {
            await GradeWithAiAsync(question);
            
            // Update score record after grading
            if (_currentScore != null)
            {
                _currentScore.CalculateScores(_examination);
                ScoreManager.Instance.SaveScore(_currentScore);
            }
        }
        else
        {
            // Update score record
            if (_currentScore != null)
            {
                _currentScore.CalculateScores(_examination);
                ScoreManager.Instance.SaveScore(_currentScore);
            }
        }
        
        AnsiConsole.MarkupLine($"[green]{_i18n.GetText("answer.recorded")}[/]");
        Console.ReadKey(true);
    }
    
    private string GetSingleChoiceAnswer(Question question)
    {
        if (question.Options == null || question.Options.Count == 0)
        {
            return AnsiConsole.Ask<string>($"[yellow]{_i18n.GetText("answer.your_answer")}[/]");
        }
        
        var options = question.Options
            .Select(o => $"{o.Item1}) {o.Item2}")
            .ToArray();
        
        var selectedOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]{_i18n.GetText("answer.select_answer")}[/]")
                .PageSize(10)
                .AddChoices(options));
        
        return selectedOption.Split(')')[0].Trim();
    }
    
    private string[] GetMultipleChoiceAnswers(Question question)
    {
        if (question.Options == null || question.Options.Count == 0)
        {
            var answer = AnsiConsole.Ask<string>($"[yellow]{_i18n.GetText("answer.multiple_answers")}[/]");
            return answer.Split(',').Select(s => s.Trim()).ToArray();
        }
        
        var options = question.Options
            .Select(o => $"{o.Item1}) {o.Item2}")
            .ToArray();
        
        var selectedOptions = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title($"[yellow]{_i18n.GetText("answer.select_answers")}[/]")
                .PageSize(10)
                .AddChoices(options));
        
        return selectedOptions
            .Select(o => o.Split(')')[0].Trim())
            .ToArray();
    }
    
    private string GetJudgmentAnswer()
    {
        var options = new[] { "True", "False" };
        
        var selectedOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]{_i18n.GetText("answer.your_answer")}[/]")
                .AddChoices(options));
        
        return selectedOption;
    }
    
    private string GetTextAnswer()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[yellow]{_i18n.GetText("answer.your_answer")}[/]")
                .AllowEmpty());
    }
    
    private string GetEssayAnswer()
    {
        AnsiConsole.MarkupLine($"[yellow]{_i18n.GetText("answer.essay_prompt")}[/]");
        AnsiConsole.WriteLine();
        
        var lines = new List<string>();
        string line;
        
        do
        {
            line = Console.ReadLine() ?? string.Empty;
            if (!string.IsNullOrEmpty(line))
            {
                lines.Add(line);
            }
        } while (!string.IsNullOrEmpty(line));
        
        return string.Join(Environment.NewLine, lines);
    }
    
    private async Task GradeWithAiAsync(Question question)
    {
        AnsiConsole.MarkupLine($"[blue]{_i18n.GetText("grading.title")}[/]");
        
        await AnsiConsole.Status()
            .StartAsync(_i18n.GetText("grading.processing"), async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.Status(_i18n.GetText("grading.submitting"));
                
                // Generate prompt
                var prompt = QuestionPromptTools.GetJsonGradingPrompt(question);
                
                // Get AI response
                var aiResponse = await AiTools.SendChatMessageAsync(_aiClient, _config, prompt);
                
                // Parse response
                var gradingResult = QuestionPromptTools.ParseAIResponse(aiResponse!);
                
                // Display result
                ctx.Status(_i18n.GetText("grading.complete"));
                
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine($"[green]{_i18n.GetText("grading.result")}[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Markup(gradingResult.GenerateReport()));
                
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[yellow]{_i18n.GetText("app.press_any_key")}[/]");
                Console.ReadKey(true);
            });
    }
    
    private void SaveExam()
    {
        AnsiConsole.Clear();
        
        var savePath = AnsiConsole.Ask(_i18n.GetText("save.path"), _filePath);
        
        if (string.IsNullOrWhiteSpace(savePath))
        {
            savePath = _filePath;
        }
        
        bool success = ExaminationSerializer.SerializeToFile(_examination, savePath);
        
        if (success)
        {
            AnsiConsole.MarkupLine($"[green]{_i18n.GetText("save.success", savePath)}[/]");
            
            // 保存分数记录
            if (_currentScore != null)
            {
                _currentScore.CalculateScores(_examination);
                if (ScoreManager.Instance.SaveScore(_currentScore))
                {
                    AnsiConsole.MarkupLine("[green]Score record saved successfully[/]");
                }
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]{_i18n.GetText("save.failure")}[/]");
        }
        
        AnsiConsole.MarkupLine($"[yellow]{_i18n.GetText("app.press_any_key")}[/]");
        Console.ReadKey(true);
    }
    
    private void DisplayStatistics()
    {
        AnsiConsole.Clear();
        
        AnsiConsole.MarkupLine($"[blue]== {_i18n.GetText("stats.title")} ==[/]");
        AnsiConsole.WriteLine();
        
        int totalQuestions = 0;
        int answeredQuestions = 0;
        
        if (_examination.ExaminationSections != null)
        {
            foreach (var section in _examination.ExaminationSections)
            {
                if (section.Questions != null)
                {
                    totalQuestions += section.Questions.Length;
                    answeredQuestions += section.Questions.Count(q => q.UserAnswer != null && q.UserAnswer.Length > 0);
                }
            }
        }
        
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Metric")
            .AddColumn("Value");
        
        table.AddRow(_i18n.GetText("stats.total_questions"), totalQuestions.ToString());
        table.AddRow(_i18n.GetText("stats.answered_questions"), answeredQuestions.ToString());
        table.AddRow(_i18n.GetText("stats.completion_rate"), $"{(totalQuestions > 0 ? (double)answeredQuestions / totalQuestions * 100 : 0):F1}%");
        
        // 显示当前分数记录
        if (_currentScore != null)
        {
            table.AddRow("Current Score", $"{_currentScore.ObtainedScore}/{_currentScore.TotalScore}");
            table.AddRow("Score Percentage", $"{(_currentScore.TotalScore > 0 ? _currentScore.ObtainedScore / _currentScore.TotalScore * 100 : 0):F1}%");
        }
        
        AnsiConsole.Write(table);
        
        // 显示章节分数
        if (_currentScore != null && _currentScore.SectionScores.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[blue]== Section Scores ==[/]");
            
            var sectionTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Section")
                .AddColumn("Score");
                
            foreach (var score in _currentScore.SectionScores)
            {
                sectionTable.AddRow(
                    score.Key,
                    $"{score.Value}"
                );
            }
            
            AnsiConsole.Write(sectionTable);
        }
        
        // 显示问题分数详情
        if (_currentScore != null && _currentScore.QuestionScores.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[blue]== Question Scores ==[/]");
            
            foreach (var sectionEntry in _currentScore.QuestionScores)
            {
                AnsiConsole.MarkupLine($"[yellow]Section: {sectionEntry.Key}[/]");
                
                var scoreTable = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("Question ID")
                    .AddColumn("Score")
                    .AddColumn("Result");
                    
                foreach (var score in sectionEntry.Value)
                {
                    scoreTable.AddRow(
                        score.Key,
                        $"{score.Value.ObtainedScore}/{score.Value.MaxScore}",
                        score.Value.IsCorrect ? "[green]Correct[/]" : "[red]Incorrect[/]"
                    );
                }
                
                AnsiConsole.Write(scoreTable);
                AnsiConsole.WriteLine();
            }
        }
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]{_i18n.GetText("app.press_any_key")}[/]");
        Console.ReadKey(true);
    }
}