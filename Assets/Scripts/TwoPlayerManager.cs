using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TwoPlayerManager : MonoBehaviour
{
    [Header("Game Mode")]
    public bool isTwoPlayerMode = false;
    public bool enableBoardRotation = true;
    
    [Header("UI References")]
    public GameObject boardRotationParent;
    public TextMeshProUGUI currentPlayerText;
    public GameObject playerTransitionPanel;
    public TextMeshProUGUI transitionText;
    public Image playerIndicatorWhite;
    public Image playerIndicatorBlack;
    
    [Header("Rotation Settings")]
    public float rotationDuration = 1.0f;
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float transitionDisplayTime = 2.0f;
    
    [Header("Colors")]
    public Color activePlayerColor = Color.green;
    public Color inactivePlayerColor = Color.gray;
    
    private bool isRotating = false;
    private Quaternion targetRotation;
    private PlayerTurn currentActivePlayer;
    
    public static TwoPlayerManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void Initialize()
    {
        if (playerTransitionPanel != null)
            playerTransitionPanel.SetActive(false);
            
        if (boardRotationParent != null)
            boardRotationParent.transform.rotation = Quaternion.Euler(0, 0, 0);
            
        UpdatePlayerIndicators(PlayerTurn.White);

        // Postavi mod igre na osnovu PlayerPrefs
        bool twoPlayer = PlayerPrefs.GetInt("TwoPlayerMode", 0) == 1;
        SetGameMode(twoPlayer);
    }
    
    public void SetGameMode(bool twoPlayerMode)
    {
        isTwoPlayerMode = twoPlayerMode;
        
        if (isTwoPlayerMode)
        {
            var chessAI = ChessGameManager.Instance.GetComponent<ChessAI>();
            if (chessAI != null)
                chessAI.enabled = false;
                
            Debug.Log("Two Player Mode enabled");
        }
        else
        {
            var chessAI = ChessGameManager.Instance.GetComponent<ChessAI>();
            if (chessAI != null)
                chessAI.enabled = true;
                
            if (boardRotationParent != null)
                boardRotationParent.transform.rotation = Quaternion.Euler(0, 0, 0);
                
            Debug.Log("Single Player Mode enabled");
        }
    }
    
    public IEnumerator HandlePlayerSwitch(PlayerTurn newPlayer)
    {
        if (!isTwoPlayerMode || !enableBoardRotation)
        {
            UpdatePlayerIndicators(newPlayer);
            yield break;
        }
        
        currentActivePlayer = newPlayer;
        
        if (boardRotationParent != null)
        {
            yield return StartCoroutine(RotateBoard(newPlayer));
        }
        
        UpdatePlayerIndicators(newPlayer);
    }
    
    private IEnumerator ShowTransitionScreen(PlayerTurn player)
    {
        if (playerTransitionPanel == null) yield break;
        
        string playerName = player == PlayerTurn.White ? "WHITE" : "BLACK";
        transitionText.text = $"{playerName}'s Turn";
        transitionText.color = Color.white;
        
        playerTransitionPanel.SetActive(true);
        
        CanvasGroup cg = playerTransitionPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = playerTransitionPanel.AddComponent<CanvasGroup>();
        
        float elapsed = 0;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0, 1, elapsed / 0.5f);
            yield return null;
        }
        
        yield return new WaitForSeconds(transitionDisplayTime);
        
        elapsed = 0;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1, 0, elapsed / 0.5f);
            yield return null;
        }
        
        playerTransitionPanel.SetActive(false);
    }
    
    public static Quaternion CurrentPieceCorrection { get; private set; } = Quaternion.identity;
    
    private IEnumerator RotateBoard(PlayerTurn player)
    {
        if (isRotating) yield break;
        
        isRotating = true;
        
        // Ažuriraj statičku korekciju
        CurrentPieceCorrection = (player == PlayerTurn.Black) ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
        
        // Pronađi sve figure
        var board = ChessGameManager.Instance.GetBoard();
        var pieceTransforms = new List<Transform>();
        if (board != null)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    ChessPiece piece = board.GetPiece(new Vector2Int(x, y));
                    if (piece != null)
                    {
                        pieceTransforms.Add(piece.transform);
                    }
                }
            }
        }

        Quaternion startBoardRotation = boardRotationParent.transform.rotation;
        Quaternion endBoardRotation = (player == PlayerTurn.White) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(180, 0, 0);

        // Pretpostavimo da sve figure imaju istu početnu rotaciju pre okreta
        Quaternion startPieceRotation = (player == PlayerTurn.White) ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
        Quaternion endPieceRotation = CurrentPieceCorrection;
        
        float elapsed = 0;
        
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = rotationCurve.Evaluate(elapsed / rotationDuration);
            
            // Rotiraj tablu
            boardRotationParent.transform.rotation = Quaternion.Lerp(startBoardRotation, endBoardRotation, t);
            
            // Rotiraj sve figure istovremeno
            foreach (var pieceTransform in pieceTransforms)
            {
                pieceTransform.localRotation = Quaternion.Lerp(startPieceRotation, endPieceRotation, t);
            }
            
            yield return null;
        }
        
        // Postavi finalne rotacije
        boardRotationParent.transform.rotation = endBoardRotation;
        foreach (var pieceTransform in pieceTransforms)
        {
            pieceTransform.localRotation = endPieceRotation;
        }
        
        isRotating = false;
    }
    
    private void UpdatePlayerIndicators(PlayerTurn player)
    {
        if (currentPlayerText != null)
        {
            string playerName = player == PlayerTurn.White ? "WHITE" : "BLACK";
            currentPlayerText.text = $"Current Turn: {playerName}";
        }
        
        if (playerIndicatorWhite != null)
        {
            playerIndicatorWhite.color = player == PlayerTurn.White ? activePlayerColor : inactivePlayerColor;
        }
        
        if (playerIndicatorBlack != null)
        {
            playerIndicatorBlack.color = player == PlayerTurn.Black ? activePlayerColor : inactivePlayerColor;
        }
    }
    
    public bool CanPlayerMakeMove(PieceColor pieceColor, PlayerTurn currentTurn)
    {
        if (!isTwoPlayerMode)
        {
            return pieceColor == PieceColor.White && currentTurn == PlayerTurn.White;
        }
        
        if (currentTurn == PlayerTurn.White)
            return pieceColor == PieceColor.White;
        else
            return pieceColor == PieceColor.Black;
    }
    
    public void ResetBoardRotation()
    {
        if (boardRotationParent != null)
        {
            bool playerIsBlack = PlayerPrefs.GetInt("PlayerIsBlack", 0) == 1;
            bool isTwoPlayer = PlayerPrefs.GetInt("TwoPlayerMode", 0) == 1;

            if (!isTwoPlayer && playerIsBlack)
            {
                SetBoardRotation(Quaternion.Euler(180, 0, 0));
                CurrentPieceCorrection = Quaternion.Euler(0, 0, 180);
            }
            else
            {
                SetBoardRotation(Quaternion.Euler(0, 0, 0));
                CurrentPieceCorrection = Quaternion.identity;
            }
        }
    }

    public void SetBoardRotation(Quaternion rotation)
    {
        if (boardRotationParent != null)
        {
            boardRotationParent.transform.rotation = rotation;
        }
    }
}