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
    public Button showReportButton; // NOVO
    
    [Header("Piece Sprites")]
    public Sprite[] whitePieceSprites;
    public Sprite[] blackPieceSprites;
    
    private ChessBoard chessBoard;
    private ChessAI chessAI;
    private PlayerTurn currentTurn = PlayerTurn.White;
    private ChessPiece selectedPiece;
    private List<Vector2Int> validMoves;
    private ChessUIManager uiManager;
    public GameStatistics gameStats; // NOVO
    public TwoPlayerManager twoPlayerManager; // NOVO

    private bool playerIsBlack = false; // NOVO
    public bool isGameOver = false; // NOVO

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
       
        if (gameStats == null) Debug.LogError("GameStatistics component not found on ChessGameManager GameObject!");
        else Debug.Log("GameStatistics component found.");
        
        twoPlayerManager.Initialize();
        
        // NOVO - Pročitaj izbor igrača
        playerIsBlack = PlayerPrefs.GetInt("PlayerIsBlack", 0) == 1;
        
        // NOVO - Konfiguriši AI boju u single-player modu
        if (twoPlayerManager != null && !twoPlayerManager.isTwoPlayerMode)
        {
            if (chessAI != null)
                chessAI.aiColor = playerIsBlack ? PieceColor.White : PieceColor.Black;
        }
        
        InitializeUI();
        StartNewGame();
    }
    
    private void InitializeUI()
    {
        uiManager.CreateChessBoard();
        newGameButton.onClick.AddListener(StartNewGame);
        undoButton.onClick.AddListener(UndoLastMove);
        
        // NOVO - dugme za prikaz izveštaja
        if (showReportButton != null)
            showReportButton.onClick.AddListener(ShowGameReport);
    }
    
    public void StartNewGame()
    {
        chessBoard.InitializeBoard();
        currentTurn = PlayerTurn.White;
        selectedPiece = null;
        validMoves = new List<Vector2Int>();
        isGameOver = false; // Reset game over flag
        
        uiManager.InitializeBoard();
        
        // NOVO - pokreni statistiku
        if (gameStats != null)
        {
            Debug.Log("Calling gameStats.StartNewGame()");
            gameStats.StartNewGame();
        }
        
        // NOVO - reset rotacije
        if (twoPlayerManager != null)
            twoPlayerManager.ResetBoardRotation();
        
        UpdateUI();
        
        // NOVO - Ako igrač igra kao crni, AI (beli) igra prvi
        if (twoPlayerManager != null && !twoPlayerManager.isTwoPlayerMode && playerIsBlack)
        {
            if (chessAI != null)
                StartCoroutine(chessAI.MakeAIMove());
        }
    }
    
    private bool CanSelectPiece(ChessPiece piece)
    {
        if (piece == null) return false;

        if (twoPlayerManager != null && twoPlayerManager.isTwoPlayerMode)
        {
            // U 2P modu, možeš selektovati figuru ako je tvoj red
            return twoPlayerManager.CanPlayerMakeMove(piece.color, currentTurn);
        }
        else
        {
            // U single-player modu, možeš selektovati samo figure svoje boje
            PieceColor playerColor = playerIsBlack ? PieceColor.Black : PieceColor.White;
            return piece.color == playerColor;
        }
    }

    public void OnSquareClicked(int x, int y)
    {
        if (isGameOver) return; // Prevent moves if game is over

        Vector2Int position = new Vector2Int(x, y);
        // U single-player modu, ignoriši klikove ako je red na AI-ju
        if (twoPlayerManager != null && !twoPlayerManager.isTwoPlayerMode)
        {
            PlayerTurn playerTurn = playerIsBlack ? PlayerTurn.Black : PlayerTurn.White;

            if (currentTurn != playerTurn)
            {
                return;
            }
        }
        
        if (selectedPiece != null)
        {
            // Ako je figura već selektovana, pokušaj da odigraš potez ili promeniš selekciju
            if (validMoves.Contains(position))
            {
                if (gameStats != null)
                {
                    gameStats.RecordPreMoveEvaluation(currentTurn);
                }
                StartCoroutine(ExecuteMove(selectedPiece, position));
            }
            else
            {
                ChessPiece pieceOnSquare = chessBoard.GetPiece(position);
                if (CanSelectPiece(pieceOnSquare))
                {
                    DeselectPiece();
                    SelectPiece(pieceOnSquare);
                }
                else
                {
                    DeselectPiece();
                }
            }
        }
        else
        {
            // Ako ni jedna figura nije selektovana, pokušaj da selektuješ jednu
            ChessPiece pieceOnSquare = chessBoard.GetPiece(position);
            if (CanSelectPiece(pieceOnSquare))
            {
                SelectPiece(pieceOnSquare);
            }
        }
    }
    
    private void SelectPiece(ChessPiece piece)
    {
                selectedPiece = piece;
                validMoves = chessBoard.GetValidMoves(piece);
        
                piece.GetComponent<ChessPieceAgent>().SetState(new SelectedState());    
        uiManager.HighlightValidMoves(validMoves);
        statusText.text = $"Selected {piece.type} at {piece.position}";
    }
    
        private void DeselectPiece()
        {
            if (selectedPiece != null)
            {
                var pieceAgent = selectedPiece.GetComponent<ChessPieceAgent>();
                pieceAgent.SetState(new IdleState());
    
                uiManager.ResetPiecePosition(selectedPiece);
            }
    
            selectedPiece = null;
            validMoves.Clear();
            uiManager.ClearHighlights();
            statusText.text = "Select a piece to move";
        }    
    private IEnumerator ExecuteMove(ChessPiece piece, Vector2Int targetPosition)
    {
        if (isGameOver) yield break; // Prevent execution if game is already over

        uiManager.ClearHighlights();
        
        var pieceAgent = piece.GetComponent<ChessPieceAgent>();
        pieceAgent.SetState(new MovingState());
        
        var pathfinder = piece.GetComponent<ChessPathfinder>();
        var path = pathfinder.FindPath(piece.position, targetPosition);
        ChessPiece capturedPiece = chessBoard.GetPiece(targetPosition);

        Debug.Log($"Captured piece at {targetPosition}: {(capturedPiece != null ? capturedPiece.type.ToString() : "None")}");

        Vector2Int fromPosition = piece.position;

        // Record move BEFORE animating capture and destroying the piece
        if (gameStats != null)
        {
            PlayerTurn playerMakingMove = currentTurn; // Pretpostavi da je trenutni red igrač koji pravi potez
            if (!twoPlayerManager.isTwoPlayerMode && currentTurn == (playerIsBlack ? PlayerTurn.Black : PlayerTurn.White))
            {
                // Ako je single-player mod i trenutni red je igračev, onda je igrač napravio potez
                playerMakingMove = playerIsBlack ? PlayerTurn.Black : PlayerTurn.White;
            }
            else if (!twoPlayerManager.isTwoPlayerMode && currentTurn == (chessAI.aiColor == PieceColor.White ? PlayerTurn.White : PlayerTurn.Black))
            {
                // Ako je single-player mod i trenutni red je AI-jev, onda je AI napravio potez
                playerMakingMove = chessAI.aiColor == PieceColor.White ? PlayerTurn.White : PlayerTurn.Black;
            }
            Debug.Log($"Calling gameStats.RecordMove() for piece {piece.type} to {targetPosition} by {playerMakingMove}");
            gameStats.RecordMove(fromPosition, targetPosition, piece, capturedPiece, playerMakingMove);
        }
        
        if (capturedPiece != null)
        {
            var communication = GetComponent<AgentCommunication>();
            communication.BroadcastThreat(targetPosition, piece.color);
            
            yield return StartCoroutine(AnimateCapture(capturedPiece));
        }

        // Privremeno promeni roditelja figure da bi se izbegao konflikt sa LayoutGroup-om
        Transform pieceTransform = piece.transform;
        pieceTransform.SetParent(uiManager.boardParent.parent, true); // 'true' čuva svetsku poziciju

        if (path != null && path.Count > 0)
        {
            Debug.Log($"Animating movement for {piece.type}. Path length: {path.Count}");
            yield return StartCoroutine(AnimateMovement(piece, path));
        }
        
        chessBoard.MovePiece(piece.position, targetPosition);
        piece.position = targetPosition;
        uiManager.MovePieceVisually(piece, targetPosition); // Vraća figuru kao dete novog polja
        piece.hasMoved = true;
        
        DeselectPiece();
        
        PieceColor opponentColor = currentTurn == PlayerTurn.White ? PieceColor.Black : PieceColor.White;
        if (chessBoard.IsInCheck(opponentColor))
        {
            if (gameStats != null)
                gameStats.RecordCheck(currentTurn);
        }
        
        if (chessBoard.IsCheckmate(opponentColor))
        {
            string winner = currentTurn == PlayerTurn.White ? "White wins!" : "Black wins!";
            statusText.text = $"Checkmate! {winner}";
            
            if (gameStats != null)
                gameStats.EndGame(winner, "Checkmate");
            
            isGameOver = true; // Set game over flag
            yield return StartCoroutine(ShowReportAfterDelay(2f));
            
            yield break;
        }
        else if (chessBoard.IsStalemate(opponentColor))
        {
            statusText.text = "Stalemate! It's a draw.";

            if (gameStats != null)
                gameStats.EndGame("Draw", "Stalemate");

            isGameOver = true; // Set game over flag
            yield return StartCoroutine(ShowReportAfterDelay(2f));

            yield break;
        }
        
        currentTurn = currentTurn == PlayerTurn.White ? PlayerTurn.Black : PlayerTurn.White;
        
        if (twoPlayerManager != null && twoPlayerManager.isTwoPlayerMode)
        {
            yield return StartCoroutine(twoPlayerManager.HandlePlayerSwitch(currentTurn));
        }
        
        UpdateUI();
        
        // MODIFIKOVANO - Proveri da li je red na AI
        if (chessAI != null && chessAI.enabled && currentTurn == (chessAI.aiColor == PieceColor.White ? PlayerTurn.White : PlayerTurn.Black))
        {
            yield return StartCoroutine(chessAI.MakeAIMove());
        }
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
        Debug.Log($"Animating capture for {capturedPiece.type} at {capturedPiece.position}");
        var agent = capturedPiece.GetComponent<ChessPieceAgent>();
        agent.SetState(new CapturedState());
        yield return null; // Allow CapturedState to start its fade-out coroutine
    
        chessBoard.SetPiece(capturedPiece.position, null);
        uiManager.RemovePieceVisually(capturedPiece);
    }
    
    public void OnAIMoveComplete()
    {
        if (isGameOver) return; // Prevent turn switch if game is over

        // MODIFIKOVANO - Postavi red na igrača
        currentTurn = (chessAI != null && chessAI.aiColor == PieceColor.White) ? PlayerTurn.Black : PlayerTurn.White;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (isGameOver)
        {
            turnText.text = ""; // Clear turn text if game is over
            // statusText will already be set by ExecuteMove for checkmate/stalemate
            return;
        }

        turnText.text = $"Current Turn: {currentTurn}";
        
        if (chessBoard.IsInCheck(currentTurn == PlayerTurn.White ? PieceColor.White : PieceColor.Black))
        {
            statusText.text = "Check!";
        }
        else
        {
            if (twoPlayerManager != null && twoPlayerManager.isTwoPlayerMode)
            {
                statusText.text = $"{currentTurn}'s turn - Make your move";
            }
            else
            {
                PlayerTurn playerTurn = playerIsBlack ? PlayerTurn.Black : PlayerTurn.White;
                statusText.text = currentTurn == playerTurn ? "Your turn" : "AI thinking...";
            }
        }
    }
    
    private void ShowGameReport()
    {
        if (gameStats != null)
        {
            var report = gameStats.GenerateReport();
            var reportUI = GetComponent<GameReportUI>();
            if (reportUI != null)
            {
                reportUI.ShowReport(report);
            }
        }
    }
    
    private IEnumerator ShowReportAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowGameReport();
    }
    
    public void UndoLastMove()
    {
        // TODO: Implementiraj undo funkcionalnost
    }
    
    public ChessBoard GetBoard() => chessBoard;
    public PlayerTurn GetCurrentTurn() => currentTurn;
}
