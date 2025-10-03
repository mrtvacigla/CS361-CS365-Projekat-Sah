using UnityEngine;

public class CapturedState : IChessPieceState
{
    public void OnEnterState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} entered CapturedState");
        // Logic for captured state, e.g., disable renderer, move to captured pile
        agent.StartCoroutine(agent.GetComponent<SteeringBehavior>().FadeOutAndDeactivate(0.5f)); // 0.5 seconds fade out
    }

    public void OnExitState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} exited CapturedState");
        // agent.gameObject.SetActive(true); // Re-activation is handled by game logic if piece is reused
    }

    public void HandleUpdate(ChessPieceAgent agent)
    {
        // Captured pieces don't need update logic
    }

    public void CheckThreatStatus(ChessPieceAgent agent)
    {
        // Captured pieces generally ignore threat status checks
    }

    public void OnThreatBroadcastReceived(ChessPieceAgent agent, Vector2Int threatPosition, PieceColor attackerColor)
    {
        // Captured pieces generally ignore threat broadcasts
    }

    public void OnDefenseBroadcastReceived(ChessPieceAgent agent, Vector2Int position, PieceColor allyColor)
    {
        // Captured pieces generally ignore defense requests
    }
}