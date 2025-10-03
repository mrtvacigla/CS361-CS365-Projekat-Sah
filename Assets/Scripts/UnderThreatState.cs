using UnityEngine;

public class UnderThreatState : IChessPieceState
{
    public void OnEnterState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} entered UnderThreatState");
        // agent.communication.RequestDefense(agent.chessPiece.position, agent.chessPiece.color); // Removed: Threat state should not immediately request defense
        agent.isUnderThreat = true;
    }

    public void OnExitState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} exited UnderThreatState");
        // Reset shaking effect if needed
        agent.GetComponent<SteeringBehavior>().ResetShakingEffect();
        agent.isUnderThreat = false;
    }

    public void HandleUpdate(ChessPieceAgent agent)
    {
        agent.GetComponent<SteeringBehavior>().ApplyShakingEffect();
    }

    public void CheckThreatStatus(ChessPieceAgent agent)
    {
        var board = ChessGameManager.Instance.GetBoard();
        var enemyColor = agent.chessPiece.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        
        if (!board.IsPositionUnderAttack(agent.chessPiece.position, enemyColor))
        {
            agent.SetState(new IdleState());
        }
    }

    public void OnThreatBroadcastReceived(ChessPieceAgent agent, Vector2Int threatPosition, PieceColor attackerColor)
    {
        // Already under threat, no change needed unless it's a different, more severe threat (not implemented here)
    }

    public void OnDefenseBroadcastReceived(ChessPieceAgent agent, Vector2Int position, PieceColor allyColor)
    {
        if (agent.chessPiece.color == allyColor)
        {
            float distance = Vector2Int.Distance(agent.chessPiece.position, position);
            if (distance <= agent.threatRadius)
            {
                agent.SetState(new DefendingState());
            }
        }
    }
}