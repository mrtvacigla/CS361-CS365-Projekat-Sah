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
    
    private Vector3 originalRotation;
    private bool hasStoredOriginalRotation = false;
    
    private void Start()
    {
        originalRotation = transform.localRotation.eulerAngles;
        hasStoredOriginalRotation = true;
    }
    
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
        
        if (hasStoredOriginalRotation)
        {
            transform.localRotation = Quaternion.Euler(originalRotation);
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
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
    
        float rotationAmount = Mathf.Sin(Time.time * 8.0f) * 10.0f;
        Vector3 targetRotation = originalRotation + Vector3.forward * rotationAmount;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(targetRotation), 0.25f);
    }

    public void ApplyShakingEffect()
    {
        float rotationAmount = Mathf.Sin(Time.time * 40.0f) * 1.5f;
        Vector3 targetRotation = originalRotation + Vector3.forward * rotationAmount;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(targetRotation), 0.25f);
    
        /*
        float swingAmount = Mathf.Sin(Time.time * 40.0f) * 0.025f; 
        Vector3 targetPos = Vector3.zero + Vector3.right * swingAmount;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, 0.25f);
        */
    }

    public void ApplyDefendingEffect()
    {
        /*
        float rotationAmount = Mathf.Sin(Time.time * 1.0f) * 15.0f;
        Vector3 targetRotation = originalRotation + Vector3.right * rotationAmount; 
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(targetRotation), 0.25f);
        */
        float swingAmount = Mathf.Cos(Time.time * 5.0f) * 1f; 
        Vector3 targetPos = Vector3.zero + Vector3.right * swingAmount;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, 0.5f);
    }

    public void ApplyMovingEffect()
    {
        float rotationAmount = Mathf.Sin(Time.time * 8.0f) * 5.0f;
        Vector3 targetRotation = originalRotation + Vector3.forward * rotationAmount;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(targetRotation), 0.25f);
    }
}