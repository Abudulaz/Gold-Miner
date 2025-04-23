using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gold : Collectible
{
    // Define gold sizes similar to rock for consistency
    public enum GoldSize { Small, Medium, Large }
    
    [Header("Gold Settings")]
    public GoldSize size = GoldSize.Small;
    
    void Start()
    {
        // Set gold properties based on size variant
        switch (size)
        {
            case GoldSize.Small:
                value = 100;
                weight = 12.0f; // Gold is denser than rock
                transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                break;
                
            case GoldSize.Medium:
                value = 200;
                weight = 22.0f; // Heavier than medium rock
                transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                break;
                
            case GoldSize.Large:
                value = 300;
                weight = 35.0f; // Heavier than large rock
                transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
                break;
        }
        
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
            float displayWeight = 0;
            int displayValue = 0;
            switch (size)
            {
                case GoldSize.Small: 
                    displayWeight = 12.0f; 
                    displayValue = 100;
                    break;
                case GoldSize.Medium: 
                    displayWeight = 22.0f; 
                    displayValue = 200;
                    break;
                case GoldSize.Large: 
                    displayWeight = 35.0f; 
                    displayValue = 300;
                    break;
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up, "Weight: " + displayWeight + ", Value: " + displayValue);
        }
        #endif
    }
}
