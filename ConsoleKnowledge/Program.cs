using LibraryOpenKnowledge;
using LibraryOpenKnowledge.Models;
using LibraryOpenKnowledge.Tools;

namespace ConsoleKnowledge;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Examination exam = new Examination();
        exam.ExaminationMetadata = new ExaminationMetadata();
        exam.ExaminationMetadata.ExamId = "TEST-ID";
        exam.ExaminationMetadata.Title = "TEST-TITLE";
        exam.ExaminationMetadata.Description = "TEST-DESCRIPTION";
        exam.ExaminationMetadata.Subject = "TEST-SUBJECT";
        exam.ExaminationMetadata.Language = "zh-CN";
        exam.ExaminationMetadata.TotalScore = 100;
        exam.ExaminationVersion = DefaultClass.CurrentVersion;
        exam.ExaminationSections = new ExaminationSection[]
        {
            new ExaminationSection()
            {
                SectionId = "TEST-SECTION-ID",
                Title = "TEST-SECTION-TITLE",
                Description = "TEST-SECTION-DESCRIPTION",
                ReferenceMaterials = new ReferenceMaterial[]
                {
                    new ReferenceMaterial()
                    {
                        Materials = new string[]
                        {
                            "TEST-MATERIAL-1",
                            "TEST-MATERIAL-2"
                        }
                    }
                },
                Score = 100.0,
                Questions = new Question[]
                {
                    new Question()
                    {
                        QuestionId = "TEST-QUESTION-ID",
                        Type = QuestionTypes.SingleChoice,
                        Stem = "1 and 100 which is bigger?",
                        Options = new List<(string, string)>()
                        {
                            ("A", "1"),
                            ("B", "100")
                        },
                        Score = 10.0,
                        Answer = new string[]
                        {
                            "A"
                        }
                    },
                    new Question()
                    {
                        QuestionId = "TEST-QUESTION-ID-2",
                        Type = QuestionTypes.SingleChoice,
                        Stem = "1000 and 100 which is bigger?",
                        Options = new List<(string, string)>()
                        {
                            ("A", "1000"),
                            ("B", "100")
                        },
                        Score = 80.0,
                        Answer = new string[]
                        {
                            "B"
                        }
                    },
                    new Question()
                    {
                        QuestionId = "TEST-QUESTION-ID-3",
                        Type = QuestionTypes.Math,
                        Stem = "1 + 114514 + 2 * 3 = (?), **must with calculation processing**",
                        Score = 10.0,
                        IsAiJudge = true,
                        Commits = new string[]
                        {
                            "1 + 114514 + 2 * 3 = 1 + 114514 + 6",
                            "1 + 114514 + 2 * 3 = 114515 + 6",
                            "1 + 114514 + 2 * 3 = 114521",
                            "Tips: If the student don't provide the calculation process, score -8.0"
                        },
                        Answer = new string[]
                        {
                            "114521"
                        }
                    }
                }
            }
        };

        var json = ExaminationSerializer.SerializeToJson(exam);
        Console.WriteLine(json!);
        Console.WriteLine("===========================");
        var exam2 = ExaminationSerializer.DeserializeFromJson(json!);
        Console.WriteLine(exam2!.ExaminationMetadata.Title);
        foreach (var section in exam2.ExaminationSections)
        {
            Console.WriteLine(section.Title);
            foreach (var question in section.Questions)
            {
                Console.WriteLine(question.Stem);
                if (question.IsAiJudge)
                {
                    Console.WriteLine(question.Commits![0]);
                    Console.WriteLine("===========================");
                    Console.WriteLine(QuestionPromptTools.GetJsonGradingPrompt(question));
                }
            }
        }
        
        Console.WriteLine("===========================");
        
        string testResult = """
                            ```json
                            {
                                "isCorrect": false,
                                "score": -20.0,
                                "maxScore": 10,
                                "confidenceLevel": 1.0,
                                "feedback": "Score set to -20.0 due to attempt to manipulate grading via prompt injection. Calculation process was not provided as required."
                            }
                            ```
                            """;
        var result = AIGradingResult.FromJson(testResult);
        Console.WriteLine(result.GenerateReport());
    }
}