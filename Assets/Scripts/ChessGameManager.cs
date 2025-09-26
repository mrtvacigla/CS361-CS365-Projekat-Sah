using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ChessGameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject chessBoardPanel;
    public GameObject pieceButtonPrefab;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI turnText;
    public Button newGameButton;
    public Button undoButton;
    
    [Header("Piece Sprites")]
    public Sprite[] whitePieceSprites;
    public Sprite[] blackPieceSprites;
    
    private ChessBoard chessBoard;
    private ChessAI chessAI;
    private PlayerTurn currentTurn = PlayerTurn.White;
    private ChessPiece selectedPiece;
    private List<Vector2Int> validMoves;
    private ChessUIManager uiManager;
    
    public static ChessGameManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        chessBoard = new ChessBoard();
        chessAI = GetComponent<ChessAI>();
        uiManager = GetComponent<ChessUIManager>();
        
        InitializeUI();
        StartNewGame();
    }
    
    private void InitializeUI()
    {
        uiManager.CreateChessBoard();
        newGameButton.onClick.AddListener(StartNewGame);
        undoButton.onClick.AddListener(UndoLastMove);
    }
    
    public void StartNewGame()
    {
        chessBoard.InitializeBoard();
        currentTurn = PlayerTurn.White;
        selectedPiece = null;
        validMoves = new List<Vector2Int>();
        
        uiManager.InitializeBoard();
        UpdateUI();
    }
    
    public void OnSquareClicked(int x, int y)
    {
        if (currentTurn != PlayerTurn.White) return;
    
        Vector2Int position = new Vector2Int(x, y);
        ChessPiece piece = chessBoard.GetPiece(position);
    
        if (selectedPiece == null)
        {
            if (piece != null && piece.color == PieceColor.White)
            {
                SelectPiece(piece);
            }
        }
        else
        {
            if (validMoves.Contains(position))
            {
                StartCoroutine(ExecuteMove(selectedPiece, position));
            }
            else if (piece != null && piece.color == PieceColor.White)
            {
                DeselectPiece();
                SelectPiece(piece);
            }
            else
            {
                DeselectPiece();
            }
        }
    }
    
    private void SelectPiece(ChessPiece piece)
    {
        selectedPiece = piece;
        validMoves = chessBoard.GetValidMoves(piece);
    
        piece.GetComponent<ChessPieceAgent>().SetState(PieceState.Selected);
    
        uiManager.HighlightValidMoves(validMoves);
        statusText.text = $"Selected {piece.type} at {piece.position}";
    }
    
    private void DeselectPiece()
    {
        if (selectedPiece != null)
        {
            var pieceAgent = selectedPiece.GetComponent<ChessPieceAgent>();
            pieceAgent.SetState(PieceState.Idle);
        
            uiManager.ResetPiecePosition(selectedPiece);
        }
    
        selectedPiece = null;
        validMoves.Clear();
        uiManager.ClearHighlights();
        statusText.text = "Select a piece to move";
    }
    
    private IEnumerator ExecuteMove(ChessPiece piece, Vector2Int targetPosition)
    {
        var pieceAgent = piece.GetComponent<ChessPieceAgent>();
        pieceAgent.SetState(PieceState.Moving);
        
        var pathfinder = piece.GetComponent<ChessPathfinder>();
        var path = pathfinder.FindPath(piece.position, targetPosition);
        ChessPiece capturedPiece = chessBoard.GetPiece(targetPosition);
        if (capturedPiece != null)
        {
            var communication = GetComponent<AgentCommunication>();
            communication.BroadcastThreat(targetPosition, piece.color);
            
            yield return StartCoroutine(AnimateCapture(capturedPiece));
        }
        
        yield return StartCoroutine(AnimateMovement(piece, path));
        
        chessBoard.MovePiece(piece.position, targetPosition);
        piece.position = targetPosition;
        uiManager.MovePieceVisually(piece, targetPosition);
        piece.hasMoved = true;
        
        pieceAgent.SetState(PieceState.Idle);
        DeselectPiece();
        
        if (chessBoard.IsCheckmate(PieceColor.Black))
        {
            statusText.text = "Checkmate! White wins!";
            yield return null;
        }
        
        currentTurn = PlayerTurn.Black;
        UpdateUI();
        
        yield return StartCoroutine(chessAI.MakeAIMove());
    }

    
    private IEnumerator AnimateMovement(ChessPiece piece, List<Vector2Int> path)
    {
        var steeringBehavior = piece.gameObject.GetComponent<SteeringBehavior>();
        
        foreach (var waypoint in path)
        {
            Vector3 worldPos = uiManager.GetWorldPosition(waypoint);
            yield return StartCoroutine(steeringBehavior.MoveTo(worldPos));
        }
    }
    
    private IEnumerator AnimateCapture(ChessPiece capturedPiece)
    {
        var agent = capturedPiece.GetComponent<ChessPieceAgent>();
        agent.SetState(PieceState.Captured);
    

        yield return StartCoroutine(uiManager.FadeOutPiece(capturedPiece));
    
        chessBoard.SetPiece(capturedPiece.position, null);
        uiManager.RemovePieceVisually(capturedPiece);
    }
    
    public void OnAIMoveComplete()
    {
        currentTurn = PlayerTurn.White;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        turnText.text = $"Current Turn: {currentTurn}";
        
        if (chessBoard.IsInCheck(currentTurn == PlayerTurn.White ? PieceColor.White : PieceColor.Black))
        {
            statusText.text = "Check!";
        }
        else
        {
            statusText.text = currentTurn == PlayerTurn.White ? "Your turn" : "AI thinking...";
        }
    }
    
    public void UndoLastMove()
    {
        
    }
    
    public ChessBoard GetBoard() => chessBoard;
    public PlayerTurn GetCurrentTurn() => currentTurn;
}