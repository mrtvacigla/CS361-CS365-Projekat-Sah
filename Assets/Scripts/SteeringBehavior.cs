using UnityEngine;
using System.Collections;

public class SteeringBehavior : MonoBehaviour
{
    [Header("Steering Properties")]
    public float maxSpeed = 5.0f;
    public float maxForce = 3.0f;
    public float arriveRadius = 0.5f;
    public float slowRadius = 1.5f;
    
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private bool isMoving = false;
    
    private void Update()
    {
        if (isMoving)
        {
            ApplySteering();
        }
    }

    public void StopMovement()
    {
        isMoving = false;
        velocity = Vector3.zero;
    }

    public void ResetToCenter()
    {
        StopMovement();
        transform.localPosition = Vector3.zero;
        
        // Koristi korekciju iz TwoPlayerManager-a bez obzira na mod igre, ako je tabla rotirana
        transform.localRotation = TwoPlayerManager.CurrentPieceCorrection;
    }
    
    public IEnumerator MoveTo(Vector3 target)
    {
        targetPosition = target;
        isMoving = true;
        
        while (Vector3.Distance(transform.position, targetPosition) > arriveRadius)
        {
            yield return null;
        }
        
        isMoving = false;
        velocity = Vector3.zero;
        transform.position = targetPosition;
    }
    
    private void ApplySteering()
    {
        Vector3 desired = Arrive(targetPosition);
        Vector3 steer = Vector3.ClampMagnitude(desired - velocity, maxForce);
        
        velocity = Vector3.ClampMagnitude(velocity + steer, maxSpeed);
        transform.position += velocity * Time.deltaTime;
    }
    
    private Vector3 Arrive(Vector3 target)
    {
        Vector3 desired = target - transform.position;
        float distance = desired.magnitude;
        
        if (distance < slowRadius)
        {
            desired = desired.normalized * (maxSpeed * (distance / slowRadius));
        }
        else
        {
            desired = desired.normalized * maxSpeed;
        }
        
        return desired;
    }
    
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
    }
    
    public void ApplyHoverEffect()
    {
        float hover = Mathf.Sin(Time.time * 5.0f) * 5f;
        Vector3 targetPos = Vector3.zero + Vector3.up * hover;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, 0.25f);
    
        // Koristi korekciju iz TwoPlayerManager-a kao osnovu bez obzira na mod igre
        Quaternion baseRotation = TwoPlayerManager.CurrentPieceCorrection;
                                  
        float rotationAmount = Mathf.Sin(Time.time * 8.0f) * 10.0f;
        Quaternion hoverRotation = Quaternion.Euler(0, 0, rotationAmount);
        
        transform.localRotation = Quaternion.Lerp(transform.localRotation, baseRotation * hoverRotation, 0.25f);
    }

    public void ApplyShakingEffect()
    {
        // Koristi korekciju iz TwoPlayerManager-a kao osnovu bez obzira na mod igre
        Quaternion baseRotation = TwoPlayerManager.CurrentPieceCorrection;
                                  
        float rotationAmount = Mathf.Sin(Time.time * 40.0f) * 1.5f;
        Quaternion shakeRotation = Quaternion.Euler(0, 0, rotationAmount);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, baseRotation * shakeRotation, 0.25f);
    }

    public void ApplyDefendingEffect()
    {
        float swingAmount = Mathf.Cos(Time.time * 5.0f) * 1f; 
        Vector3 targetPos = Vector3.zero + Vector3.right * swingAmount;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, 0.5f);
    }

    public void ApplyMovingEffect()
    {
        // Koristi korekciju iz TwoPlayerManager-a kao osnovu bez obzira na mod igre
        Quaternion baseRotation = TwoPlayerManager.CurrentPieceCorrection;
                                  
        float rotationAmount = Mathf.Sin(Time.time * 8.0f) * 5.0f;
        Quaternion movingRotation = Quaternion.Euler(0, 0, rotationAmount);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, baseRotation * movingRotation, 0.25f);
    }
}