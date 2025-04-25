using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Add this for Canvas support

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
    private enum HookState { Swinging, Extending, Retracting, Pulling, Broken }
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
    
    // Rope stress penalties
    private float hookStrengthPenalty = 0f;
    private float hookSpeedPenalty = 0f;
    private float penaltyDecayRate = 0.05f; // How quickly penalties fade
    private RopeManager ropeManager;

    void Start()
    {
        // Create hook object if not assigned
        if (hook == null)
        {
            hook = Instantiate(hookPrefab, transform.position + Vector3.down, Quaternion.identity);
            hook.transform.parent = transform;
            
            // Set the hook tag for detection by stress objects
            hook.tag = "Hook";
        }
        
        hookStartPosition = transform.position;
        
        // Initialize line renderer if assigned
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hook.transform.position);
        }
        
        // Find RopeManager
        ropeManager = FindObjectOfType<RopeManager>();
    }

    void Update()
    {
        // Decay penalties over time
        DecayPenalties();
        
        // Only process hook logic if the game is running
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Running)
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
                    
                case HookState.Broken:
                    // Do nothing while rope is broken
                    break;
            }
        }
        
        // Always update line renderer positions regardless of game state
        if (lineRenderer != null)
        {
            // Ensure the line renderer is always enabled unless we're in broken state
            lineRenderer.enabled = (currentState != HookState.Broken);
            
            // Update the line positions
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
        // Apply hook speed penalty from stress
        float adjustedHookSpeed = hookSpeed * (1f - hookSpeedPenalty);
        
        // Move hook in the direction it was pointing
        hook.transform.position += hookDirection * adjustedHookSpeed * Time.deltaTime;
        currentHookDistance = Vector3.Distance(transform.position, hook.transform.position);
        
        // Apply hook strength penalty from stress
        float adjustedMaxDistance = maxHookDistance * (1f - hookStrengthPenalty);
        
        // Check if hook has reached maximum distance
        if (currentHookDistance >= adjustedMaxDistance)
        {
            currentState = HookState.Retracting;
        }
    }
    
    void RetractHook()
    {
        // Apply hook speed penalty from stress
        float adjustedHookSpeed = hookSpeed * (1f - hookSpeedPenalty);
        
        // Move hook back to player
        Vector3 direction = (transform.position - hook.transform.position).normalized;
        hook.transform.position += direction * adjustedHookSpeed * Time.deltaTime;
        
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
            // Ensure line renderer is visible
            if (lineRenderer != null && !lineRenderer.enabled)
            {
                lineRenderer.enabled = true;
                Debug.Log("Re-enabled line renderer during pull");
            }
            
            // Move caught object and hook toward player
            Vector3 direction = (transform.position - hook.transform.position).normalized;
            
            // Better formula for weight-based speed:
            // - For light objects (<10): faster speeds with minimal weight impact
            // - For medium objects (10-20): moderate reduction in speed
            // - For heavy objects (>20): significant reduction but still playable
            float baseSpeed = pullSpeed * (1f - hookSpeedPenalty); // Apply stress penalty
            float weight = caughtObject.weight;
            
            float speed;
            if (weight < 10) {
                // Light objects: Linear relationship (minor slowdown)
                speed = baseSpeed * (1.2f - (weight / 20f));
            } else if (weight < 20) {
                // Medium objects: Sqrt relationship (moderate slowdown)
                speed = baseSpeed * (0.9f - (Mathf.Sqrt(weight - 9) / 10f));
            } else {
                // Heavy objects: More dramatic slowdown but with a minimum
                speed = baseSpeed * (0.6f - ((weight - 20) / 100f));
            }
            
            // Always ensure a minimum speed so objects never feel stuck
            speed = Mathf.Max(speed, baseSpeed * 0.4f); // Increased minimum speed from 0.3f to 0.4f
            
            // Log pull info for debugging
            if (Time.frameCount % 60 == 0) // Log once per second approximately
            {
                Debug.Log($"Pulling: Weight={weight}, Speed={speed}, Distance={Vector3.Distance(transform.position, hook.transform.position)}, State={currentState}");
            }
            
            // Move the hook toward the player
            hook.transform.position += direction * speed * Time.deltaTime;
            
            // The collectible will automatically follow the hook since we've parented it
            
            // Add stress to rope from pulling the object, but ONLY if we have a rope manager
            // This makes the rope stress system optional
            if (ropeManager != null)
            {
                try {
                    // Cap the weight value sent to stress calculation to prevent excessive stress
                    float cappedWeight = Mathf.Min(weight, 25f);
                    ropeManager.AddPullStress(cappedWeight);
                }
                catch (System.Exception e) {
                    // If there's any error with the rope manager, log it but don't let it affect gameplay
                    Debug.LogError("Error applying pull stress: " + e.Message);
                    // Continue gameplay even if rope manager fails
                }
            }
            
            // Check if object is close enough to player
            if (Vector3.Distance(transform.position, hook.transform.position) < 1.0f)
            {
                try
            {
                // Add score with value multiplier applied
                    if (GameManager.Instance != null)
                    {
                int adjustedValue = Mathf.RoundToInt(caughtObject.value * Collectible.valueMultiplier);
                GameManager.Instance.AddScore(adjustedValue);
                    }
                
                // Detach the object before destroying it
                caughtObject.Detach();
                
                // Destroy caught object
                Destroy(caughtObject.gameObject);
                caughtObject = null;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error processing collected object: " + e.Message);
                    caughtObject = null; // Ensure we don't get stuck
                }
                finally
                {
                    // Reset hook state regardless of any errors
                currentState = HookState.Swinging;
                hook.transform.position = transform.position + Vector3.down;
                }
            }
        }
        else
        {
            // If somehow we lost our object, just retract
            Debug.LogWarning("PullObject called but caughtObject is null. Switching to Retracting state.");
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
    
    // Apply stress penalties from rope stress objects
    public void ApplyStressPenalties(float strengthPenalty, float speedPenalty, bool causeBreak)
    {
        // Only apply penalties if they're within reasonable limits
        if (strengthPenalty >= 0 && strengthPenalty <= 0.5f && 
            speedPenalty >= 0 && speedPenalty <= 0.5f) 
        {
            // Accumulate penalties (caps at 0.9 or 90%)
            hookStrengthPenalty = Mathf.Min(0.9f, hookStrengthPenalty + strengthPenalty);
            hookSpeedPenalty = Mathf.Min(0.9f, hookSpeedPenalty + speedPenalty);
            
            Debug.Log($"Applied penalties: Strength -{hookStrengthPenalty*100}%, Speed -{hookSpeedPenalty*100}%");
            
            // If it causes a break and we're not already broken
            if (causeBreak && currentState != HookState.Broken)
            {
                OnRopeBreak();
            }
        }
        else
        {
            Debug.LogWarning($"Ignoring invalid stress penalties: Strength={strengthPenalty}, Speed={speedPenalty}");
        }
    }
    
    // Handle rope breaking 
    public void OnRopeBreak()
    {
        if (currentState != HookState.Broken)
        {
            Debug.Log("Rope broke! Waiting for repair...");
            
            // Save previous state to restore after repair
            HookState previousState = currentState;
            
            // Set to broken state
            currentState = HookState.Broken;
            
            // Create floating text at the hook position
            ShowRopeBreakText();
            
            // Drop any caught object
            if (caughtObject != null)
            {
                caughtObject.Detach();
                caughtObject = null;  // Don't destroy the object, just detach it
            }
            
            // Visual feedback - make rope invisible
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
            
            // Start auto-repair after delay if needed
            StartCoroutine(AutoRepairAfterDelay(3.0f));
        }
    }
    
    // Show floating text when rope breaks
    private void ShowRopeBreakText()
    {
        if (hook != null)
        {
            // Try to find the main canvas
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas != null)
            {
                // Find main camera
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    // Create floating text that says "Rope Broke!"
                    FloatingText.CreateWorldToUI(hook.transform.position, mainCanvas, mainCamera, "Rope Broke!");
                }
            }
        }
    }
    
    private IEnumerator AutoRepairAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnRopeRepaired();
    }
    
    // Handle rope repair
    public void OnRopeRepaired()
    {
        Debug.Log("Rope repaired! Resuming operation.");
        
        // Restore rope renderer
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
        
        // Reset hook position
        hook.transform.position = transform.position + Vector3.down;
        
        // Return to swinging state
        currentState = HookState.Swinging;
    }
    
    // Gradually reduce penalties over time
    private void DecayPenalties()
    {
        if (hookStrengthPenalty > 0)
        {
            hookStrengthPenalty = Mathf.Max(0, hookStrengthPenalty - (penaltyDecayRate * Time.deltaTime));
        }
        
        if (hookSpeedPenalty > 0)
        {
            hookSpeedPenalty = Mathf.Max(0, hookSpeedPenalty - (penaltyDecayRate * Time.deltaTime));
        }
    }
}
