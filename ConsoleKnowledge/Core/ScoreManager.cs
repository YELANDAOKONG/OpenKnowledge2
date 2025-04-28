using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibraryOpenKnowledge.Models;
using Newtonsoft.Json;

namespace ConsoleKnowledge.Core;

public class ScoreManager
{
    private static ScoreManager? _instance;
    private static readonly object LockObject = new object();
    private string _scoreDirectory;
    
    public static ScoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (LockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new ScoreManager();
                    }
                }
            }
            return _instance;
        }
    }
    
    private ScoreManager()
    {
        string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        _scoreDirectory = Path.Combine(configDir, "open-knowledge", "scores");
        
        // Create directory if it doesn't exist
        if (!Directory.Exists(_scoreDirectory))
        {
            Directory.CreateDirectory(_scoreDirectory);
        }
    }
    
    public bool SaveScore(ScoreRecord score)
    {
        try
        {
            string filePath = Path.Combine(_scoreDirectory, $"{score.Id}.json");
            string json = JsonConvert.SerializeObject(score, Formatting.Indented);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public ScoreRecord? GetScore(string id)
    {
        try
        {
            string filePath = Path.Combine(_scoreDirectory, $"{id}.json");
            if (!File.Exists(filePath))
                return null;
                
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<ScoreRecord>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    public List<ScoreRecord> GetAllScores()
    {
        var scores = new List<ScoreRecord>();
        
        try
        {
            foreach (var file in Directory.GetFiles(_scoreDirectory, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var score = JsonConvert.DeserializeObject<ScoreRecord>(json);
                    if (score != null)
                    {
                        scores.Add(score);
                    }
                }
                catch (Exception)
                {
                    // Skip files that can't be parsed
                }
            }
        }
        catch (Exception)
        {
            // Return whatever scores were successfully loaded
        }
        
        return scores;
    }
    
    public List<ScoreRecord> GetScoresByUser(string userId)
    {
        return GetAllScores().Where(s => s.UserId == userId).ToList();
    }
    
    public List<ScoreRecord> GetScoresByExam(string examId)
    {
        return GetAllScores().Where(s => s.ExamId == examId).ToList();
    }
    
    public bool DeleteScore(string id)
    {
        try
        {
            string filePath = Path.Combine(_scoreDirectory, $"{id}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public ScoreRecord CreateScoreFromExamination(Examination examination, string userId = "", string userName = "")
    {
        var score = new ScoreRecord
        {
            ExamId = examination.ExaminationMetadata.ExamId ?? string.Empty,
            ExamTitle = examination.ExaminationMetadata.Title,
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.Now
        };
        
        score.CalculateScores(examination);
        return score;
    }
}
