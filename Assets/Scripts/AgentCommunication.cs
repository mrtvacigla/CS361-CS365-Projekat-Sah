using UnityEngine;
using System;

public class AgentCommunication : MonoBehaviour
{
    public event Action<Vector2Int, PieceColor> OnThreatBroadcast;
    public event Action<Vector2Int, PieceColor> OnDefenseRequest;
    public event Action<Vector2Int, PieceColor> OnPositionSecured;
    
    [Header("Communication Settings")]
    public float communicationRange = 3.0f;
    public LayerMask pieceLayerMask = 1;
    
    public void BroadcastThreat(Vector2Int position, PieceColor attackerColor)
    {
        Debug.Log($"Threat broadcast at {position} by {attackerColor}");
        OnThreatBroadcast?.Invoke(position, attackerColor);
        
        // Visual feedback
        StartCoroutine(ShowThreatIndicator(position));
    }
    
    public void RequestDefense(Vector2Int position, PieceColor allyColor)
    {
        Debug.Log($"Defense requested at {position} by {allyColor}");
        OnDefenseRequest?.Invoke(position, allyColor);
    }
    
    public void SecurePosition(Vector2Int position, PieceColor controllingColor)
    {
        Debug.Log($"Position {position} secured by {controllingColor}");
        OnPositionSecured?.Invoke(position, controllingColor);
    }
    
    private System.Collections.IEnumerator ShowThreatIndicator(Vector2Int position)
    {
        // Create visual indicator for threat
        var uiManager = ChessGameManager.Instance.GetComponent<ChessUIManager>();
        yield return StartCoroutine(uiManager.ShowThreatEffect(position));
    }
    
    // Method to check if pieces can communicate based on distance
    public bool CanCommunicate(Vector2Int pos1, Vector2Int pos2)
    {
        float distance = Vector2Int.Distance(pos1, pos2);
        return distance <= communicationRange;
    }
}