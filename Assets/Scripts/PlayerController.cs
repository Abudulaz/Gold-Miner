using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Hook Settings")]
    public GameObject hookPrefab;
    public float swingSpeed = 2.0f;
    public float hookSpeed = 5.0f;
    public float maxHookDistance = 10.0f;
    public float pullSpeed = 3.0f;

    [Header("References")]
    public LineRenderer lineRenderer;

    // Hook state
    private enum HookState { Swinging, Extending, Retracting, Pulling }
    private HookState currentState = HookState.Swinging;
    
    // Hook position and rotation
    private GameObject hook;
    private float swingAngle = 0f;
    private float swingDirection = 1f;
    private float maxSwingAngle = 80f;
    private Vector3 hookStartPosition;
    private Vector3 hookDirection;
    private float currentHookDistance = 0f;
    private Collectible caughtObject = null;

    void Start()
    {
        // Create hook object if not assigned
        if (hook == null)
        {
            hook = Instantiate(hookPrefab, transform.position + Vector3.down, Quaternion.identity);
            hook.transform.parent = transform;
        }
        
        hookStartPosition = transform.position;
        
        // Initialize line renderer if assigned
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hook.transform.position);
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case HookState.Swinging:
                SwingHook();
                // Check for left mouse button click to extend hook
                if (Input.GetMouseButtonDown(0))
                {
                    currentState = HookState.Extending;
                    hookDirection = (hook.transform.position - transform.position).normalized;
                }
                break;
                
            case HookState.Extending:
                ExtendHook();
                break;
                
            case HookState.Retracting:
                RetractHook();
                break;
                
            case HookState.Pulling:
                PullObject();
                break;
        }
        
        // Update line renderer positions
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hook.transform.position);
        }
    }
    
    void SwingHook()
    {
        // Calculate swing angle
        swingAngle += swingSpeed * swingDirection * Time.deltaTime;
        
        // Reverse direction when reaching max angle
        if (Mathf.Abs(swingAngle) >= maxSwingAngle)
        {
            swingDirection *= -1;
        }
        
        // Calculate hook position based on swing angle
        float x = Mathf.Sin(swingAngle * Mathf.Deg2Rad);
        float y = -Mathf.Cos(swingAngle * Mathf.Deg2Rad);
        
        Vector3 newPosition = transform.position + new Vector3(x, y, 0) * 1.5f;
        hook.transform.position = newPosition;
    }
    
    void ExtendHook()
    {
        // Move hook in the direction it was pointing
        hook.transform.position += hookDirection * hookSpeed * Time.deltaTime;
        currentHookDistance = Vector3.Distance(transform.position, hook.transform.position);
        
        // Check if hook has reached maximum distance
        if (currentHookDistance >= maxHookDistance)
        {
            currentState = HookState.Retracting;
        }
    }
    
    void RetractHook()
    {
        // Move hook back to player
        Vector3 direction = (transform.position - hook.transform.position).normalized;
        hook.transform.position += direction * hookSpeed * Time.deltaTime;
        
        // Check if hook is close enough to player
        if (Vector3.Distance(transform.position, hook.transform.position) < 1.0f)
        {
            currentState = HookState.Swinging;
            hook.transform.position = transform.position + Vector3.down;
        }
    }
    
    void PullObject()
    {
        if (caughtObject != null)
        {
            // Move caught object and hook toward player
            Vector3 direction = (transform.position - hook.transform.position).normalized;
            float speed = pullSpeed / caughtObject.weight;
            
            hook.transform.position += direction * speed * Time.deltaTime;
            
            // The collectible will automatically follow the hook since we've parented it
            
            // Check if object is close enough to player
            if (Vector3.Distance(transform.position, hook.transform.position) < 1.0f)
            {
                // Add score
                GameManager.Instance.AddScore(caughtObject.value);
                
                // Detach the object before destroying it
                caughtObject.Detach();
                
                // Destroy caught object
                Destroy(caughtObject.gameObject);
                caughtObject = null;
                
                // Reset hook state
                currentState = HookState.Swinging;
                hook.transform.position = transform.position + Vector3.down;
            }
        }
        else
        {
            // If somehow we lost our object, just retract
            currentState = HookState.Retracting;
        }
    }
    
    // Called when hook collides with collectible
    public void CatchObject(Collectible obj)
    {
        Debug.Log("CatchObject called with: " + obj.gameObject.name + ", current state: " + currentState);
        
        if (currentState == HookState.Extending)
        {
            Debug.Log("Setting caught object and changing state to Pulling");
            caughtObject = obj;
            
            // Attach the object to the hook
            obj.AttachToHook(hook.transform);
            
            // Change state to pulling
            currentState = HookState.Pulling;
        }
        else
        {
            Debug.Log("Not in Extending state, ignoring catch. Current state: " + currentState);
        }
    }
}
