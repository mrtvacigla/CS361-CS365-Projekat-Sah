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
        OnThreatBroadcast?.Invoke(position, attackerColor);
        StartCoroutine(ShowThreatIndicator(position));
    }
    
    public void RequestDefense(Vector2Int position, PieceColor allyColor)
    {
        OnDefenseRequest?.Invoke(position, allyColor);
    }
    
    public void SecurePosition(Vector2Int position, PieceColor controllingColor)
    {
        OnPositionSecured?.Invoke(position, controllingColor);
    }
    
    private System.Collections.IEnumerator ShowThreatIndicator(Vector2Int position)
    {
        var uiManager = ChessGameManager.Instance.GetComponent<ChessUIManager>();
        yield return StartCoroutine(uiManager.ShowThreatEffect(position));
    }
    
    public bool CanCommunicate(Vector2Int pos1, Vector2Int pos2)
    {
        float distance = Vector2Int.Distance(pos1, pos2);
        return distance <= communicationRange;
    }
}