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
        return FindPathAStar(start, goal);
    }

    private List<Vector2Int> FindPathAStar(Vector2Int start, Vector2Int goal)
    {
        var openSet = new List<PathNode>();
        var closedSet = new HashSet<Vector2Int>();
        
        var startNode = new PathNode { position = start, gCost = 0, hCost = GetHeuristicCost(start, goal), parent = null };
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
                if (closedSet.Contains(neighbor))
                    continue;

                // Ključna izmena: Putanja može ići samo preko praznih polja, osim ako je polje cilj.
                if (neighbor != goal && board.GetPiece(neighbor) != null)
                    continue;

                float newGCost = currentNode.gCost + GetMovementCost(currentNode.position, neighbor);
                var existingNode = openSet.FirstOrDefault(n => n.position == neighbor);
                
                if (existingNode == null)
                {
                    var neighborNode = new PathNode { position = neighbor, gCost = newGCost, hCost = GetHeuristicCost(neighbor, goal), parent = currentNode };
                    openSet.Add(neighborNode);
                }
                else if (newGCost < existingNode.gCost)
                {
                    existingNode.gCost = newGCost;
                    existingNode.parent = currentNode;
                }
            }
        }
        
        // Ako put nije pronađen, vrati praznu listu
        return new List<Vector2Int>();
    }

    private List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        var neighbors = new List<Vector2Int>();
        // Za potrebe animacije, uvek proveravamo svih 8 susednih polja
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                
                Vector2Int neighborPos = position + new Vector2Int(dx, dy);
                if (IsValidBoardPosition(neighborPos))
                {
                    neighbors.Add(neighborPos);
                }
            }
        }
        return neighbors;
    }

    private bool IsValidBoardPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < 8 && position.y >= 0 && position.y < 8;
    }

    private float GetHeuristicCost(Vector2Int a, Vector2Int b)
    {
        // Dijagonalna distanca je bolja za mrežu sa 8 smerova kretanja
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    private float GetMovementCost(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        return (dx > 0 && dy > 0) ? 1.4f : 1.0f; // Dijagonalni potezi su "duži"
    }

    private List<Vector2Int> ReconstructPath(PathNode node)
    {
        var path = new List<Vector2Int>();
        while (node.parent != null) // Ne uključujemo početnu poziciju u putanju
        {
            path.Add(node.position);
            node = node.parent;
        }
        path.Reverse();
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
