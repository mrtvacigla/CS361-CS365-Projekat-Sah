using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ChessUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject squareButtonPrefab;
    public GameObject chessPiecePrefab;
    public Transform boardParent;
    public Color lightSquareColor = Color.white;
    public Color darkSquareColor = Color.gray;
    public Sprite highlightSprite;
    public Color threatColor = Color.red;
    
    private Button[,] squareButtons = new Button[8, 8];
    private Dictionary<ChessPiece, GameObject> pieceObjects = new Dictionary<ChessPiece, GameObject>();
    
    public void CreateChessBoard()
    {
        var gridLayout = boardParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = boardParent.gameObject.AddComponent<GridLayoutGroup>();
        }
        
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 8;
        gridLayout.cellSize = new Vector2(125,125);
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        
        for (int y = 7; y >= 0; y--)
        {
            for (int x = 0; x < 8; x++)
            {
                CreateSquareButton(x, y);
            }
        }
    }
    
    private void CreateSquareButton(int x, int y)
    {
        GameObject squareObj = Instantiate(squareButtonPrefab, boardParent);
        Button button = squareObj.GetComponent<Button>();
        Image image = squareObj.GetComponent<Image>();
        bool isLightSquare = (x + y) % 2 == 0;
        image.color = new Color(0, 0, 0, 0);
        int capturedX = x, capturedY = y;
        button.onClick.AddListener(() => ChessGameManager.Instance.OnSquareClicked(capturedX, capturedY));
        
        squareButtons[x, y] = button;
        squareObj.name = $"Square_{(char)('a' + x)}{y + 1}";
    }

    public void InitializeBoard()
    {
        foreach (var pieceObj in pieceObjects.Values)
        {
            if (pieceObj != null)
                Destroy(pieceObj);
        }

        pieceObjects.Clear();

        var board = ChessGameManager.Instance.GetBoard();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var setupData = board.GetInitialSetup(x, y);
                if (setupData != null)
                {
                    CreatePieceObject(setupData, x, y);
                }
            }
        }
    }
    

    public void ResetPiecePosition(ChessPiece piece)
    {
        if (piece == null || !pieceObjects.ContainsKey(piece))
            return;
    
        var pieceObj = pieceObjects[piece];
        if (pieceObj == null)
            return;

        StopCoroutine(nameof(ResetPiecePositionSmooth));

        StartCoroutine(ResetPiecePositionSmooth(pieceObj));
    }

    private IEnumerator ResetPiecePositionSmooth(GameObject pieceObj)
    {
        Vector3 startPos = pieceObj.transform.localPosition;
        Quaternion startRot = pieceObj.transform.localRotation;
    
        Vector3 targetPos = Vector3.zero;
        Quaternion targetRot = Quaternion.Euler(0, 0, 0);

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            pieceObj.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            pieceObj.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        pieceObj.transform.localPosition = targetPos;
        pieceObj.transform.localRotation = targetRot;
    }

    private void CreatePieceObject(PieceSetupData setupData, int x, int y)
    {
        GameObject pieceObj = Instantiate(chessPiecePrefab, squareButtons[x, y].transform);
        ChessPiece chessPiece = pieceObj.GetComponent<ChessPiece>();
        if (chessPiece != null)
        {
            chessPiece.Initialize(setupData.type, setupData.color, setupData.position);
        }
        
        Image pieceImage = pieceObj.GetComponent<Image>();
        if (pieceImage != null)
        {
            Sprite pieceSprite = GetPieceSprite(chessPiece);
            pieceImage.sprite = pieceSprite;
        }

        var board = ChessGameManager.Instance.GetBoard();
        board.SetPiece(new Vector2Int(x, y), chessPiece);
        
        pieceObjects[chessPiece] = pieceObj;
        pieceObj.name = $"{chessPiece.color}_{chessPiece.type}_{x}_{y}";
        pieceObj.transform.localPosition = Vector3.zero;
    }
    
    public void MovePieceVisually(ChessPiece piece, Vector2Int newPosition)
    {
        if (piece == null || !pieceObjects.ContainsKey(piece))
            return;
            
        var pieceObj = pieceObjects[piece];
        if (pieceObj == null)
            return;
        
        var newParent = squareButtons[newPosition.x, newPosition.y].transform;
        pieceObj.transform.SetParent(newParent);
        pieceObj.transform.localPosition = Vector3.zero;
        pieceObj.transform.localRotation = Quaternion.Euler(0,0,0);
        pieceObj.transform.rotation = Quaternion.Euler(0,0,0);
        
        pieceObj.name = $"{piece.color}_{piece.type}_{newPosition.x}_{newPosition.y}";
    }
    
    public void RemovePieceVisually(ChessPiece piece)
    {
        if (piece != null && pieceObjects.ContainsKey(piece))
        {
            var pieceObj = pieceObjects[piece];
            if (pieceObj != null)
            {
                Destroy(pieceObj);
            }
            pieceObjects.Remove(piece);
        }
    }
    
    private Sprite GetPieceSprite(ChessPiece piece)
    {
        var gameManager = ChessGameManager.Instance;
        Sprite[] sprites = piece.color == PieceColor.White ? gameManager.whitePieceSprites : gameManager.blackPieceSprites;
        
        int index = piece.type switch
        {
            PieceType.King => 0,
            PieceType.Queen => 1,
            PieceType.Rook => 2,
            PieceType.Bishop => 3,
            PieceType.Knight => 4,
            PieceType.Pawn => 5,
            _ => 0
        };
        
        return sprites[index];
    }
    
    public void HighlightValidMoves(List<Vector2Int> moves)
    {
        ClearHighlights();
        
        foreach (var move in moves)
        {
            var button = squareButtons[move.x, move.y];
            var image = button.GetComponent<Image>();
            image.color = new  Color(0,0, 0, 255);
            image.sprite = highlightSprite;
        }
    }
    
    public void ClearHighlights()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var button = squareButtons[x, y];
                var image = button.GetComponent<Image>();
                
                image.color = new  Color(0, 0, 0, 0);
                image.sprite = null;
            }
        }
    }
    
    public Vector3 GetWorldPosition(Vector2Int boardPosition)
    {
        return squareButtons[boardPosition.x, boardPosition.y].transform.position;
    }
    
    public IEnumerator ShowThreatEffect(Vector2Int position)
    {
        var button = squareButtons[position.x, position.y];
        var image = button.GetComponent<Image>();
        Color originalColor = image.color;
        
        for (int i = 0; i < 3; i++)
        {
            image.color = threatColor;
            yield return new WaitForSeconds(0.2f);
            image.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    public IEnumerator FadeOutPiece(ChessPiece piece)
    {
        if (pieceObjects.ContainsKey(piece))
        {
            var pieceObj = pieceObjects[piece];
            var image = pieceObj.GetComponent<Image>();
            
            float alpha = 1.0f;
            while (alpha > 0)
            {
                alpha -= Time.deltaTime * 2.0f;
                Color color = image.color;
                color.a = alpha;
                image.color = color;
                yield return null;
            }
        }
    }
}