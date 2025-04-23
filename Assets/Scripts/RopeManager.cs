using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Add this for Canvas support

public class RopeManager : MonoBehaviour
{
    [Header("Rope Stress Settings")]
    public int maxStress = 500;          // Drastically increased from 100 to 500
    public float stressDecayRate = 2.0f; // Doubled from 1.0f to 2.0f for much faster decay
    public float stressPerPull = 0.1f;   // Further reduced from 0.2f to 0.1f
    public float stressPerWeightUnit = 0.01f; // Further reduced from 0.02f to 0.01f
    
    [Header("Rope Visual Feedback")]
    public Color lowStressColor = Color.green;
    public Color mediumStressColor = Color.yellow;
    public Color highStressColor = Color.red;
    public float warningThreshold = 0.9f; // Increased from 0.8f to 0.9f (90% threshold for warnings)
    
    [Header("Rope Break Settings")]
    public float breakThreshold = 2.0f;   // Increased from 1.5f to 2.0f (200% of max stress causes breaks)
    public float repairTime = 1.5f;       // Further reduced from 2.0f to 1.5f seconds
    
    // Internal state
    private float currentStress = 0f;
    private bool isRepairing = false;
    private LineRenderer lineRenderer; // Reference to the rope's LineRenderer
    private Color originalRopeColor;
    
    // Properties
    public float CurrentStress { get { return currentStress; } }
    public float StressPercentage { get { return currentStress / maxStress; } }
    public bool IsRepairing { get { return isRepairing; } }
    
    void Start()
    {
        // Get line renderer from the player's rope
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.lineRenderer != null)
        {
            lineRenderer = player.lineRenderer;
            originalRopeColor = lineRenderer.startColor;
        }
    }
    
    void Update()
    {
        // Skip updates if repairing
        if (isRepairing)
            return;
            
        // Decay stress over time
        if (currentStress > 0)
        {
            currentStress = Mathf.Max(0, currentStress - (stressDecayRate * Time.deltaTime));
            UpdateRopeColor();
        }
    }
    
    // Add stress from pulls or stress objects
    public void AddStress(float stressAmount)
    {
        // Skip if repairing
        if (isRepairing)
            return;
            
        currentStress += stressAmount;
        
        // Check for rope break
        if (currentStress >= maxStress * breakThreshold)
        {
            BreakRope();
        }
        
        UpdateRopeColor();
    }
    
    // Apply stress based on stress type and impact value
    public void ApplyStress(RopeStress.StressType stressType, float stressImpact)
    {
        float multiplier = 0.25f; // Reduced from 0.5f to 0.25f
        
        // Different stress types affect the rope differently
        switch (stressType)
        {
            case RopeStress.StressType.Pressure:
                multiplier = 0.25f;  // Reduced from 0.5f to 0.25f
                break;
            case RopeStress.StressType.Tension:
                multiplier = 0.4f;   // Reduced from 0.75f to 0.4f
                break;
            case RopeStress.StressType.Shear:
                multiplier = 0.6f;   // Reduced from 1.0f to 0.6f
                break;
        }
        
        // Apply the stress with the appropriate multiplier
        AddStress(stressImpact * multiplier);
        
        // Apply penalties to player if needed
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // Different penalties based on stress type
            switch (stressType)
            {
                case RopeStress.StressType.Pressure:
                    player.ApplyStressPenalties(0.02f, 0.0f, false); // Reduced from 0.05f to 0.02f
                    break;
                case RopeStress.StressType.Tension:
                    player.ApplyStressPenalties(0.05f, 0.02f, false); // Reduced from 0.1f,0.05f to 0.05f,0.02f
                    break;
                case RopeStress.StressType.Shear:
                    player.ApplyStressPenalties(0.08f, 0.05f, true); // Reduced from 0.15f,0.1f to 0.08f,0.05f
                    break;
            }
        }
        
        Debug.Log($"Applied {stressType} stress with impact {stressImpact}. Current stress: {currentStress}/{maxStress}");
    }
    
    // Calculate stress for pulling an object based on its weight
    public void AddPullStress(float weight)
    {
        // Calculate base stress from pulling
        float pullStress = stressPerPull;
        
        // Add weight-based stress with diminishing returns
        // Use square root to reduce the impact of very heavy objects
        if (weight > 0)
        {
            pullStress += Mathf.Sqrt(weight) * stressPerWeightUnit;
        }
        
        // Cap the maximum stress that can be added in one frame
        pullStress = Mathf.Min(pullStress, maxStress * 0.01f); // Maximum 1% of max stress per frame
        
        // Apply the stress
        AddStress(pullStress);
    }
    
    // Handle rope breaking
    private void BreakRope()
    {
        if (isRepairing)
            return;
            
        Debug.Log("Rope broke! Repairing...");
        isRepairing = true;
        
        // Notify player controller
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.OnRopeBreak();
            
            // Show a "Stress Limit!" floating text near the player
            ShowRopeStressText(player.transform.position);
        }
        
        // Start repair coroutine
        StartCoroutine(RepairRope());
    }
    
    // Show stress limit exceeded text
    private void ShowRopeStressText(Vector3 position)
    {
        // Try to find the main canvas
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas != null)
        {
            // Find main camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // Create floating text with a warning about stress
                FloatingText.CreateWorldToUI(position, mainCanvas, mainCamera, "Stress Limit!");
            }
        }
    }
    
    // Repair rope after break
    private IEnumerator RepairRope()
    {
        // Wait for repair time
        yield return new WaitForSeconds(repairTime);
        
        // Reset stress
        currentStress = 0;
        isRepairing = false;
        
        // Update visual feedback
        UpdateRopeColor();
        
        // Notify player controller
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.OnRopeRepaired();
        }
        
        Debug.Log("Rope repaired!");
    }
    
    // Update rope color based on current stress
    private void UpdateRopeColor()
    {
        if (lineRenderer != null)
        {
            Color stressedColor;
            
            if (StressPercentage < 0.5f)
                stressedColor = Color.Lerp(originalRopeColor, lowStressColor, StressPercentage * 2);
            else if (StressPercentage < 0.8f)
                stressedColor = Color.Lerp(lowStressColor, mediumStressColor, (StressPercentage - 0.5f) * (1f / 0.3f));
            else
                stressedColor = Color.Lerp(mediumStressColor, highStressColor, (StressPercentage - 0.8f) * (1f / 0.2f));
                
            lineRenderer.startColor = stressedColor;
            lineRenderer.endColor = stressedColor;
        }
    }
    
    // Apply upgrades to stress system
    public void ApplyRopeReinforcement(float stressReductionPercent)
    {
        stressPerPull *= (1f - stressReductionPercent);
        stressPerWeightUnit *= (1f - stressReductionPercent);
        Debug.Log($"Rope reinforced! Stress per pull reduced to {stressPerPull}, stress per weight unit reduced to {stressPerWeightUnit}");
    }
    
    public void ApplyBetterRope(float stressLimitIncrease)
    {
        int originalMax = maxStress;
        maxStress = Mathf.RoundToInt(maxStress * (1f + stressLimitIncrease));
        Debug.Log($"Better rope applied! Stress limit increased from {originalMax} to {maxStress}");
        
        // Update visual feedback
        UpdateRopeColor();
    }
} 