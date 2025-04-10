using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : Collectible
{
    public enum RockSize { Small, Medium, Large }
    
    [Header("Rock Settings")]
    public RockSize size = RockSize.Small;
    
    void Start()
    {
        // Set rock properties based on size variant
        switch (size)
        {
            case RockSize.Small:
                value = 5;
                weight = 8.0f;
                transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                break;
                
            case RockSize.Medium:
                value = 10;
                weight = 15.0f;
                transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                break;
                
            case RockSize.Large:
                value = 15;
                weight = 25.0f;
                transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
                break;
        }
        
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
            float displayWeight = 0;
            switch (size)
            {
                case RockSize.Small: displayWeight = 8.0f; break;
                case RockSize.Medium: displayWeight = 15.0f; break;
                case RockSize.Large: displayWeight = 25.0f; break;
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up, "Weight: " + displayWeight);
        }
        #endif
    }
}
