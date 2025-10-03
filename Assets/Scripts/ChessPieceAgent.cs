using UnityEngine;
using System.Collections;

public class ChessPieceAgent : MonoBehaviour
{
    [Header("Agent Properties")]
    public float threatRadius = 2.0f;
    public bool isUnderThreat = false;
    
    public ChessPiece chessPiece { get; private set; }
    public SteeringBehavior steeringBehavior { get; private set; }
    public AgentCommunication communication { get; private set; }
    
    private IChessPieceState _currentState;

    private void Awake()
    {
        StartCoroutine(WaitForLoading());
    }

    public IEnumerator WaitForLoading()
    {
        yield return new WaitForSeconds(1f);
        chessPiece = GetComponent<ChessPiece>();
        steeringBehavior = GetComponent<SteeringBehavior>();
    
        communication = FindObjectOfType<AgentCommunication>();
   
        communication.OnThreatBroadcast += OnThreatReceived;
        communication.OnDefenseRequest += OnDefenseRequested;

        // Set initial state
        SetState(new IdleState());
    }

    
    private void Update()
    {
        _currentState?.HandleUpdate(this);
        _currentState?.CheckThreatStatus(this);
    }
    
    public void SetState(IChessPieceState newState)
    {
        _currentState?.OnExitState(this);
        _currentState = newState;
        _currentState.OnEnterState(this);
    }
    

    private void OnThreatReceived(Vector2Int threatPosition, PieceColor attackerColor)
    {
        _currentState?.OnThreatBroadcastReceived(this, threatPosition, attackerColor);
    }
    
    private void OnDefenseRequested(Vector2Int position, PieceColor allyColor)
    {
        _currentState?.OnDefenseBroadcastReceived(this, position, allyColor);
    }
    
    private IEnumerator DefendFor(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (_currentState is DefendingState)
        {
            SetState(new IdleState());
        }
    }
    
    private void OnDestroy()
    {
        if (communication != null)
        {
            communication.OnThreatBroadcast -= OnThreatReceived;
            communication.OnDefenseRequest -= OnDefenseRequested;
        }
    }
}