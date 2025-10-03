using UnityEngine;

public class IdleState : IChessPieceState
{
    public void OnEnterState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} entered IdleState");
        var steeringBehavior = agent.GetComponent<SteeringBehavior>();
        if (steeringBehavior != null)
        {
            steeringBehavior.StopMovement();
        }
        agent.isUnderThreat = false;
    }

    public void OnExitState(ChessPieceAgent agent)
    {
        // Debug.Log($"{agent.name} exited IdleState");
    }

    public void HandleUpdate(ChessPieceAgent agent)
    {
        // Logic from HandleIdleState()
        var steeringBehavior = agent.GetComponent<SteeringBehavior>();
        if (steeringBehavior != null)
        {
            if (Vector3.Distance(agent.transform.localPosition, Vector3.zero) > 0.1f)
            {
                agent.transform.localPosition = Vector3.Lerp(agent.transform.localPosition, Vector3.zero, Time.deltaTime * 5f);
            }
            agent.transform.localRotation = TwoPlayerManager.CurrentPieceCorrection;
        }
    }

    public void CheckThreatStatus(ChessPieceAgent agent)
    {
        var board = ChessGameManager.Instance.GetBoard();
        var enemyColor = agent.chessPiece.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        
        if (board.IsPositionUnderAttack(agent.chessPiece.position, enemyColor))
        {
            agent.SetState(new UnderThreatState());
        }
    }

    public void OnThreatBroadcastReceived(ChessPieceAgent agent, Vector2Int threatPosition, PieceColor attackerColor)
    {
        if (agent.chessPiece.color != attackerColor)
        {
            float distance = Vector2Int.Distance(agent.chessPiece.position, threatPosition);
            if (distance <= agent.threatRadius)
            {
                agent.SetState(new UnderThreatState());
            }
        }
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