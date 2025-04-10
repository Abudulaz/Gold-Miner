using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditCard : Collectible
{
    public enum CardType { Bronze, Silver, Gold }
    
    [Header("Credit Card Settings")]
    public CardType cardType = CardType.Bronze;
    [Range(0, 100)] public int discountPercentage;
    [Range(0, 100)] public int moneyReductionPercentage;
    
    // Static variables to track card effects across game sessions
    public static bool hasActiveCard = false;
    public static CardType activeCardType = CardType.Bronze;
    public static bool discountApplied = false;
    
    void Start()
    {
        // Set properties based on card type
        switch (cardType)
        {
            case CardType.Bronze:
                discountPercentage = 25;
                moneyReductionPercentage = 50;
                value = 10; // Small positive value for collecting
                weight = 3.0f;
                break;
                
            case CardType.Silver:
                discountPercentage = 50;
                moneyReductionPercentage = 75;
                value = 20;
                weight = 3.0f;
                break;
                
            case CardType.Gold:
                discountPercentage = 75;
                moneyReductionPercentage = 90;
                value = 30;
                weight = 3.0f;
                break;
        }
        
        // Set credit card appearance
        if (GetComponent<Renderer>() != null)
        {
            Color cardColor;
            switch (cardType)
            {
                case CardType.Bronze:
                    cardColor = new Color(0.8f, 0.5f, 0.2f); // Bronze color
                    break;
                case CardType.Silver:
                    cardColor = new Color(0.75f, 0.75f, 0.75f); // Silver color
                    break;
                case CardType.Gold:
                    cardColor = new Color(1f, 0.84f, 0f); // Gold color
                    break;
                default:
                    cardColor = Color.white;
                    break;
            }
            GetComponent<Renderer>().material.color = cardColor;
        }
        
        // Make credit card flat and card-like
        transform.localScale = new Vector3(
            transform.localScale.x * 1.5f, 
            transform.localScale.y * 0.1f, 
            transform.localScale.z * 1f
        );
    }
    
    // Override the AttachToHook method to apply special credit card effects
    public override void AttachToHook(Transform hookTransform)
    {
        // Activate credit card effect
        hasActiveCard = true;
        activeCardType = cardType;
        discountApplied = false;
        
        // Show notification
        Debug.Log($"Collected a {cardType} Credit Card! You'll get {discountPercentage}% discount in the store.");
        
        // Call base method to attach to hook
        base.AttachToHook(hookTransform);
    }
    
    // Apply discount to a price
    public static int ApplyDiscount(int originalPrice)
    {
        if (!hasActiveCard || discountApplied)
            return originalPrice;
            
        // Calculate discount based on card type
        float discountMultiplier = 0f;
        switch (activeCardType)
        {
            case CardType.Bronze: discountMultiplier = 0.25f; break;
            case CardType.Silver: discountMultiplier = 0.50f; break;
            case CardType.Gold: discountMultiplier = 0.75f; break;
        }
        
        // Apply discount
        int discountedPrice = Mathf.RoundToInt(originalPrice * (1 - discountMultiplier));
        
        // Mark discount as applied to prevent multiple uses
        discountApplied = true;
        
        return discountedPrice;
    }
    
    // Apply money reduction
    public static void ApplyMoneyReduction(GameManager gameManager)
    {
        if (!hasActiveCard || !discountApplied)
            return;
            
        // Calculate reduction based on card type
        float reductionMultiplier = 0f;
        switch (activeCardType)
        {
            case CardType.Bronze: reductionMultiplier = 0.50f; break;
            case CardType.Silver: reductionMultiplier = 0.75f; break;
            case CardType.Gold: reductionMultiplier = 0.90f; break;
        }
        
        // Apply reduction
        int currentMoney = gameManager.score;
        int reduction = Mathf.RoundToInt(currentMoney * reductionMultiplier);
        
        // Display warning
        Debug.Log($"Credit Card debt collected! {reductionMultiplier * 100}% of your money ({reduction} coins) has been deducted.");
        
        // Apply the reduction
        gameManager.AddScore(-reduction);
        
        // Reset credit card state
        hasActiveCard = false;
    }
    
    void OnDrawGizmosSelected()
    {
        // Display credit card details in editor
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            string info = "";
            switch (cardType)
            {
                case CardType.Bronze:
                    info = "Bronze: 25% discount / 50% reduction";
                    break;
                case CardType.Silver:
                    info = "Silver: 50% discount / 75% reduction";
                    break;
                case CardType.Gold:
                    info = "Gold: 75% discount / 90% reduction";
                    break;
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up, info);
        }
        #endif
    }
}
