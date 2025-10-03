using UnityEngine;

public interface IChessPieceState
{
    void OnEnterState(ChessPieceAgent agent);
    void OnExitState(ChessPieceAgent agent);
    void HandleUpdate(ChessPieceAgent agent);
    void CheckThreatStatus(ChessPieceAgent agent);
    void OnThreatBroadcastReceived(ChessPieceAgent agent, Vector2Int threatPosition, PieceColor attackerColor);
    void OnDefenseBroadcastReceived(ChessPieceAgent agent, Vector2Int position, PieceColor allyColor);
}