using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Redworm : Collectible
{
    public enum RedwormSpeed { Slow, Medium, Fast }
    
    [Header("Redworm Settings")]
    public RedwormSpeed speedType = RedwormSpeed.Slow;
    public float movementSpeed;
    public float penaltyTime;  // Time penalty when hooked
    public bool causesStun;    // Whether it causes stun effect
    
    private Vector3 moveDirection;
    private float boundaryWidth = 8.0f;  // Match this with GameManager's spawnAreaWidth
    private float boundaryHeight = 6.0f; // Match this with camera's vertical boundary
    private float timeSinceDirectionChange = 0f;
    private float directionChangeInterval = 2f;
    private float stuckCheckInterval = 0.5f;
    private float stuckCheckTimer = 0f;
    private Vector3 lastPosition;
    private bool potentiallyStuck = false;
    private int consecStuckFrames = 0;
    private int stuckThreshold = 3;
    
    void Start()
    {
        // Set properties based on speed variant
        switch (speedType)
        {
            case RedwormSpeed.Slow:
                movementSpeed = 1f;
                penaltyTime = 1f;
                causesStun = false;
                value = 0;  // Neutral value
                weight = 5.0f;
                break;
                
            case RedwormSpeed.Medium:
                movementSpeed = 2f;
                penaltyTime = 2f;
                causesStun = false;
                value = -10;  // Minor penalty
                weight = 5.0f;
                break;
                
            case RedwormSpeed.Fast:
                movementSpeed = 4f;
                penaltyTime = 3f;
                causesStun = true;
                value = -20;  // Major penalty
                weight = 5.0f;
                break;
        }
        
        // Initialize random direction
        ChangeDirection();
        
        // Set redworm appearance
        if (GetComponent<Renderer>() != null)
        {
            // Color varies by speed variant
            Color wormColor = Color.red;
            switch (speedType)
            {
                case RedwormSpeed.Slow:
                    wormColor = new Color(0.7f, 0.2f, 0.2f); // Dark red
                    break;
                case RedwormSpeed.Medium:
                    wormColor = new Color(0.9f, 0.3f, 0.3f); // Medium red
                    break;
                case RedwormSpeed.Fast:
                    wormColor = new Color(1f, 0.4f, 0.4f); // Bright red
                    break;
            }
            GetComponent<Renderer>().material.color = wormColor;
        }
        
        // Make the redworm more elongated horizontally
        transform.localScale = new Vector3(
            transform.localScale.x * 1.5f, 
            transform.localScale.y * 0.7f, 
            transform.localScale.z
        );
    }
    
    void Update()
    {
        // Only move if the game is running
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Running)
            return;
            
        // Move the redworm
        transform.position += moveDirection * movementSpeed * Time.deltaTime;
        
        // Change direction periodically or at boundaries
        timeSinceDirectionChange += Time.deltaTime;
        if (timeSinceDirectionChange > directionChangeInterval || IsAtBoundary())
        {
            ChangeDirection();
            timeSinceDirectionChange = 0f;
        }
        
        // Check if stuck
        stuckCheckTimer += Time.deltaTime;
        if (stuckCheckTimer >= stuckCheckInterval)
        {
            CheckIfStuck();
            stuckCheckTimer = 0f;
        }
    }
    
    void ChangeDirection()
    {
        // If at a boundary, move back toward center
        if (IsAtBoundary())
        {
            Vector3 toCenter = -transform.position.normalized;
            // Add some randomness so it doesn't move in a straight line
            float randomAngle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
            Vector3 randomDir = new Vector3(
                Mathf.Cos(randomAngle) * toCenter.x - Mathf.Sin(randomAngle) * toCenter.y,
                Mathf.Sin(randomAngle) * toCenter.x + Mathf.Cos(randomAngle) * toCenter.y,
                0
            );
            moveDirection = randomDir.normalized;
        }
        else
        {
            // Pick a random 2D direction
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            moveDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;
        }
    }
    
    bool IsAtBoundary()
    {
        // Check if near the edge of the game area (both X and Y axes)
        float halfWidth = boundaryWidth * 0.5f;
        float halfHeight = boundaryHeight * 0.5f;
        return (transform.position.x > halfWidth || transform.position.x < -halfWidth ||
                transform.position.y > halfHeight || transform.position.y < -halfHeight);
    }
    
    void CheckIfStuck()
    {
        // If we're close to the same position after a check interval, we might be stuck
        if (lastPosition != Vector3.zero)
        {
            float distance = Vector3.Distance(transform.position, lastPosition);
            
            // If barely moved, count as potentially stuck
            if (distance < 0.05f)
            {
                consecStuckFrames++;
                if (consecStuckFrames >= stuckThreshold)
                {
                    // We're definitely stuck, force a new direction toward center
                    Vector3 toCenter = Vector3.zero - transform.position;
                    moveDirection = toCenter.normalized;
                    consecStuckFrames = 0;
                }
            }
            else
            {
                // Reset counter since we're moving fine
                consecStuckFrames = 0;
            }
        }
        
        // Remember position for next check
        lastPosition = transform.position;
    }
    
    // Override the AttachToHook method to apply penalty
    public override void AttachToHook(Transform hookTransform)
    {
        // Apply penalty
        StartCoroutine(ApplyHookPenalty());
        
        // We don't parent to the hook - worm escapes!
        GameManager.Instance.AddScore(value); // Apply score change (could be negative)
        
        // Destroy self after penalty applied
        Destroy(gameObject, 0.2f);
    }
    
    private IEnumerator ApplyHookPenalty()
    {
        // Find the PlayerController
        PlayerController playerController = FindObjectOfType<PlayerController>();
        
        if (playerController != null)
        {
            // Display visual feedback
            // Future enhancement: Add visual effect for player penalty
            
            // Cause stun effect on harder variants
            if (causesStun)
            {
                // Future enhancement: Implement stun mechanic
                Debug.Log("Player stunned by fast redworm!");
            }
            
            Debug.Log($"Redworm caused {penaltyTime}s time penalty!");
        }
        
        // Short delay before destroying
        yield return new WaitForSeconds(0.2f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Display movement speed in editor
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            float displaySpeed = 0;
            switch (speedType)
            {
                case RedwormSpeed.Slow: displaySpeed = 1f; break;
                case RedwormSpeed.Medium: displaySpeed = 2f; break;
                case RedwormSpeed.Fast: displaySpeed = 4f; break;
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up, "Speed: " + displaySpeed);
        }
        #endif
    }
}
