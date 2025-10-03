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
    public AIState currentState = AIState.Idle;
    public int searchDepth = 5;
    public float thinkingTime = 1f;
    public PieceColor aiColor = PieceColor.Black; // NOVO
    
    private ChessBoard board;
    private AgentCommunication communication;
    private GameStatistics gameStats; // NOVO
    private ChessGameManager gameManager; // NOVO
    
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
    }
    
    public IEnumerator MakeAIMove()
    {
        SetState(AIState.Analyzing);
        
        yield return new WaitForSeconds(thinkingTime);
        
        SetState(AIState.Planning);
        
        var bestMove = GetBestMove();
        
        if (bestMove != null)
        {
            SetState(AIState.Executing);
            yield return StartCoroutine(ExecuteAIMove(bestMove));
        }
        
        SetState(AIState.Idle);
        ChessGameManager.Instance.OnAIMoveComplete();
    }
    
    private void SetState(AIState newState)
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
    
    private ChessMove GetBestMove()
    {
        var possibleMoves = GetAllPossibleMoves(aiColor); // MODIFIKOVANO
        
        if (possibleMoves.Count == 0)
            return null;
        
        ChessMove bestMove = null;
        float bestScore = float.MinValue;
        
        foreach (var move in possibleMoves)
        {
            var capturedPiece = board.GetPiece(move.to);
            board.MovePiece(move.from, move.to);
            
            float score = Minimax(searchDepth - 1, false, float.MinValue, float.MaxValue);
            
            board.MovePiece(move.to, move.from);
            if (capturedPiece != null)
            {
                board.SetPiece(move.to, capturedPiece);
            }
            
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        
        return bestMove;
    }
    
    private float Minimax(int depth, bool maximizing, float alpha, float beta)
    {
        if (depth == 0 || IsGameOver())
        {
            PieceColor perspective = maximizing ? aiColor : (aiColor == PieceColor.White ? PieceColor.Black : PieceColor.White);
            return EvaluatePosition(perspective);
        }
        
        // MODIFIKOVANO - Odredi boju na osnovu aiColor
        PieceColor moveColor = maximizing ? aiColor : (aiColor == PieceColor.White ? PieceColor.Black : PieceColor.White);
        var moves = GetAllPossibleMoves(moveColor);
        
        if (maximizing)
        {
            float maxEval = float.MinValue;
            foreach (var move in moves)
            {
                var capturedPiece = board.GetPiece(move.to);
                board.MovePiece(move.from, move.to);
                
                float eval = Minimax(depth - 1, false, alpha, beta);
                
                board.MovePiece(move.to, move.from);
                if (capturedPiece != null)
                    board.SetPiece(move.to, capturedPiece);
                
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                
                if (beta <= alpha)
                    break;
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
                
                float eval = Minimax(depth - 1, true, alpha, beta);
                
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
    // NOVO - javna metoda za evaluaciju trenutne pozicije
    public float EvaluateCurrentPosition(PlayerTurn playerTurn)
    {
        PieceColor currentPlayerColor = playerTurn == PlayerTurn.White ? PieceColor.White : PieceColor.Black;
        return EvaluatePosition(currentPlayerColor);
    }
    
    private float EvaluatePosition(PieceColor perspectiveColor)
    {
        float score = 0;
        
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
        
        score += EvaluatePositionalFactors(perspectiveColor);
        
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
    
    private IEnumerator ExecuteAIMove(ChessMove move)
    {
        var piece = board.GetPiece(move.from);
        if (piece == null) yield break;
    
        var pieceAgent = piece.GetComponent<ChessPieceAgent>();
        pieceAgent.SetState(PieceState.Moving);
    
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
    
        pieceAgent.SetState(PieceState.Idle);
    }
    
    private IEnumerator AnimateCapture(ChessPiece piece)
    {
        var agent = piece.GetComponent<ChessPieceAgent>();
        agent.SetState(PieceState.Captured);
    
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