using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ChessAIOld : MonoBehaviour
{
    [Header("AI Settings")]
    public AIState currentState = AIState.Idle;
    public int searchDepth = 3;
    public float thinkingTime = 2.0f;

    private PieceColor playingColor;
    
    private ChessBoard board;
    private AgentCommunication communication;
    public GameStatistics gameStats;
    public ChessAI newai;
    
    private void Start()
    {
        board = ChessGameManager.Instance.GetBoard();
        communication = GetComponent<AgentCommunication>();

        // Retrieve AI difficulty from PlayerPrefs and set searchDepth
        int difficulty = PlayerPrefs.GetInt("AIDifficulty", 1); // Default to Medium (index 1)
        int playcolorint = PlayerPrefs.GetInt("PlayerIsBlack");
        playingColor = playcolorint == 1 ? PieceColor.White : PieceColor.Black;
        searchDepth = difficulty switch
        {
            0 => 1, // Easy
            1 => 2, // Medium
            2 => 3, // Hard
            _ => 2  // Default to Medium
        };
    }
    
    public ChessMove GetBestMoveForAI()
    {
        return GetBestMoveOld();
    }
    public IEnumerator MakeAIMoveOld()
    {
        SetState(AIState.Analyzing);
        
        // Simulate thinking time
        yield return new WaitForSeconds(thinkingTime);
        
        SetState(AIState.Planning);
        
        // Get best move using minimax algorithm
        var bestMove = GetBestMoveOld();
        
        if (bestMove != null) 
        {
            SetState(AIState.Executing);
            yield return StartCoroutine(ExecuteAIMoveOld(bestMove));
        }
        
        SetState(AIState.Idle);
        ChessGameManager.Instance.OnAIMoveComplete();
    }
    
    public void SetState(AIState newState)
    {
        currentState = newState;
        
        string stateMessage = newState switch
        {
            AIState.Analyzing => "AI is analyzing the position...",
            AIState.Planning => "AI is planning the best move...",
            AIState.Executing => "AI is executing move...",
            _ => "AI thinking..."
        };
        
        ChessGameManager.Instance.statusText.text = stateMessage;
    }
    
    public ChessMove GetBestMoveOld()
    {
        var possibleMoves = GetAllPossibleMovesOld(playingColor);
        
        if (possibleMoves.Count == 0)
            return null;
        
        ChessMove bestMove = null;
        float bestScore = float.MinValue;
        
        foreach (var move in possibleMoves)
        {
            // Simulate move
            var capturedPiece = board.GetPiece(move.to);
            board.MovePiece(move.from, move.to);
            
            // Evaluate position using minimax
            float score = MinimaxOld(searchDepth - 1, false, float.MinValue, float.MaxValue);
            
            // Undo move
            board.MovePiece(move.to, move.from);
            if (capturedPiece != null)
            {
                board.SetPiece(move.to, capturedPiece);
            }
            if (board.IsStalemate(playingColor == PieceColor.Black ? PieceColor.White : PieceColor.Black))
            {
                score = -50000f;
            }
            if (board.IsCheckmate(playingColor == PieceColor.Black ? PieceColor.White : PieceColor.Black))
            {
                score = 100000f;
            }
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        
        return bestMove;
    }
    
    public float MinimaxOld(int depth, bool maximizing, float alpha, float beta)
    {
        if (depth == 0 || IsGameOverOld())
        {
            return EvaluatePositionOld();
        }
        
        var moves = GetAllPossibleMovesOld(maximizing ? playingColor : playingColor == PieceColor.Black ? PieceColor.White : PieceColor.Black);
        
        if (maximizing)
        {
            float maxEval = float.MinValue;
            foreach (var move in moves)
            {
                var capturedPiece = board.GetPiece(move.to);
                board.MovePiece(move.from, move.to);
                
                float eval = MinimaxOld(depth - 1, false, alpha, beta);
                
                board.MovePiece(move.to, move.from);
                if (capturedPiece != null)
                    board.SetPiece(move.to, capturedPiece);
                
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                
                if (beta <= alpha)
                    break; // Alpha-beta pruning
            }
            return maxEval;
        }
        else
        {
            float minEval = float.MaxValue;
            foreach (var move in moves)
            {
                var capturedPiece = board.GetPiece(move.to);
                board.MovePiece(move.from, move.to);
                
                float eval = MinimaxOld(depth - 1, true, alpha, beta);
                
                board.MovePiece(move.to, move.from);
                if (capturedPiece != null)
                    board.SetPiece(move.to, capturedPiece);
                
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                
                if (beta <= alpha)
                    break;
            }
            return minEval;
        }
    }
    
    public float EvaluatePositionOld()
    {
        float score = 0;
        
        // Material evaluation
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board.GetPiece(new Vector2Int(x, y));
                if (piece != null)
                {
                    float pieceValue = GetPieceValueOld(piece.type);
                    if (piece.color == playingColor)
                        score += pieceValue;
                    else
                        score -= pieceValue;
                }
            }
        }
        
        // Positional bonuses
        score += EvaluatePositionalFactorsOld();
        
        return score;
    }
    
    public float GetPieceValueOld(PieceType type)
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
    
    public float EvaluatePositionalFactorsOld()
    {
        float score = 0;
        
        // Center control bonus
        Vector2Int[] centerSquares = {
            new Vector2Int(3, 3), new Vector2Int(3, 4),
            new Vector2Int(4, 3), new Vector2Int(4, 4)
        };
        
        foreach (var square in centerSquares)
        {
            var piece = board.GetPiece(square);
            if (piece != null)
            {
                if (piece.color == playingColor)
                    score += 0.3f;
                else
                    score -= 0.3f;
            }
        }
        
        return score;
    }
    
    public List<ChessMove> GetAllPossibleMovesOld(PieceColor color)
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
    
    public bool IsGameOverOld()
    {
        return board.IsCheckmate(PieceColor.White) || board.IsCheckmate(PieceColor.Black);
    }
    
    public IEnumerator ExecuteAIMoveOld(ChessMove move)
    {
        var piece = board.GetPiece(move.from);
        if (piece == null) yield break;
        
        // Set piece state
        var pieceAgent = piece.GetComponent<ChessPieceAgent>();
        pieceAgent.SetState(new MovingState());
        
        // Get pathfinding
        var pathfinder = piece.GetComponent<ChessPathfinder>();
        var path = pathfinder.FindPath(move.from, move.to);
        
        // Check for capture
        ChessPiece capturedPiece = board.GetPiece(move.to);
        Vector2Int fromPosition = piece.position;
        board.MovePiece(move.from, move.to);
        piece.position = move.to;
        if (gameStats != null)
        {
            PlayerTurn aiPlayerTurn = newai.aiColor == PieceColor.White ? PlayerTurn.White : PlayerTurn.Black;
            Debug.Log($"Calling gameStats.RecordMove() for AI piece {piece.type} to {move.to} by {aiPlayerTurn}");
            gameStats.RecordMove(fromPosition, move.to, piece, capturedPiece, aiPlayerTurn);
        }

        if (capturedPiece != null)
        {
            communication.BroadcastThreat(move.to, piece.color);
            yield return StartCoroutine(AnimateCapture(capturedPiece));
        }
        
        // Animate movement
        var steeringBehavior = piece.GetComponent<SteeringBehavior>();
        foreach (var waypoint in path)
        {
            Vector3 worldPos = ChessGameManager.Instance.GetComponent<ChessUIManager>().GetWorldPosition(waypoint);
            yield return StartCoroutine(steeringBehavior.MoveTo(worldPos));
        }
        
        
        
        // Move piece visually to correct parent
        var uiManager = ChessGameManager.Instance.GetComponent<ChessUIManager>();
        uiManager.MovePieceVisually(piece, move.to);
        
        if (gameStats != null)
        {
            gameStats.RecordMove(fromPosition, move.to, piece, capturedPiece, playingColor == PieceColor.Black ? PlayerTurn.Black : PlayerTurn.White); // MODIFIKOVANO
        }
        
        pieceAgent.SetState(new IdleState());
        
        
    }
    
    public IEnumerator AnimateCapture(ChessPiece piece)
    {
        var agent = piece.GetComponent<ChessPieceAgent>();
        agent.SetState(new CapturedState());
        yield return null; // Allow CapturedState to start its fade-out coroutine
        
        // Remove from board and visually
        // board.SetPiece(piece.position, null); // Redundant, handled by ChessGameManager.MovePiece
        var uiManager = ChessGameManager.Instance.GetComponent<ChessUIManager>();
        uiManager.RemovePieceVisually(piece);
    }
}

public struct ChessMoveOld
{
    public Vector2Int from;
    public Vector2Int to;
}