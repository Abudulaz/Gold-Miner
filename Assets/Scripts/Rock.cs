using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : Collectible
{
    public enum RockSize { Small, Medium, Large }
    
    [Header("Rock Settings")]
    public RockSize size = RockSize.Small;
    
    // Base values for different rock sizes
    private readonly float[] baseWeights = { 6.0f, 12.0f, 20.0f };
    private readonly int[] baseValues = { 5, 10, 15 };
    private readonly float[] baseScales = { 0.6f, 0.9f, 1.4f };
    
    // Level scaling factors
    private readonly float weightLevelMultiplier = 0.1f; // 10% increase per level
    private readonly float valueLevelMultiplier = 0.15f; // 15% increase per level
    
    void Start()
    {
        // Get current level from GameManager
        int currentLevel = GameManager.Instance ? GameManager.Instance.currentLevel : 1;
        
        // Get base values according to size
        int sizeIndex = (int)size;
        float baseWeight = baseWeights[sizeIndex];
        int baseValue = baseValues[sizeIndex];
        float baseScale = baseScales[sizeIndex];
        
        // Apply level-based scaling
        float levelWeightFactor = 1 + (weightLevelMultiplier * (currentLevel - 1));
        float levelValueFactor = 1 + (valueLevelMultiplier * (currentLevel - 1));
        
        // Set final values
        weight = baseWeight * levelWeightFactor;
        value = Mathf.RoundToInt(baseValue * levelValueFactor);
        transform.localScale = new Vector3(baseScale, baseScale, baseScale);
        
        Debug.Log($"Rock {size}: Level {currentLevel}, Weight {weight}, Value {value}");
        
        // Set rock appearance (darker for heavier rocks)
        if (GetComponent<Renderer>() != null)
        {
            float darkness = Mathf.Lerp(0.7f, 0.3f, weight / 30.0f);
            GetComponent<Renderer>().material.color = new Color(darkness, darkness, darkness);
        }
    }
    
    // Optional: Visualization in the editor
    void OnDrawGizmosSelected()
    {
        // Display the weight as text in the Scene view
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Get current level from GameManager or use 1 if not available
            int currentLevel = 1;
            if (GameManager.Instance)
                currentLevel = GameManager.Instance.currentLevel;
            
            // Get base values according to size
            int sizeIndex = (int)size;
            float baseWeight = baseWeights[sizeIndex];
            int baseValue = baseValues[sizeIndex];
            
            // Apply level-based scaling
            float levelWeightFactor = 1 + (weightLevelMultiplier * (currentLevel - 1));
            float levelValueFactor = 1 + (valueLevelMultiplier * (currentLevel - 1));
            
            // Calculate display values
            float displayWeight = baseWeight * levelWeightFactor;
            int displayValue = Mathf.RoundToInt(baseValue * levelValueFactor);
            
            UnityEditor.Handles.Label(transform.position + Vector3.up, 
                $"Weight: {displayWeight:F1}, Value: {displayValue}, Level: {currentLevel}");
        }
        #endif
    }
}
