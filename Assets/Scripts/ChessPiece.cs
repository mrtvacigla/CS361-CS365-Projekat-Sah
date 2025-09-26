using UnityEngine;

public enum PieceType
{
    King, Queen, Rook, Bishop, Knight, Pawn
}

public class PieceSetupData
{
    public PieceType type;
    public PieceColor color;
    public Vector2Int position;
}
public class ChessPiece : MonoBehaviour
{
    [Header("Piece Data")]
    public PieceType type;
    public PieceColor color;
    public Vector2Int position;
    public bool hasMoved = false;
    
    public void Initialize(PieceType pieceType, PieceColor pieceColor, Vector2Int piecePosition)
    {
        this.type = pieceType;
        this.color = pieceColor;
        this.position = piecePosition;
        this.hasMoved = false;
    }
        
}
