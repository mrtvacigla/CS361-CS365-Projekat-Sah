using UnityEngine;
using System.Collections.Generic;



public enum PieceColor
{
    White, Black
}

public enum PlayerTurn
{
    White, Black
}

public class ChessBoard
{
    private ChessPiece[,] board = new ChessPiece[8, 8];
    private List<ChessMove> moveHistory = new List<ChessMove>();
    
    public void InitializeBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] != null)
                {
                    if (board[x, y].gameObject != null)
                        Object.Destroy(board[x, y].gameObject);
                }
                board[x, y] = null;
            }
        }
        
        InitializePieceData();
        moveHistory.Clear();
    }
    
    private void InitializePieceData()
    {
      
    }
    
    public ChessPiece GetPiece(Vector2Int position)
    {
        if (!IsValidPosition(position)) return null;
        return board[position.x, position.y];
    }
    
    public void SetPiece(Vector2Int position, ChessPiece piece)
    {
        if (!IsValidPosition(position)) return;
        board[position.x, position.y] = piece;
        if (piece != null)
            piece.position = position;
    }
    
    public void MovePiece(Vector2Int from, Vector2Int to)
    {
        var piece = GetPiece(from);
        if (piece == null) return;
        
        moveHistory.Add(new ChessMove { from = from, to = to });
        
        board[from.x, from.y] = null;
        board[to.x, to.y] = piece;
        piece.position = to;
    }
    
    public PieceSetupData GetInitialSetup(int x, int y)
    {
        if (y == 0)
        {
            PieceType type = x switch
            {
                0 or 7 => PieceType.Rook,
                1 or 6 => PieceType.Knight,
                2 or 5 => PieceType.Bishop,
                3 => PieceType.Queen,
                4 => PieceType.King,
                _ => PieceType.Pawn
            };
            return new PieceSetupData { type = type, color = PieceColor.White, position = new Vector2Int(x, y) };
        }
        else if (y == 1)
        {
            return new PieceSetupData { type = PieceType.Pawn, color = PieceColor.White, position = new Vector2Int(x, y) };
        }
        else if (y == 7)
        {
            PieceType type = x switch
            {
                0 or 7 => PieceType.Rook,
                1 or 6 => PieceType.Knight,
                2 or 5 => PieceType.Bishop,
                3 => PieceType.Queen,
                4 => PieceType.King,
                _ => PieceType.Pawn
            };
            return new PieceSetupData { type = type, color = PieceColor.Black, position = new Vector2Int(x, y) };
        }
        else if (y == 6)
        {
            return new PieceSetupData { type = PieceType.Pawn, color = PieceColor.Black, position = new Vector2Int(x, y) };
        }
        
        return null;
    }
    
    public List<Vector2Int> GetValidMoves(ChessPiece piece)
    {
        if (piece == null) return new List<Vector2Int>();
        
        var moves = new List<Vector2Int>();
        
        switch (piece.type)
        {
            case PieceType.Pawn:
                moves.AddRange(GetPawnMoves(piece));
                break;
            case PieceType.Rook:
                moves.AddRange(GetRookMoves(piece));
                break;
            case PieceType.Knight:
                moves.AddRange(GetKnightMoves(piece));
                break;
            case PieceType.Bishop:
                moves.AddRange(GetBishopMoves(piece));
                break;
            case PieceType.Queen:
                moves.AddRange(GetQueenMoves(piece));
                break;
            case PieceType.King:
                moves.AddRange(GetKingMoves(piece));
                break;
        }
        
        return FilterLegalMoves(piece, moves);
    }
    
    private List<Vector2Int> GetPawnMoves(ChessPiece pawn)
    {
        var moves = new List<Vector2Int>();
        int direction = pawn.color == PieceColor.White ? 1 : -1;
        Vector2Int pos = pawn.position;
        
        Vector2Int forward = pos + new Vector2Int(0, direction);
        if (IsValidPosition(forward) && GetPiece(forward) == null)
        {
            moves.Add(forward);
            
            if (!pawn.hasMoved)
            {
                Vector2Int doubleForward = pos + new Vector2Int(0, direction * 2);
                if (IsValidPosition(doubleForward) && GetPiece(doubleForward) == null)
                {
                    moves.Add(doubleForward);
                }
            }
        }
        
        Vector2Int[] captureDirections = { new Vector2Int(-1, direction), new Vector2Int(1, direction) };
        foreach (var captureDir in captureDirections)
        {
            Vector2Int capturePos = pos + captureDir;
            if (IsValidPosition(capturePos))
            {
                var targetPiece = GetPiece(capturePos);
                if (targetPiece != null && targetPiece.color != pawn.color)
                {
                    moves.Add(capturePos);
                }
            }
        }
        
        return moves;
    }
    
    private List<Vector2Int> GetRookMoves(ChessPiece rook)
    {
        var moves = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        foreach (var direction in directions)
        {
            moves.AddRange(GetMovesInDirection(rook, direction));
        }
        
        return moves;
    }
    
    private List<Vector2Int> GetKnightMoves(ChessPiece knight)
    {
        var moves = new List<Vector2Int>();
        Vector2Int pos = knight.position;
        
        Vector2Int[] knightMoves = {
            new Vector2Int(2, 1), new Vector2Int(2, -1), new Vector2Int(-2, 1), new Vector2Int(-2, -1),
            new Vector2Int(1, 2), new Vector2Int(1, -2), new Vector2Int(-1, 2), new Vector2Int(-1, -2)
        };
        
        foreach (var move in knightMoves)
        {
            Vector2Int target = pos + move;
            if (IsValidPosition(target))
            {
                var targetPiece = GetPiece(target);
                if (targetPiece == null || targetPiece.color != knight.color)
                {
                    moves.Add(target);
                }
            }
        }
        
        return moves;
    }
    
    private List<Vector2Int> GetBishopMoves(ChessPiece bishop)
    {
        var moves = new List<Vector2Int>();
        Vector2Int[] directions = { 
            new Vector2Int(1, 1), new Vector2Int(1, -1), 
            new Vector2Int(-1, 1), new Vector2Int(-1, -1) 
        };
        
        foreach (var direction in directions)
        {
            moves.AddRange(GetMovesInDirection(bishop, direction));
        }
        
        return moves;
    }
    
    private List<Vector2Int> GetQueenMoves(ChessPiece queen)
    {
        var moves = new List<Vector2Int>();
        moves.AddRange(GetRookMoves(queen));
        moves.AddRange(GetBishopMoves(queen));
        return moves;
    }
    
    private List<Vector2Int> GetKingMoves(ChessPiece king)
    {
        var moves = new List<Vector2Int>();
        Vector2Int pos = king.position;
        
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                
                Vector2Int target = pos + new Vector2Int(dx, dy);
                if (IsValidPosition(target))
                {
                    var targetPiece = GetPiece(target);
                    if (targetPiece == null || targetPiece.color != king.color)
                    {
                        moves.Add(target);
                    }
                }
            }
        }
        
        return moves;
    }
    
    private List<Vector2Int> GetMovesInDirection(ChessPiece piece, Vector2Int direction)
    {
        var moves = new List<Vector2Int>();
        Vector2Int current = piece.position + direction;
        
        while (IsValidPosition(current))
        {
            var targetPiece = GetPiece(current);
            
            if (targetPiece == null)
            {
                moves.Add(current);
            }
            else
            {
                if (targetPiece.color != piece.color)
                {
                    moves.Add(current);
                }
                break;
            }
            
            current += direction;
        }
        
        return moves;
    }
    
    private List<Vector2Int> FilterLegalMoves(ChessPiece piece, List<Vector2Int> moves)
    {
        var legalMoves = new List<Vector2Int>();
        
        foreach (var move in moves)
        {
            var originalPiece = GetPiece(move);
            var originalPosition = piece.position;
            
            board[piece.position.x, piece.position.y] = null;
            board[move.x, move.y] = piece;
            piece.position = move;
            
            if (!IsInCheck(piece.color))
            {
                legalMoves.Add(move);
            }
            
            board[originalPosition.x, originalPosition.y] = piece;
            board[move.x, move.y] = originalPiece;
            piece.position = originalPosition;
        }
        
        return legalMoves;
    }
    
    public bool IsInCheck(PieceColor color)
    {
        Vector2Int kingPosition = Vector2Int.zero;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board[x, y];
                if (piece != null && piece.type == PieceType.King && piece.color == color)
                {
                    kingPosition = piece.position;
                    break;
                }
            }
        }
        
        return IsPositionUnderAttack(kingPosition, color == PieceColor.White ? PieceColor.Black : PieceColor.White);
    }
    
    public bool IsPositionUnderAttack(Vector2Int position, PieceColor attackingColor)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board[x, y];
                if (piece != null && piece.color == attackingColor)
                {
                    var attacks = GetAttackingSquares(piece);
                    if (attacks.Contains(position))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    private List<Vector2Int> GetAttackingSquares(ChessPiece piece)
    {
        var attacks = new List<Vector2Int>();
        
        switch (piece.type)
        {
            case PieceType.Pawn:
                int direction = piece.color == PieceColor.White ? 1 : -1;
                Vector2Int[] captureDirections = { new Vector2Int(-1, direction), new Vector2Int(1, direction) };
                foreach (var captureDir in captureDirections)
                {
                    Vector2Int capturePos = piece.position + captureDir;
                    if (IsValidPosition(capturePos))
                    {
                        attacks.Add(capturePos);
                    }
                }
                break;
            case PieceType.Rook:
                attacks.AddRange(GetRookMoves(piece));
                break;
            case PieceType.Knight:
                attacks.AddRange(GetKnightMoves(piece));
                break;
            case PieceType.Bishop:
                attacks.AddRange(GetBishopMoves(piece));
                break;
            case PieceType.Queen:
                attacks.AddRange(GetQueenMoves(piece));
                break;
            case PieceType.King:
                attacks.AddRange(GetKingMoves(piece));
                break;
        }
        
        return attacks;
    }
    
    public bool IsCheckmate(PieceColor color)
    {
        if (!IsInCheck(color)) return false;
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board[x, y];
                if (piece != null && piece.color == color)
                {
                    if (GetValidMoves(piece).Count > 0)
                    {
                        return false;
                    }
                }
            }
        }
        
        return true;
    }
    
    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < 8 && position.y >= 0 && position.y < 8;
    }
}
