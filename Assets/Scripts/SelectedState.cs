using UnityEngine;

public class SelectedState : IChessPieceState
{
    public void OnEnterState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} entered SelectedState");
        // agent.GetComponent<SteeringBehavior>().ApplyHoverEffect(); // Moved to HandleUpdate
    }

    public void OnExitState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} exited SelectedState");
        // Reset hover effect if needed
        agent.GetComponent<SteeringBehavior>().ResetHoverEffect();
    }

    public void HandleUpdate(ChessPieceAgent agent)
    {
        agent.GetComponent<SteeringBehavior>().ApplyHoverEffect();
    }

    public void CheckThreatStatus(ChessPieceAgent agent)
    {
        // Selected pieces generally ignore threat status checks
    }

    public void OnThreatBroadcastReceived(ChessPieceAgent agent, Vector2Int threatPosition, PieceColor attackerColor)
    {
        // Selected pieces generally ignore threat broadcasts
    }

    public void OnDefenseBroadcastReceived(ChessPieceAgent agent, Vector2Int position, PieceColor allyColor)
    {
        // Selected pieces generally ignore defense requests
    }
}