using UnityEngine;
using System.Collections;

public enum PieceState
{
    Idle,
    Selected,
    Moving,
    UnderThreat,
    Captured,
    Defending
}

public class ChessPieceAgent : MonoBehaviour
{
    [Header("Agent Properties")]
    public PieceState currentState = PieceState.Idle;
    public float threatRadius = 2.0f;
    public bool isUnderThreat = false;
    
    private ChessPiece chessPiece;
    private SteeringBehavior steeringBehavior;
    private AgentCommunication communication;
    
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
     
    }

    
    private void Update()
    {
        UpdateStateMachine();
        CheckForThreats();
    }
    
    private void UpdateStateMachine()
    {
        switch (currentState)
        {
            case PieceState.Idle:
                HandleIdleState();
                break;
            case PieceState.Selected:
                HandleSelectedState();
                break;
            case PieceState.Moving:
                HandleMovingState();
                break;
            case PieceState.UnderThreat:
                HandleThreatState();
                break;
            case PieceState.Defending:
                HandleDefendingState();
                break;
        }
    }
    
    private void HandleIdleState()
    {
        var steeringBehavior = GetComponent<SteeringBehavior>();
        if (steeringBehavior != null)
        {
            steeringBehavior.StopMovement();
        
            if (Vector3.Distance(transform.localPosition, Vector3.zero) > 0.1f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * 5f);
            }
        }
    
    }

    
    private void HandleSelectedState()
    {
        steeringBehavior.ApplyHoverEffect();
    }
    
    private void HandleMovingState()
    {
        steeringBehavior.ApplyMovingEffect();
    }
    
    private void HandleThreatState()
    {
        communication.RequestDefense(chessPiece.position, chessPiece.color);
    }
    
    private void HandleDefendingState()
    {
        steeringBehavior.ApplyDefendingEffect();
    }
    
    public void SetState(PieceState newState)
    {
        currentState = newState;
    }
    
    private void CheckForThreats()
    {
        var board = ChessGameManager.Instance.GetBoard();
        var enemyColor = chessPiece.color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        
        bool wasUnderThreat = isUnderThreat;
        isUnderThreat = board.IsPositionUnderAttack(chessPiece.position, enemyColor);
        
        if (isUnderThreat && !wasUnderThreat && currentState != PieceState.Moving)
        {
            SetState(PieceState.UnderThreat);
        }
        else if (!isUnderThreat && wasUnderThreat && currentState == PieceState.UnderThreat)
        {
            SetState(PieceState.Idle);
        }
    }
    
    private void OnThreatReceived(Vector2Int threatPosition, PieceColor attackerColor)
    {
        if (chessPiece.color != attackerColor && currentState == PieceState.Idle)
        {
            float distance = Vector2Int.Distance(chessPiece.position, threatPosition);
            if (distance <= threatRadius)
            {
                SetState(PieceState.Defending);
                
            }
        }
    }
    
    private void OnDefenseRequested(Vector2Int position, PieceColor allyColor)
    {
        if (chessPiece.color == allyColor && currentState == PieceState.Idle)
        {
            float distance = Vector2Int.Distance(chessPiece.position, position);
            if (distance <= threatRadius)
            {
                SetState(PieceState.Defending);
                
            }
        }
    }
    
    private IEnumerator DefendFor(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (currentState == PieceState.Defending)
        {
            SetState(PieceState.Idle);
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