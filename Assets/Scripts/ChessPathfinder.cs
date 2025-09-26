using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChessPathfinder : MonoBehaviour
{
    private ChessPiece chessPiece;
    private ChessBoard board;
    
    private void Awake()
    {
        chessPiece = GetComponent<ChessPiece>();
    }
    
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        board = ChessGameManager.Instance.GetBoard();
        
        if (chessPiece.type == PieceType.Knight || RequiresPathfinding(start, goal))
        {
            return FindPathAStar(start, goal);
        }
        else
        {
            return new List<Vector2Int> { goal };
        }
    }
    
    private bool RequiresPathfinding(Vector2Int start, Vector2Int goal)
    {
        Vector2Int direction = goal - start;
        Vector2Int step = new Vector2Int(
            direction.x != 0 ? direction.x / Mathf.Abs(direction.x) : 0,
            direction.y != 0 ? direction.y / Mathf.Abs(direction.y) : 0
        );
        
        Vector2Int current = start + step;
        while (current != goal)
        {
            if (board.GetPiece(current) != null)
                return true;
            current += step;
        }
        
        return false;
    }
    
    private List<Vector2Int> FindPathAStar(Vector2Int start, Vector2Int goal)
    {
        var openSet = new List<PathNode>();
        var closedSet = new HashSet<Vector2Int>();
        
        var startNode = new PathNode
        {
            position = start,
            gCost = 0,
            hCost = GetHeuristicCost(start, goal),
            parent = null
        };
        
        openSet.Add(startNode);
        
        while (openSet.Count > 0)
        {
            var currentNode = openSet.OrderBy(n => n.fCost).First();
            openSet.Remove(currentNode);
            closedSet.Add(currentNode.position);
            
            if (currentNode.position == goal)
            {
                return ReconstructPath(currentNode);
            }
            
            foreach (var neighbor in GetNeighbors(currentNode.position))
            {
                if (closedSet.Contains(neighbor) || !IsValidPosition(neighbor))
                    continue;
                    
                float newGCost = currentNode.gCost + GetMovementCost(currentNode.position, neighbor);
                var existingNode = openSet.FirstOrDefault(n => n.position == neighbor);
                
                if (existingNode == null)
                {
                    var neighborNode = new PathNode
                    {
                        position = neighbor,
                        gCost = newGCost,
                        hCost = GetHeuristicCost(neighbor, goal),
                        parent = currentNode
                    };
                    openSet.Add(neighborNode);
                }
                else if (newGCost < existingNode.gCost)
                {
                    existingNode.gCost = newGCost;
                    existingNode.parent = currentNode;
                }
            }
        }
        
        return new List<Vector2Int> { goal };
    }
    
    private List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        var neighbors = new List<Vector2Int>();
        
        var moves = GetPossibleMoves(position);
        foreach (var move in moves)
        {
            if (IsValidBoardPosition(move))
                neighbors.Add(move);
        }
        
        return neighbors;
    }
    
    private List<Vector2Int> GetPossibleMoves(Vector2Int position)
    {
        var moves = new List<Vector2Int>();
        
        switch (chessPiece.type)
        {
            case PieceType.Knight:
                int[,] knightMoves = {{2,1}, {2,-1}, {-2,1}, {-2,-1}, {1,2}, {1,-2}, {-1,2}, {-1,-2}};
                for (int i = 0; i < knightMoves.GetLength(0); i++)
                {
                    moves.Add(position + new Vector2Int(knightMoves[i,0], knightMoves[i,1]));
                }
                break;
                
            case PieceType.King:
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx != 0 || dy != 0)
                            moves.Add(position + new Vector2Int(dx, dy));
                    }
                }
                break;
                
            default:
                Vector2Int[] directions = {
                    Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                    new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1)
                };
                foreach (var dir in directions)
                {
                    moves.Add(position + dir);
                }
                break;
        }
        
        return moves;
    }
    
    private bool IsValidPosition(Vector2Int position)
    {
        if (!IsValidBoardPosition(position))
            return false;
            
        var piece = board.GetPiece(position);
        return piece == null || piece.color != chessPiece.color;
    }
    
    private bool IsValidBoardPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < 8 && position.y >= 0 && position.y < 8;
    }
    
    private float GetHeuristicCost(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
    
    private float GetMovementCost(Vector2Int from, Vector2Int to)
    {
        float baseCost = 1.0f;
        var piece = board.GetPiece(to);
        
        if (piece != null && piece.color != chessPiece.color)
            baseCost += 0.5f; 
            
        return baseCost;
    }
    
    private List<Vector2Int> ReconstructPath(PathNode node)
    {
        var path = new List<Vector2Int>();
        
        while (node != null)
        {
            path.Add(node.position);
            node = node.parent;
        }
        
        path.Reverse();
        path.RemoveAt(0);
        return path;
    }
}

public class PathNode
{
    public Vector2Int position;
    public float gCost;
    public float hCost;
    public float fCost => gCost + hCost;
    public PathNode parent;
}