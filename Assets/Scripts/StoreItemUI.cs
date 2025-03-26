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
    
    private StoreManager.StoreItem itemData;
    private StoreManager storeManager;
    private bool purchased = false;
    
    public void Setup(StoreManager.StoreItem item, StoreManager manager)
    {
        itemData = item;
        storeManager = manager;
        
        // Set the UI elements
        if (nameText != null)
            nameText.text = item.itemName;
            
        if (descriptionText != null)
            descriptionText.text = item.description;
            
        if (priceText != null)
            priceText.text = item.price.ToString();
            
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
    }
    
    void Update()
    {
        // Continuously update button state based on player's money
        if (!purchased && purchaseButton != null && storeManager != null)
        {
            UpdateButtonState();
        }
    }
    
    void UpdateButtonState()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            // Enable button only if player has enough money
            purchaseButton.interactable = gameManager.score >= itemData.price;
        }
    }
    
    void OnPurchaseClicked()
    {
        if (storeManager != null && !purchased)
        {
            storeManager.PurchaseItem(itemData);
            purchased = true;
            
            // Disable button after purchase
            if (purchaseButton != null)
                purchaseButton.interactable = false;
                
            // Show sold out overlay
            if (soldOutOverlay != null)
                soldOutOverlay.SetActive(true);
        }
    }
}
