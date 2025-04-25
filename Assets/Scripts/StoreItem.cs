using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoreItem
{
    // Basic item properties
    public string itemID;           // Unique identifier
    public string itemName;         // Display name
    public int price;               // Current price (based on difficulty)
    public bool isPurchased;        // Whether the player owns this item
    
    // Reference to the source data
    public StoreItemData itemData;
    
    // Description for the current difficulty level
    public string description;
    
    /// <summary>
    /// Constructor that creates a StoreItem from StoreItemData
    /// </summary>
    /// <param name="data">Source data</param>
    /// <param name="difficultyLevel">Current difficulty (0=Easy, 1=Medium, 2=Hard)</param>
    public StoreItem(StoreItemData data, int difficultyLevel)
    {
        itemData = data;
        
        // Copy basic properties
        itemID = data.itemID;
        itemName = data.itemName;
        isPurchased = false;
        
        // Get the variant for the current difficulty
        StoreItemVariant variant = data.GetVariant(difficultyLevel);
        
        // Set variant-specific properties
        price = variant.price;
        description = variant.description;
    }
    
    // Empty constructor for serialization
    public StoreItem() { }
} 