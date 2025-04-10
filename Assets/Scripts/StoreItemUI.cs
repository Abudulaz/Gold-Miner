using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;
    public Image iconImage;
    public Button purchaseButton;
    public GameObject soldOutOverlay;
    public GameObject discountBadge;
    public TextMeshProUGUI discountPercentText;
    
    private StoreManager.StoreItem itemData;
    private StoreManager storeManager;
    private bool purchased = false;
    private int originalPrice = 0;
    private int discountedPrice = 0;
    
    public void Setup(StoreManager.StoreItem item, StoreManager manager)
    {
        itemData = item;
        storeManager = manager;
        originalPrice = item.price;
        
        // Set the UI elements
        if (nameText != null)
            nameText.text = item.itemName;
            
        if (descriptionText != null)
            descriptionText.text = item.description;
            
        // Apply credit card discount if applicable
        UpdatePriceDisplay();
            
        if (iconImage != null && item.icon != null)
            iconImage.sprite = item.icon;
            
        // Add listeners
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
            
            // Update button interactability based on player's money
            UpdateButtonState();
        }
        
        // Hide sold out overlay initially
        if (soldOutOverlay != null)
            soldOutOverlay.SetActive(false);
            
        // Show discount badge if applicable
        UpdateDiscountBadge();
    }
    
    void Update()
    {
        // Continuously update button state based on player's money and price
        if (!purchased && purchaseButton != null && storeManager != null)
        {
            UpdateButtonState();
            UpdatePriceDisplay();
            UpdateDiscountBadge();
        }
    }
    
    void UpdatePriceDisplay()
    {
        if (priceText != null)
        {
            if (CreditCard.hasActiveCard && !CreditCard.discountApplied)
            {
                // Apply discount
                discountedPrice = CreditCard.ApplyDiscount(originalPrice);
                
                // Only display the discount (don't apply it yet)
                CreditCard.discountApplied = false;
                
                if (discountedPrice < originalPrice)
                {
                    priceText.text = $"{discountedPrice} <s>{originalPrice}</s>";
                }
                else
                {
                    priceText.text = originalPrice.ToString();
                }
            }
            else
            {
                priceText.text = originalPrice.ToString();
                discountedPrice = originalPrice;
            }
        }
    }
    
    void UpdateDiscountBadge()
    {
        if (discountBadge != null && discountPercentText != null)
        {
            if (CreditCard.hasActiveCard && !CreditCard.discountApplied && discountedPrice < originalPrice)
            {
                discountBadge.SetActive(true);
                
                int discountPercent = 0;
                switch (CreditCard.activeCardType)
                {
                    case CreditCard.CardType.Bronze: discountPercent = 25; break;
                    case CreditCard.CardType.Silver: discountPercent = 50; break;
                    case CreditCard.CardType.Gold: discountPercent = 75; break;
                }
                
                discountPercentText.text = $"-{discountPercent}%";
            }
            else
            {
                discountBadge.SetActive(false);
            }
        }
    }
    
    void UpdateButtonState()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            // Use discounted price for comparison if applicable
            int priceToCheck = (CreditCard.hasActiveCard && !CreditCard.discountApplied) ? 
                discountedPrice : originalPrice;
                
            // Enable button only if player has enough money
            purchaseButton.interactable = gameManager.score >= priceToCheck;
        }
    }
    
    void OnPurchaseClicked()
    {
        if (storeManager != null && !purchased)
        {
            // Check if a discount should be applied
            if (CreditCard.hasActiveCard && !CreditCard.discountApplied)
            {
                // Now actually apply the discount (sets discountApplied to true)
                int finalPrice = CreditCard.ApplyDiscount(originalPrice);
                
                // Temporarily modify the item price
                itemData.price = finalPrice;
                
                // Purchase with the discounted price
                storeManager.PurchaseItem(itemData);
                
                // Restore original price
                itemData.price = originalPrice;
            }
            else
            {
                // Regular purchase
                storeManager.PurchaseItem(itemData);
            }
            
            purchased = true;
            
            // Disable button after purchase
            if (purchaseButton != null)
                purchaseButton.interactable = false;
                
            // Show sold out overlay
            if (soldOutOverlay != null)
                soldOutOverlay.SetActive(true);
                
            // Hide discount badge
            if (discountBadge != null)
                discountBadge.SetActive(false);
        }
    }
}
