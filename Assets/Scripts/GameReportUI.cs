using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameReportUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject reportPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI gameInfoText;
    public TextMeshProUGUI whiteStatsText;
    public TextMeshProUGUI blackStatsText;
    public TextMeshProUGUI suggestionsText;
    public ScrollRect eventsScrollView;
    public GameObject eventItemPrefab;
    public Transform eventsContent;
    public Button closeButton;
    public Button openButton;
    public Button saveReportButton;
    
    [Header("Visual Settings")]
    public Color winColor = Color.green;
    public Color loseColor = Color.red;
    public Color drawColor = Color.yellow;
    
    private void Start()
    {
        reportPanel.SetActive(false);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseReport);
            
        if (saveReportButton != null)
            saveReportButton.onClick.AddListener(SaveReportToFile);
            
        if (openButton != null)
            openButton.onClick.AddListener(() => ShowReport(GameStatistics.Instance.GenerateReport()));
    }
    
    public void ShowReport(GameReport report)
    {
        reportPanel.SetActive(true);
        PopulateReport(report);
    }
    
    private void PopulateReport(GameReport report)
    {
        // Title
        titleText.text = $"GAME REPORT - {report.gameResult}";
        titleText.color = GetResultColor(report.gameResult);
        
        // Game Info
        int minutes = Mathf.FloorToInt(report.gameDuration / 60f);
        int seconds = Mathf.FloorToInt(report.gameDuration % 60f);
        
        gameInfoText.text = $"<b>Match Details</b>\n" +
                           $"Date: {report.gameDate:dd/MM/yyyy HH:mm}\n" +
                           $"Duration: {minutes}m {seconds}s\n" +
                           $"Total Moves: {report.totalMoves}\n" +
                           $"Result: {report.gameResult}\n" +
                           $"Condition: {report.winCondition}";
        
        // White Stats
        whiteStatsText.text = GeneratePlayerStatsText("WHITE PLAYER", report.whiteStats);
        
        // Black Stats
        blackStatsText.text = GeneratePlayerStatsText("BLACK PLAYER", report.blackStats);
        
        // Prikazuj suggestions
        if (suggestionsText != null)
            suggestionsText.gameObject.SetActive(true);
        suggestionsText.text = string.Join("\n", report.suggestions);
        
        // Events (optional - može biti dosta)
        PopulateEvents(report.events);
    }
    
    private string GeneratePlayerStatsText(string playerName, PlayerStats stats)
    {
        Debug.Log($"Generating stats for {playerName}: Captured={stats.piecesCaptured}, Lost={stats.piecesLost}");
        string text = $"<b>{playerName}</b>\n\n";
        
        // Osnovne stats
        text += $"<b>Material</b>\n";
        text += $"Pieces Captured: {stats.piecesCaptured}\n";
        text += $"Pieces Lost: {stats.piecesLost}\n";
        text += $"Net Material: {(stats.materialScore > 0 ? "+" : "")}{stats.materialScore:F1}\n\n";
        
        // Breakdown po tipu figure
        text += $"<b>Captures by Type</b>\n";
        foreach (var kvp in stats.piecesCapturedByType)
        {
            Debug.Log($"{playerName} Captured by Type: {kvp.Key}: {kvp.Value}");
            if (kvp.Value > 0)
                text += $"{kvp.Key}: {kvp.Value}  ";
        }
        text += "\n\n";
        
        text += $"<b>Losses by Type</b>\n";
        foreach (var kvp in stats.piecesLostByType)
        {
            Debug.Log($"{playerName} Lost by Type: {kvp.Key}: {kvp.Value}");
            if (kvp.Value > 0)
                text += $"{kvp.Key}: {kvp.Value}  ";
        }
        text += "\n\n";
        
        // Gameplay stats
        text += $"<b>Gameplay</b>\n";
        text += $"Total Moves: {stats.totalMoves}\n";
        text += $"Checks Given: {stats.checksGiven}\n";
        text += $"Blunders: {stats.blunders}\n\n";
        
        // Timing
        text += $"<b>Timing</b>\n";
        text += $"Avg Move Time: {stats.averageMoveTime:F1}s\n";
        text += $"Longest Think: {stats.longestThinkTime:F1}s\n";
        
        return text;
    }
    
    private void PopulateEvents(List<GameEvent> events)
    {
        // Clear existing events
        foreach (Transform child in eventsContent)
        {
            Destroy(child.gameObject);
        }
        
        // Prikazuj samo značajne događaje (captures, checks, blunders)
        foreach (var gameEvent in events)
        {
            if (IsSignificantEvent(gameEvent))
            {
                CreateEventItem(gameEvent);
            }
        }
    }
    
    private bool IsSignificantEvent(GameEvent ev)
    {
        return ev.eventTitle.Contains("capture") || 
               ev.eventTitle.Contains("Check") || 
               ev.eventTitle.Contains("Blunder") ||
               ev.eventTitle.Contains("Game");
    }
    
    private void CreateEventItem(GameEvent gameEvent)
    {
        GameObject eventObj = Instantiate(eventItemPrefab, eventsContent);
        
        TextMeshProUGUI eventText = eventObj.GetComponentInChildren<TextMeshProUGUI>();
        if (eventText != null)
        {
            eventText.text = $"<b>Move {gameEvent.moveNumber}:</b> {gameEvent.eventTitle}\n" +
                           $"<size=80%>{gameEvent.eventDescription}</size>";
        }
    }
    
    private Color GetResultColor(string result)
    {
        if (result.Contains("White wins"))
            return winColor;
        else if (result.Contains("Black wins"))
            return loseColor;
        else if (result.Contains("Draw"))
            return drawColor;
        else
            return Color.white;
    }
    
    public void CloseReport()
    {
        reportPanel.SetActive(false);
    }
    
    private void SaveReportToFile()
    {
        var report = GameStatistics.Instance.GenerateReport();
        string reportText = GenerateTextReport(report);
        
        string path = Application.persistentDataPath + $"/ChessReport_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
        System.IO.File.WriteAllText(path, reportText);
        
        Debug.Log($"Report saved to: {path}");
        statusText.text = "Report saved successfully!";
    }
    
    private string GenerateTextReport(GameReport report)
    {
        string text = "========================================\n";
        text += "         CHESS GAME REPORT\n";
        text += "========================================\n\n";
        
        text += $"Date: {report.gameDate:dd/MM/yyyy HH:mm}\n";
        text += $"Duration: {Mathf.Floor(report.gameDuration / 60)}m {Mathf.Floor(report.gameDuration % 60)}s\n";
        text += $"Result: {report.gameResult}\n";
        text += $"Condition: {report.winCondition}\n\n";
        
        text += "========================================\n";
        text += "           WHITE PLAYER\n";
        text += "========================================\n";
        text += GeneratePlayerStatsText("", report.whiteStats) + "\n";
        
        text += "========================================\n";
        text += "           BLACK PLAYER\n";
        text += "========================================\n";
        text += GeneratePlayerStatsText("", report.blackStats) + "\n";
        
        text += "========================================\n";
        text += "        ANALYSIS & SUGGESTIONS\n";
        text += "========================================\n";
        foreach (var suggestion in report.suggestions)
        {
            text += $"• {suggestion}\n";
        }
        
        return text;
    }
    
    [Header("Debug")]
    public TextMeshProUGUI statusText;
}