using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    public ExamManager(string filePath)
    {
        _filePath = filePath;
        _config = ConfigTools.GetOrCreateSystemConfig();
        _aiClient = AiTools.CreateOpenAiClient(_config);
        
        // Load the examination
        var examination = ExaminationSerializer.DeserializeFromFile(filePath);
        if (examination == null)
        {
            throw new FileNotFoundException($"Could not load examination from {filePath}");
        }
        
        _examination = examination;
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
        
        var title = new FigletText("Exam System")
            .LeftJustified()
            .Color(Color.Blue);
        
        AnsiConsole.Write(title);
        AnsiConsole.WriteLine();
        
        var metadata = _examination.ExaminationMetadata;
        AnsiConsole.MarkupLine($"[yellow]Title:[/] {metadata.Title}");
        if (!string.IsNullOrEmpty(metadata.Description))
        {
            AnsiConsole.MarkupLine($"[yellow]Description:[/] {metadata.Description}");
        }
        if (!string.IsNullOrEmpty(metadata.Subject))
        {
            AnsiConsole.MarkupLine($"[yellow]Subject:[/] {metadata.Subject}");
        }
        AnsiConsole.MarkupLine($"[yellow]Total Score:[/] {metadata.TotalScore}");
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
    private string DisplayMainMenu()
    {
        AnsiConsole.Clear();
        
        AnsiConsole.MarkupLine($"[blue]== {_examination.ExaminationMetadata.Title} ==[/]");
        AnsiConsole.WriteLine();
        
        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .PageSize(10)
                .AddChoices(new[] {
                    "Browse Exam Sections and Questions",
                    "Answer Questions",
                    "Save Exam",
                    "View Statistics",
                    "Exit"
                }));
        switch (option)
        {
            case "Browse Exam Sections and Questions": return "browse";
            case "Answer Questions": return "answer";
            case "Save Exam": return "save";
            case "View Statistics": return "stats";
            case "Exit": return "exit";
            default: return "browse";
        }
    }
    private void BrowseExam()
    {
        AnsiConsole.Clear();
        
        if (_examination.ExaminationSections == null || _examination.ExaminationSections.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]No sections found in this examination.[/]");
            AnsiConsole.MarkupLine("[green]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }
        var sectionNames = _examination.ExaminationSections
            .Select(s => s.Title)
            .ToArray();
        
        var selectedSection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a section to browse:")
                .PageSize(10)
                .AddChoices(sectionNames.Concat(new[] { "Back to Main Menu" })));
        
        if (selectedSection == "Back to Main Menu")
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
        
        AnsiConsole.MarkupLine($"[blue]== Section: {section.Title} ==[/]");
        if (!string.IsNullOrEmpty(section.Description))
        {
            AnsiConsole.MarkupLine($"[yellow]Description:[/] {section.Description}");
        }
        AnsiConsole.WriteLine();
        
        if (section.Questions == null || section.Questions.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]No questions found in this section.[/]");
            AnsiConsole.MarkupLine("[green]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }
        
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("ID")
            .AddColumn("Type")
            .AddColumn("Question")
            .AddColumn("Answered");
        
        for (int i = 0; i < section.Questions.Length; i++)
        {
            var question = section.Questions[i];
            table.AddRow(
                question.QuestionId ?? (i + 1).ToString(),
                question.Type.ToString(),
                question.Stem.Length > 50 ? question.Stem.Substring(0, 47) + "..." : question.Stem,
                (question.UserAnswer != null && question.UserAnswer.Length > 0) ? "[green]Yes[/]" : "[red]No[/]"
            );
        }
        
        AnsiConsole.Write(table);
        
        // View individual question
        var questionIndices = Enumerable.Range(1, section.Questions.Length)
            .Select(i => i.ToString())
            .ToArray();
        
        var selectedQuestion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a question to view (or 0 to go back):")
                .PageSize(10)
                .AddChoices(questionIndices.Concat(new[] { "0" })));
        
        if (selectedQuestion == "0")
        {
            return;
        }
        
        int questionIndex = int.Parse(selectedQuestion) - 1;
        DisplayQuestion(section.Questions[questionIndex]);
        
        AnsiConsole.MarkupLine("[green]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
    private void DisplayQuestion(Question question)
    {
        AnsiConsole.Clear();
        
        AnsiConsole.MarkupLine($"[blue]== Question {question.QuestionId} ==[/]");
        AnsiConsole.MarkupLine($"[yellow]Type:[/] {question.Type}");
        AnsiConsole.MarkupLine($"[yellow]Score:[/] {question.Score}");
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[green]Question:[/]");
        AnsiConsole.WriteLine(question.Stem);
        AnsiConsole.WriteLine();
        
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
        
        // Display user answer if available
        if (question.UserAnswer != null && question.UserAnswer.Length > 0)
        {
            AnsiConsole.MarkupLine("[green]Your Answer:[/]");
            foreach (var answer in question.UserAnswer)
            {
                AnsiConsole.WriteLine(answer);
            }
            AnsiConsole.WriteLine();
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Not answered yet[/]");
            AnsiConsole.WriteLine();
        }
    }
    private async Task AnswerQuestionsAsync()
    {
        if (_examination.ExaminationSections == null || _examination.ExaminationSections.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]No sections found in this examination.[/]");
            AnsiConsole.MarkupLine("[green]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }
        // Choose section
        var sectionNames = _examination.ExaminationSections
            .Select(s => s.Title)
            .ToArray();
        
        var selectedSection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a section to answer questions:")
                .PageSize(10)
                .AddChoices(sectionNames.Concat(new[] { "Back to Main Menu" })));
        
        if (selectedSection == "Back to Main Menu")
        {
            return;
        }
        
        var section = _examination.ExaminationSections
            .First(s => s.Title == selectedSection);
        
        if (section.Questions == null || section.Questions.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]No questions found in this section.[/]");
            AnsiConsole.MarkupLine("[green]Press any key to continue...[/]");
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
        if (question.ReferenceMaterials != null && question.ReferenceMaterials.Length > 0)
        {
            AnsiConsole.MarkupLine("[green]Reference Materials:[/]");
            foreach (var material in question.ReferenceMaterials)
            {
                AnsiConsole.WriteLine(material);
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
            AnsiConsole.MarkupLine("[green]Previous Answer:[/]");
            foreach (var answer in question.UserAnswer)
            {
                AnsiConsole.WriteLine(answer);
            }
            AnsiConsole.WriteLine();
            
            var modifyOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Would you like to modify your answer?")
                    .AddChoices(new[] { "Yes", "No", "Skip" }));
            
            if (modifyOption == "No" || modifyOption == "Skip")
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
        }
        
        AnsiConsole.MarkupLine("[green]Answer recorded! Press any key to continue...[/]");
        Console.ReadKey(true);
    }
    private string GetSingleChoiceAnswer(Question question)
    {
        if (question.Options == null || question.Options.Count == 0)
        {
            return AnsiConsole.Ask<string>("[yellow]Your answer:[/]");
        }
        
        var options = question.Options
            .Select(o => $"{o.Item1}) {o.Item2}")
            .ToArray();
        
        var selectedOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select your answer:[/]")
                .PageSize(10)
                .AddChoices(options));
        
        return selectedOption.Split(')')[0].Trim();
    }
    private string[] GetMultipleChoiceAnswers(Question question)
    {
        if (question.Options == null || question.Options.Count == 0)
        {
            var answer = AnsiConsole.Ask<string>("[yellow]Your answers (comma separated):[/]");
            return answer.Split(',').Select(s => s.Trim()).ToArray();
        }
        
        var options = question.Options
            .Select(o => $"{o.Item1}) {o.Item2}")
            .ToArray();
        
        var selectedOptions = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[yellow]Select your answers:[/]")
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
                .Title("[yellow]Your answer:[/]")
                .AddChoices(options));
        
        return selectedOption;
    }
    private string GetTextAnswer()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Your answer:[/]")
                .AllowEmpty());
    }
    private string GetEssayAnswer()
    {
        AnsiConsole.MarkupLine("[yellow]Enter your essay (press Enter on an empty line to finish):[/]");
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
        AnsiConsole.MarkupLine("[blue]Grading with AI...[/]");
        
        await AnsiConsole.Status()
            .StartAsync("Processing...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.Status("Submitting to AI for grading...");
                
                // Generate prompt
                var prompt = QuestionPromptTools.GetJsonGradingPrompt(question);
                
                // Get AI response
                var aiResponse = await AiTools.SendChatMessageAsync(_aiClient, _config, prompt);
                
                // Parse response
                var gradingResult = QuestionPromptTools.ParseAIResponse(aiResponse!);
                
                // Display result
                ctx.Status("AI grading complete!");
                
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[green]AI Grading Result:[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Markup(gradingResult.GenerateReport()));
                
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[yellow]Press any key to continue...[/]");
                Console.ReadKey(true);
            });
    }
    private void SaveExam()
    {
        AnsiConsole.Clear();
        
        var savePath = AnsiConsole.Ask<string>("[yellow]Save path (press Enter to use original path):[/]", _filePath);
        
        if (string.IsNullOrWhiteSpace(savePath))
        {
            savePath = _filePath;
        }
        
        bool success = ExaminationSerializer.SerializeToFile(_examination, savePath);
        
        if (success)
        {
            AnsiConsole.MarkupLine($"[green]Examination saved successfully to {savePath}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Failed to save examination.[/]");
        }
        
        AnsiConsole.MarkupLine("[yellow]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
    private void DisplayStatistics()
    {
        AnsiConsole.Clear();
        
        AnsiConsole.MarkupLine("[blue]== Examination Statistics ==[/]");
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
        
        table.AddRow("Total Questions", totalQuestions.ToString());
        table.AddRow("Answered Questions", answeredQuestions.ToString());
        table.AddRow("Completion Rate", $"{(totalQuestions > 0 ? (double)answeredQuestions / totalQuestions * 100 : 0):F1}%");
        
        AnsiConsole.Write(table);
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
}