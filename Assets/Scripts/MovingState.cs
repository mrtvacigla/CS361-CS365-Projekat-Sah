using UnityEngine;

public class MovingState : IChessPieceState
{
    public void OnEnterState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} entered MovingState");
        // agent.GetComponent<SteeringBehavior>().ApplyMovingEffect(); // Moved to HandleUpdate
    }

    public void OnExitState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} exited MovingState");
        // Reset moving effect if needed
        agent.GetComponent<SteeringBehavior>().ResetMovingEffect();
    }

    public void HandleUpdate(ChessPieceAgent agent)
    {
        agent.GetComponent<SteeringBehavior>().ApplyMovingEffect();
    }

    public void CheckThreatStatus(ChessPieceAgent agent)
    {
        // Moving pieces generally ignore threat status checks
    }

    public void OnThreatBroadcastReceived(ChessPieceAgent agent, Vector2Int threatPosition, PieceColor attackerColor)
    {
        // Moving pieces generally ignore threat broadcasts
    }

    public void OnDefenseBroadcastReceived(ChessPieceAgent agent, Vector2Int position, PieceColor allyColor)
    {
        // Moving pieces generally ignore defense requests
    }
}