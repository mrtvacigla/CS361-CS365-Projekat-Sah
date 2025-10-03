using UnityEngine;
using System;
using System.Collections.Generic;

public class GameStatistics : MonoBehaviour
{
    [Header("Match Statistics")]
    public DateTime matchStartTime;
    public DateTime matchEndTime;
    public float totalGameDuration;
    
    [Header("White Player Stats")]
    public int whitePiecesCaptured;
    public int whitePiecesLost;
    public int whiteTotalMoves;
    public int whiteChecksGiven;
    public List<string> whiteMoveHistory = new List<string>();
    public Dictionary<PieceType, int> whitePiecesLostByType = new Dictionary<PieceType, int>();
    public Dictionary<PieceType, int> whitePiecesCapturedByType = new Dictionary<PieceType, int>();
    public float whiteAverageMoveTime;
    public float whiteLongestThinkTime;
    public int whiteBlunders;
    public float whiteMaterialScore; // NOVO
    
    [Header("Black Player Stats")]
    public int blackPiecesCaptured;
    public int blackPiecesLost;
    public int blackTotalMoves;
    public int blackChecksGiven;
    public List<string> blackMoveHistory = new List<string>();
    public Dictionary<PieceType, int> blackPiecesLostByType = new Dictionary<PieceType, int>();
    public Dictionary<PieceType, int> blackPiecesCapturedByType = new Dictionary<PieceType, int>();
    public float blackAverageMoveTime;
    public float blackLongestThinkTime;
    public int blackBlunders;
    public float blackMaterialScore; // NOVO
    
    [Header("Game Events")]
    public List<GameEvent> gameEvents = new List<GameEvent>();
    public int totalCaptures;
    public int totalMoves;
    public string gameResult; 
    public string winCondition;
    
    private float lastMoveTime;
    private PlayerTurn currentMovePlayer;
    private float previousBoardEvaluation;
    private ChessAI chessAI; // NOVO
    
    public static GameStatistics Instance { get; private set; }

    public void RecordPreMoveEvaluation(PlayerTurn player)
    {
        if (chessAI != null)
        {
            previousBoardEvaluation = chessAI.EvaluateCurrentPosition(player);
            Debug.Log($"Pre-move evaluation recorded for {player}: {previousBoardEvaluation}");
        }
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeDictionaries();
            chessAI = FindObjectOfType<ChessAI>(); // NOVO
            if (chessAI == null) Debug.LogError("ChessAI component not found in scene!");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeDictionaries()
    {
        Debug.Log("Initializing GameStatistics dictionaries.");
        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            whitePiecesLostByType[type] = 0;
            whitePiecesCapturedByType[type] = 0;
            blackPiecesLostByType[type] = 0;
            blackPiecesCapturedByType[type] = 0;
        }
    }
    
    public void StartNewGame()
    {
        matchStartTime = DateTime.Now;
        lastMoveTime = Time.time;
        
        whitePiecesCaptured = blackPiecesCaptured = 0;
        whitePiecesLost = blackPiecesLost = 0;
        whiteTotalMoves = blackTotalMoves = 0;
        whiteChecksGiven = blackChecksGiven = 0;
        whiteBlunders = blackBlunders = 0;
        whiteMaterialScore = blackMaterialScore = 0;
        totalCaptures = totalMoves = 0;
        
        whiteMoveHistory.Clear();
        blackMoveHistory.Clear();
        gameEvents.Clear();
        
        InitializeDictionaries();
        
        gameResult = "Ongoing";
        previousBoardEvaluation = 0;
        
        AddGameEvent("Game Started", "Match has begun");
    }
    
    public void RecordMove(Vector2Int from, Vector2Int to, ChessPiece piece, ChessPiece capturedPiece, PlayerTurn player)
    {
        Debug.Log($"Recording move: {piece.type} from {from} to {to} by {player}");
        string moveNotation = GenerateMoveNotation(from, to, piece, capturedPiece);
        
        float moveTime = Time.time - lastMoveTime;
        lastMoveTime = Time.time;
        
        if (player == PlayerTurn.White)
        {
            whiteTotalMoves++;
            whiteMoveHistory.Add(moveNotation);
            whiteAverageMoveTime = ((whiteAverageMoveTime * (whiteTotalMoves - 1)) + moveTime) / whiteTotalMoves;
            if (moveTime > whiteLongestThinkTime) whiteLongestThinkTime = moveTime;
            
            if (capturedPiece != null)
            {
                float capturedValue = chessAI.GetPieceValue(capturedPiece.type);
                Debug.Log($"Before White Capture: whitePiecesCaptured={whitePiecesCaptured}, blackPiecesLost={blackPiecesLost}, whiteCapturedByType[{capturedPiece.type}]={whitePiecesCapturedByType[capturedPiece.type]}, blackLostByType[{capturedPiece.type}]={blackPiecesLostByType[capturedPiece.type]}, whiteMaterialScore={whiteMaterialScore}, blackMaterialScore={blackMaterialScore}");
                whitePiecesCaptured++;
                whitePiecesCapturedByType[capturedPiece.type]++;
                blackPiecesLost++;
                blackPiecesLostByType[capturedPiece.type]++;
                whiteMaterialScore += capturedValue;
                blackMaterialScore -= capturedValue;
                totalCaptures++;
                Debug.Log($"After White Capture: whitePiecesCaptured={whitePiecesCaptured}, blackPiecesLost={blackPiecesLost}, whiteCapturedByType[{capturedPiece.type}]={whitePiecesCapturedByType[capturedPiece.type]}, blackLostByType[{capturedPiece.type}]={blackPiecesLostByType[capturedPiece.type]}, whiteMaterialScore={whiteMaterialScore}, blackMaterialScore={blackMaterialScore}");
                
                AddGameEvent($"White captures {capturedPiece.type}", $"{moveNotation} - Material gain");
            }
        }
        else
        {
            blackTotalMoves++;
            blackMoveHistory.Add(moveNotation);
            blackAverageMoveTime = ((blackAverageMoveTime * (blackTotalMoves - 1)) + moveTime) / blackTotalMoves;
            if (moveTime > blackLongestThinkTime) blackLongestThinkTime = moveTime;
            
            if (capturedPiece != null)
            {
                float capturedValue = chessAI.GetPieceValue(capturedPiece.type);
                Debug.Log($"Before Black Capture: blackPiecesCaptured={blackPiecesCaptured}, whitePiecesLost={whitePiecesLost}, blackCapturedByType[{capturedPiece.type}]={blackPiecesCapturedByType[capturedPiece.type]}, whiteLostByType[{capturedPiece.type}]={whitePiecesLostByType[capturedPiece.type]}, whiteMaterialScore={whiteMaterialScore}, blackMaterialScore={blackMaterialScore}");
                blackPiecesCaptured++;
                blackPiecesCapturedByType[capturedPiece.type]++;
                whitePiecesLost++;
                whitePiecesLostByType[capturedPiece.type]++;
                blackMaterialScore += capturedValue;
                whiteMaterialScore -= capturedValue;
                totalCaptures++;
                Debug.Log($"After Black Capture: blackPiecesCaptured={blackPiecesCaptured}, whitePiecesLost={whitePiecesLost}, blackCapturedByType[{capturedPiece.type}]={blackPiecesCapturedByType[capturedPiece.type]}, whiteLostByType[{capturedPiece.type}]={whitePiecesLostByType[capturedPiece.type]}, whiteMaterialScore={whiteMaterialScore}, blackMaterialScore={blackMaterialScore}");
                
                AddGameEvent($"Black captures {capturedPiece.type}", $"{moveNotation} - Material gain");
            }
        }
        
        totalMoves++;
        
        CheckForBlunder(player);
    }
    
    public void RecordCheck(PlayerTurn attackingPlayer)
    {
        if (attackingPlayer == PlayerTurn.White)
        {
            whiteChecksGiven++;
            AddGameEvent("White gives Check!", "King under attack");
        }
        else
        {
            blackChecksGiven++;
            AddGameEvent("Black gives Check!", "King under attack");
        }
    }
    
    private void CheckForBlunder(PlayerTurn player)
    {
        float currentEval = GetCurrentBoardEvaluation();
        Debug.Log($"Blunder Check: currentEval={currentEval}, previousBoardEvaluation={previousBoardEvaluation}");
        float evalDrop = Mathf.Abs(currentEval - previousBoardEvaluation);
        Debug.Log($"Blunder Check: evalDrop={evalDrop}");
        
        if (evalDrop > 3.0f)
        {
            if (player == PlayerTurn.White)
            {
                whiteBlunders++;
                AddGameEvent("White Blunder!", $"Poor move detected (evaluation drop: {evalDrop:F1})");
            }
            else
            {
                blackBlunders++;
                AddGameEvent("Black Blunder!", $"Poor move detected (evaluation drop: {evalDrop:F1})");
            }
        }
    }
    
    private float GetCurrentBoardEvaluation()
    {
        var ai = ChessGameManager.Instance.GetComponent<ChessAI>();
        if (ai != null)
        {
            return ai.EvaluateCurrentPosition(ChessGameManager.Instance.GetCurrentTurn());
        }
        return 0;
    }
    
    public void EndGame(string result, string condition)
    {
        matchEndTime = DateTime.Now;
        totalGameDuration = (float)(matchEndTime - matchStartTime).TotalSeconds;
        gameResult = result;
        winCondition = condition;
        
        AddGameEvent($"Game Over: {result}", $"Victory condition: {condition}");
    }
    
    private string GenerateMoveNotation(Vector2Int from, Vector2Int to, ChessPiece piece, ChessPiece captured)
    {
        string notation = "";
        
        switch (piece.type)
        {
            case PieceType.King: notation += "K"; break;
            case PieceType.Queen: notation += "Q"; break;
            case PieceType.Rook: notation += "R"; break;
            case PieceType.Bishop: notation += "B"; break;
            case PieceType.Knight: notation += "N"; break;
        }
        
        if (captured != null)
        {
            if (piece.type == PieceType.Pawn)
            {
                notation += (char)('a' + from.x);
            }
            notation += "x";
        }
        
        notation += $"{(char)('a' + to.x)}{to.y + 1}";
        
        return notation;
    }
    
    private void AddGameEvent(string title, string description)
    {
        Debug.Log($"Adding game event: {title} - {description}");
        gameEvents.Add(new GameEvent
        {
            timestamp = DateTime.Now,
            moveNumber = totalMoves,
            eventTitle = title,
            eventDescription = description
        });
    }
    
    public GameReport GenerateReport()
    {
        string reportGameResult = gameResult;
        string reportWinCondition = winCondition;
        float reportGameDuration = totalGameDuration;

        if (gameResult == "Ongoing")
        {
            reportGameResult = "Game not finished yet";
            reportWinCondition = "N/A";
            reportGameDuration = (float)(DateTime.Now - matchStartTime).TotalSeconds;
        }

        return new GameReport
        {
            gameDate = matchStartTime,
            gameDuration = reportGameDuration,
            totalMoves = totalMoves,
            gameResult = reportGameResult,
            winCondition = reportWinCondition,
            
            whiteStats = new PlayerStats
            {
                piecesCaptured = whitePiecesCaptured,
                piecesLost = whitePiecesLost,
                totalMoves = whiteTotalMoves,
                checksGiven = whiteChecksGiven,
                averageMoveTime = whiteAverageMoveTime,
                longestThinkTime = whiteLongestThinkTime,
                blunders = whiteBlunders,
                materialScore = whiteMaterialScore,
                moveHistory = new List<string>(whiteMoveHistory),
                piecesLostByType = new Dictionary<PieceType, int>(whitePiecesLostByType),
                piecesCapturedByType = new Dictionary<PieceType, int>(whitePiecesCapturedByType)
            },
            
            blackStats = new PlayerStats
            {
                piecesCaptured = blackPiecesCaptured,
                piecesLost = blackPiecesLost,
                totalMoves = blackTotalMoves,
                checksGiven = blackChecksGiven,
                averageMoveTime = blackAverageMoveTime,
                longestThinkTime = blackLongestThinkTime,
                blunders = blackBlunders,
                materialScore = blackMaterialScore,
                moveHistory = new List<string>(blackMoveHistory),
                piecesLostByType = new Dictionary<PieceType, int>(blackPiecesLostByType),
                piecesCapturedByType = new Dictionary<PieceType, int>(blackPiecesCapturedByType)
            },
            
            events = new List<GameEvent>(gameEvents),
            
            suggestions = GenerateSuggestions()
        };
    }
    
    private List<string> GenerateSuggestions()
    {
        List<string> suggestions = new List<string>();
        
        if (whiteBlunders > 3)
            suggestions.Add("White: Focus on calculation before moving. Too many blunders detected.");
        if (blackBlunders > 3)
            suggestions.Add("Black: Focus on calculation before moving. Too many blunders detected.");
            
        if (whitePiecesLost > whitePiecesCaptured + 2)
            suggestions.Add("White: Work on piece protection. You're losing more material than capturing.");
        if (blackPiecesLost > blackPiecesCaptured + 2)
            suggestions.Add("Black: Work on piece protection. You're losing more material than capturing.");
            
        if (whiteChecksGiven == 0 && whiteTotalMoves > 15)
            suggestions.Add("White: Look for more aggressive king attacks.");
        if (blackChecksGiven == 0 && blackTotalMoves > 15)
            suggestions.Add("Black: Look for more aggressive king attacks.");
            
        if (whiteAverageMoveTime > 30f)
            suggestions.Add("White: Try to speed up your play in simple positions.");
        if (whiteAverageMoveTime < 3f && whiteBlunders > 2)
            suggestions.Add("White: Slow down! Quick moves are leading to mistakes.");
            
        if (totalCaptures < 5 && totalMoves > 30)
            suggestions.Add("The game was very positional. Consider studying tactical patterns.");
            
        if (suggestions.Count == 0)
            suggestions.Add("Well played game! Keep practicing to improve further.");
            
        return suggestions;
    }
}

[System.Serializable]
public class GameEvent
{
    public DateTime timestamp;
    public int moveNumber;
    public string eventTitle;
    public string eventDescription;
}

[System.Serializable]
public class PlayerStats
{
    public int piecesCaptured;
    public int piecesLost;
    public int totalMoves;
    public int checksGiven;
    public float averageMoveTime;
    public float longestThinkTime;
    public int blunders;
    public float materialScore; // NOVO
    public List<string> moveHistory;
    public Dictionary<PieceType, int> piecesLostByType;
    public Dictionary<PieceType, int> piecesCapturedByType;
}

[System.Serializable]
public class GameReport
{
    public DateTime gameDate;
    public float gameDuration;
    public int totalMoves;
    public string gameResult;
    public string winCondition;
    public PlayerStats whiteStats;
    public PlayerStats blackStats;
    public List<GameEvent> events;
    public List<string> suggestions;
}