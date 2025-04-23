using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeStress : MonoBehaviour
{
    // Stress properties
    [SerializeField] private float stressImpact = 0.1f;
    [SerializeField] private StressType stressType = StressType.Pressure;
    
    public enum StressType
    {
        Pressure,
        Tension,
        Shear
    }
    
    private Renderer objectRenderer;
    private bool hasAppliedStress = false;
    private float cooldownTimer = 0f;
    private float cooldownDuration = 1.5f; // Time before this object can apply stress again
    
    void Start()
    {
        // Get the renderer and make the object invisible
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            objectRenderer.enabled = false;
        }
        
        // Ensure the collider is a trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }
    
    void Update()
    {
        // Cool down timer for reapplying stress
        if (hasAppliedStress && cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                hasAppliedStress = false;
            }
        }
    }
    
    // Auto-apply stress when hook enters trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only apply stress if not on cooldown
        if (!hasAppliedStress)
        {
            // Check if the collider is the hook
            if (other.CompareTag("Hook"))
            {
                // Find rope manager and apply stress
                RopeManager ropeManager = FindObjectOfType<RopeManager>();
                if (ropeManager != null)
                {
                    ApplyStressTo(ropeManager);
                    hasAppliedStress = true;
                    cooldownTimer = cooldownDuration;
                    Debug.Log($"Applied {stressType} stress to hook via trigger collision!");
                }
            }
        }
    }
    
    public float GetStressImpact()
    {
        return stressImpact;
    }
    
    public StressType GetStressType()
    {
        return stressType;
    }
    
    // This is used when we want to apply stress directly rather than through collision
    public void ApplyStressTo(RopeManager ropeManager)
    {
        if (ropeManager != null)
        {
            ropeManager.ApplyStress(stressType, stressImpact);
        }
    }
} 