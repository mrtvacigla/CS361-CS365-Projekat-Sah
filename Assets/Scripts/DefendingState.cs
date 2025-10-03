using UnityEngine;

public class DefendingState : IChessPieceState
{
    public void OnEnterState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} entered DefendingState");
        agent.GetComponent<SteeringBehavior>().ApplyDefendingEffect();
    }

    public void OnExitState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} exited DefendingState");
        // Reset defending effect if needed
        agent.GetComponent<SteeringBehavior>().ResetDefendingEffect();
    }

    public void HandleUpdate(ChessPieceAgent agent)
    {
        agent.GetComponent<SteeringBehavior>().ApplyDefendingEffect();
    }

    public void CheckThreatStatus(ChessPieceAgent agent)
    {
        // While defending, we don't change state based on threat status from here
    }

    public void OnThreatBroadcastReceived(ChessPieceAgent agent, Vector2Int threatPosition, PieceColor attackerColor)
    {
        // Already defending, no change needed
    }

    public void OnDefenseBroadcastReceived(ChessPieceAgent agent, Vector2Int position, PieceColor allyColor)
    {
        // Already defending, no change needed
    }
}