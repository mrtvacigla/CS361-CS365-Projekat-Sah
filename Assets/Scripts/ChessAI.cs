using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum AIState
{
    Idle,
    Analyzing,
    Planning,
    Executing
}

public class ChessAI : MonoBehaviour
{
    [Header("AI Settings")]
    public PieceColor aiColor = PieceColor.Black; // NOVO
    
    private ChessBoard board;
    private AgentCommunication communication;
    private GameStatistics gameStats; // NOVO
    private ChessGameManager gameManager; // NOVO
    public ChessAIOld oldAI; // NOVO
    
    private void Awake()
    {
        StartCoroutine(WaitForLoading());
    }

    public IEnumerator WaitForLoading()
    {
        yield return new WaitForSeconds(0.5f);
        board = ChessGameManager.Instance.GetBoard();
        communication = GetComponent<AgentCommunication>();
        gameStats = GetComponent<GameStatistics>(); // NOVO
        gameManager = ChessGameManager.Instance; // NOVO

        // Retrieve AI difficulty from PlayerPrefs and set searchDepth
        int difficulty = PlayerPrefs.GetInt("AIDifficulty", 1); // Default to Medium (index 1)
        
    }
    
    public IEnumerator MakeAIMove()
    {
        yield return oldAI.MakeAIMoveOld();
    }
   
    
    // NOVO - javna metoda za evaluaciju trenutne pozicije
    public float EvaluateCurrentPosition(PlayerTurn playerTurn)
    {
        PieceColor currentPlayerColor = playerTurn == PlayerTurn.White ? PieceColor.White : PieceColor.Black;
        return EvaluatePosition(currentPlayerColor);
    }
    
    private float EvaluatePosition(PieceColor perspectiveColor)
    {
        // Check for game over conditions first
        if (board.IsCheckmate(perspectiveColor))
        {
            return 100000f; // Very high score for checkmating the opponent
        }
        if (board.IsCheckmate(perspectiveColor == PieceColor.White ? PieceColor.Black : PieceColor.White))
        {
            return -100000f; // Very low score if AI is checkmated
        }
        if (board.IsStalemate(perspectiveColor))
        {
            return -50000f; // Significant penalty for stalemate
        }

        float score = 0;

        // Penalty for king in check
        if (board.IsInCheck(perspectiveColor))
        {
            score -= 50f;
        }
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board.GetPiece(new Vector2Int(x, y));
                if (piece != null)
                {
                    float pieceValue = GetPieceValue(piece.type);
                    if (piece.color == perspectiveColor)
                        score += pieceValue;
                    else
                        score -= pieceValue;
                }
            }
        }
        
        // Mobility factor
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board.GetPiece(new Vector2Int(x, y));
                if (piece != null && piece.color == perspectiveColor)
                {
                    score += board.GetValidMoves(piece).Count * 0.1f; // Small bonus for each valid move
                }
            }
        }

        score += EvaluatePositionalFactors(perspectiveColor);
        score += EvaluateKingSafety(perspectiveColor); // Add king safety score
        
        return score;
    }
    
    public float GetPieceValue(PieceType type)
    {
        return type switch
        {
            PieceType.Pawn => 1.0f,
            PieceType.Knight => 3.0f,
            PieceType.Bishop => 3.0f,
            PieceType.Rook => 5.0f,
            PieceType.Queen => 9.0f,
            PieceType.King => 1000.0f,
            _ => 0
        };
    }
    

    private float EvaluateKingSafety(PieceColor color)
    {
        float kingSafetyScore = 0;
        Vector2Int kingPosition = Vector2Int.zero;

        // Find king's position
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board.GetPiece(new Vector2Int(x, y));
                if (piece != null && piece.type == PieceType.King && piece.color == color)
                {
                    kingPosition = piece.position;
                    break;
                }
            }
        }

        // Check for pawn shield in front of the king
        int pawnDirection = (color == PieceColor.White) ? 1 : -1;
        int frontRow = kingPosition.y + pawnDirection;

        // Check squares in front of the king (left, center, right)
        for (int dx = -1; dx <= 1; dx++)
        {
            Vector2Int pawnShieldPos = new Vector2Int(kingPosition.x + dx, frontRow);
            if (board.IsValidPosition(pawnShieldPos))
            {
                var piece = board.GetPiece(pawnShieldPos);
                if (piece != null && piece.type == PieceType.Pawn && piece.color == color)
                {
                    kingSafetyScore += 0.5f; // Bonus for pawn in front of king
                }
                else
                {
                    kingSafetyScore -= 0.2f; // Penalty for missing pawn in front of king
                }
            }
        }

        return kingSafetyScore;
    }

    private float EvaluatePositionalFactors(PieceColor perspectiveColor)
    {
        float score = 0;
        
        Vector2Int[] centerSquares = {
            new Vector2Int(3, 3), new Vector2Int(3, 4),
            new Vector2Int(4, 3), new Vector2Int(4, 4)
        };
        
        foreach (var square in centerSquares)
        {
            var piece = board.GetPiece(square);
            if (piece != null)
            {
                // MODIFIKOVANO - Evaluacija na osnovu perspectiveColor
                if (piece.color == perspectiveColor)
                    score += 0.3f;
                else
                    score -= 0.3f;
            }
        }
        
        return score;
    }
    
    private List<ChessMove> GetAllPossibleMoves(PieceColor color)
    {
        var moves = new List<ChessMove>();
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board.GetPiece(new Vector2Int(x, y));
                if (piece != null && piece.color == color)
                {
                    var validMoves = board.GetValidMoves(piece);
                    foreach (var move in validMoves)
                    {
                        moves.Add(new ChessMove { from = piece.position, to = move });
                    }
                }
            }
        }
        
        return moves;
    }
    
    private bool IsGameOver()
    {
        return board.IsCheckmate(PieceColor.White) || board.IsCheckmate(PieceColor.Black);
    }
    
    public IEnumerator ExecuteAIMove(ChessMove move)
    {
        var piece = board.GetPiece(move.from);
        if (piece == null) yield break;
    
        var pieceAgent = piece.GetComponent<ChessPieceAgent>();
        pieceAgent.SetState(new MovingState());
    
        var pathfinder = piece.GetComponent<ChessPathfinder>();
        var path = pathfinder.FindPath(move.from, move.to);
    
        var capturedPiece = board.GetPiece(move.to);
        if (capturedPiece != null)
        {
            communication.BroadcastThreat(move.to, aiColor); // MODIFIKOVANO
            yield return StartCoroutine(AnimateCapture(capturedPiece));
        }
    
        var steeringBehavior = piece.GetComponent<SteeringBehavior>();
        foreach (var waypoint in path)
        {
            Vector3 worldPos = ChessGameManager.Instance.GetComponent<ChessUIManager>().GetWorldPosition(waypoint);
            yield return StartCoroutine(steeringBehavior.MoveTo(worldPos));
        }
    
        Vector2Int fromPosition = piece.position;
        board.MovePiece(move.from, move.to);
        piece.position = move.to;
    
        var uiManager = ChessGameManager.Instance.GetComponent<ChessUIManager>();
        uiManager.MovePieceVisually(piece, move.to);
    
        // NOVO - zabeleži AI potez u statistici
        if (gameStats != null)
        {
            gameStats.RecordMove(fromPosition, move.to, piece, capturedPiece, aiColor == PieceColor.White ? PlayerTurn.White : PlayerTurn.Black); // MODIFIKOVANO
        }
    
        pieceAgent.SetState(new IdleState());
    }
    
    private IEnumerator AnimateCapture(ChessPiece piece)
    {
        var agent = piece.GetComponent<ChessPieceAgent>();
        agent.SetState(new CapturedState());
    
        var uiManager = ChessGameManager.Instance.GetComponent<ChessUIManager>();
        yield return StartCoroutine(uiManager.FadeOutPiece(piece));
    
        board.SetPiece(piece.position, null);
        uiManager.RemovePieceVisually(piece);
    }
}

public class ChessMove
{
    public Vector2Int from;
    public Vector2Int to;
}