using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gold : Collectible
{
    // Define gold sizes similar to rock for consistency
    public enum GoldSize { Small, Medium, Large }
    
    [Header("Gold Settings")]
    public GoldSize size = GoldSize.Small;
    
    // Base values for different gold sizes
    private readonly float[] baseWeights = { 10.0f, 18.0f, 28.0f };
    private readonly int[] baseValues = { 100, 200, 300 };
    private readonly float[] baseScales = { 0.6f, 0.9f, 1.4f };
    
    // Level scaling factors
    private readonly float weightLevelMultiplier = 0.15f; // 15% increase per level
    private readonly float valueLevelMultiplier = 0.25f; // 25% increase per level
    
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
        
        Debug.Log($"Gold {size}: Level {currentLevel}, Weight {weight}, Value {value}");
        
        // Set gold appearance
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().material.color = Color.yellow;
        }
    }
    
    // Optional: Visualization in the editor
    void OnDrawGizmosSelected()
    {
        // Display the weight and value as text in the Scene view
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
