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
    public GameObject explosionPrefab; // Added for explosion effect
    
    private Vector3 moveDirection;
    private float boundaryWidth;      // Will match GameManager's spawnAreaWidth
    private float boundaryHeight;     // Will match GameManager's spawnAreaHeight
    private float boundaryYOffset;    // Will match GameManager's spawnAreaYOffset
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
        // Get boundary values from GameManager
        if (GameManager.Instance != null)
        {
            boundaryWidth = GameManager.Instance.spawnAreaWidth;
            boundaryHeight = GameManager.Instance.spawnAreaHeight;
            boundaryYOffset = GameManager.Instance.spawnAreaYOffset;
        }
        else
        {
            // Default values if GameManager is not available
            boundaryWidth = 8.0f;
            boundaryHeight = 4.0f;
            boundaryYOffset = -3.0f;
        }
        
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
        
        // Ensure we have a 3D collider set as trigger
        if (GetComponent<Collider>() == null)
        {
            // Add a box collider by default to match the elongated shape
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 0.5f, 0.5f); // Match the elongated shape
            collider.isTrigger = true;
        }
        else
        {
            // Make sure existing collider is set as trigger
            Collider existingCollider = GetComponent<Collider>();
            existingCollider.isTrigger = true;
        }
    }
    
    void Update()
    {
        // Only move if the game is running
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Running)
            return;
            
        // Check if we're at or beyond boundary before moving
        if (IsAtBoundary())
        {
            ChangeDirection();
        }
            
        // Move the redworm
        transform.position += moveDirection * movementSpeed * Time.deltaTime;
        
        // Change direction periodically
        timeSinceDirectionChange += Time.deltaTime;
        if (timeSinceDirectionChange > directionChangeInterval)
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
            // Calculate direction toward center of spawn area
            Vector3 spawnAreaCenter = new Vector3(0, boundaryYOffset, 0);
            Vector3 toCenter = spawnAreaCenter - transform.position;
            
            // Add some randomness so it doesn't move in a straight line
            float randomAngle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
            Vector3 randomDir = new Vector3(
                Mathf.Cos(randomAngle) * toCenter.x - Mathf.Sin(randomAngle) * toCenter.y,
                Mathf.Sin(randomAngle) * toCenter.x + Mathf.Cos(randomAngle) * toCenter.y,
                0
            );
            moveDirection = randomDir.normalized;
            
            // Debug boundary hit
            Debug.Log($"Redworm hit boundary at {transform.position}, redirecting to center");
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
        // Calculate spawn area boundaries considering Y offset
        float halfWidth = boundaryWidth * 0.5f;
        float halfHeight = boundaryHeight * 0.5f;
        float minX = -halfWidth;
        float maxX = halfWidth;
        float minY = boundaryYOffset - halfHeight;
        float maxY = boundaryYOffset + halfHeight;
        
        // Add a small buffer (0.1f) to turn around before actually hitting the edge
        bool outsideX = transform.position.x > maxX - 0.1f || transform.position.x < minX + 0.1f;
        bool outsideY = transform.position.y > maxY - 0.1f || transform.position.y < minY + 0.1f;
        
        return outsideX || outsideY;
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
                    Vector3 spawnAreaCenter = new Vector3(0, boundaryYOffset, 0);
                    Vector3 toCenter = spawnAreaCenter - transform.position;
                    moveDirection = toCenter.normalized;
                    consecStuckFrames = 0;
                    Debug.Log("Redworm stuck, forcing move to center");
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
    
    // Use OnTriggerEnter for hook collision detection (3D version)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hook"))
        {
            // Instantiate explosion effect
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            
            // Apply penalty (without attaching to hook)
            ApplyHookPenalty(); // Call the penalty logic directly
            
            // Add score penalty
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(value); // Apply money change (could be negative)
            }

            // Destroy the redworm
            Destroy(gameObject);
        }
    }
    
    // Override the AttachToHook method to apply penalty
    public override void AttachToHook(Transform hookTransform)
    {
        Debug.LogWarning("Redworm AttachToHook called, but interaction should be handled by OnTriggerEnter if hook is a trigger.");
        ApplyHookPenalty();
        
        // Add score penalty
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(value);
        }
        
        Destroy(gameObject);
    }
    
    // Apply penalty logic
    private void ApplyHookPenalty()
    {
        // Find the PlayerController
        PlayerController playerController = FindObjectOfType<PlayerController>();
        
        if (playerController != null)
        {
            // Future enhancement: Add visual effect for player penalty
            
            // Cause stun effect on harder variants
            if (causesStun)
            {
                // Future enhancement: Implement stun mechanic
                Debug.Log("Player stunned by fast redworm!");
            }
            
            Debug.Log($"Redworm caused {penaltyTime}s time penalty!");
        }
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
        else
        {
            // Draw movement direction in play mode
            Debug.DrawRay(transform.position, moveDirection * 1.5f, Color.yellow);
            
            // Draw boundaries for debugging
            float halfWidth = boundaryWidth * 0.5f;
            float halfHeight = boundaryHeight * 0.5f;
            float minX = -halfWidth;
            float maxX = halfWidth;
            float minY = boundaryYOffset - halfHeight;
            float maxY = boundaryYOffset + halfHeight;
            
            Vector3 bottomLeft = new Vector3(minX, minY, 0);
            Vector3 bottomRight = new Vector3(maxX, minY, 0);
            Vector3 topLeft = new Vector3(minX, maxY, 0);
            Vector3 topRight = new Vector3(maxX, maxY, 0);
            
            Debug.DrawLine(bottomLeft, bottomRight, Color.red);
            Debug.DrawLine(bottomRight, topRight, Color.red);
            Debug.DrawLine(topRight, topLeft, Color.red);
            Debug.DrawLine(topLeft, bottomLeft, Color.red);
        }
        #endif
    }
}
