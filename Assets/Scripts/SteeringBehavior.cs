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
        float hover = Mathf.Sin(Time.time * 5.0f) * 0.035f;
        Vector3 currentPos = transform.localPosition;
        transform.localPosition = new Vector3(currentPos.x, currentPos.y + hover, currentPos.z);
        
        float rotationAmount = Mathf.Sin(Time.time * 8.0f) * 10.0f; 
        //transform.localRotation = Quaternion.Euler(originalRotation.x, originalRotation.y, originalRotation.z + rotationAmount);
        
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(originalRotation.x, originalRotation.y, originalRotation.z + rotationAmount), 0.25f);
    }

    public void ApplyShakingEffect()
    {
        float rotationAmount = Mathf.Sin(Time.time * 12.0f) * 20.0f;
        transform.localRotation = Quaternion.Euler(originalRotation.x + rotationAmount , originalRotation.y , originalRotation.z);
        
        Vector3 currentPos = transform.localPosition;
        transform.localPosition = new Vector3(currentPos.x + rotationAmount * 0.01f, currentPos.y , currentPos.z);
    }
    
    public void ApplyDefendingEffect()
    {
        float rotationAmount = Mathf.Sin(Time.time * 1.0f) *5.0f;
        transform.localRotation = Quaternion.Euler(originalRotation.x + rotationAmount , originalRotation.y , originalRotation.z);
        
        Vector3 currentPos = transform.localPosition;
        transform.localPosition = new Vector3(currentPos.x + rotationAmount * 0.01f, currentPos.y , currentPos.z);
    }

    public void ApplyMovingEffect()
    {
        float rotationAmount = Mathf.Sin(Time.time * 8.0f) * 5.0f; 
        transform.localRotation = Quaternion.Euler(originalRotation.x, originalRotation.y, originalRotation.z + rotationAmount);
    }
}