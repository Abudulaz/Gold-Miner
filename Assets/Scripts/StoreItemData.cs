using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoreItem", menuName = "Gold Miner/Store Item Data")]
public class StoreItemData : ScriptableObject
{
    [Header("Item Basic Info")]
    public string itemID;           // Unique identifier
    public string itemName;         // Display name 
    public Sprite icon;             // Optional sprite icon
    
    [Header("Item Variants")]
    public StoreItemVariant[] variants = new StoreItemVariant[3]; // Easy, Medium, Hard
    
    // Get the variant based on difficulty level (0=Easy, 1=Medium, 2=Hard)
    public StoreItemVariant GetVariant(int difficultyLevel)
    {
        // Clamp difficulty between 0-2
        int index = Mathf.Clamp(difficultyLevel, 0, variants.Length - 1);
        return variants[index];
    }
}

[System.Serializable]
public class StoreItemVariant
{
    public string description;      // Description of this variant's effect
    public int price;               // Price of this variant
    
    [TextArea(2, 4)]
    public string effectDescription; // Detailed description of effect
    
    // Additional effect parameters (these would be used by the game systems)
    public float effectValue;       // Primary effect value (e.g. +10% strength)
    public float secondaryValue;    // Secondary effect value if needed
} 