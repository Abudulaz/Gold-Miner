using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI descriptionText; // Description text
    public Button purchaseButton;
    public TextMeshProUGUI soldOutText;  // Text instead of overlay
    
    [Header("Audio")]
    public AudioClip purchaseSound;      // Optional purchase sound

    private StoreItem item;
    private StoreManager storeManager;
    private AudioSource audioSource;

    private void Awake()
    {
        // Find StoreManager immediately to ensure it's available
        storeManager = FindObjectOfType<StoreManager>();
    }

    private void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && purchaseSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Setup the UI element with the store item data
    /// </summary>
    /// <param name="storeItem">The store item to display</param>
    public void Setup(StoreItem storeItem)
    {
        item = storeItem;
        
        // Make sure we have a reference to the StoreManager
        if (storeManager == null)
        {
            storeManager = FindObjectOfType<StoreManager>();
            if (storeManager == null)
            {
                Debug.LogError("StoreManager not found - StoreItemUI cannot function properly");
                return;
            }
        }
        
        // Set main properties
        nameText.text = storeItem.itemName;
            
        // Set description if available
        if (descriptionText != null)
            descriptionText.text = storeItem.description;
        
        // Update price display
        UpdatePriceDisplay();
        
        // Check if already owned
        bool isOwned = storeManager.PlayerOwnsItem(storeItem.itemID);
        soldOutText.gameObject.SetActive(isOwned);
        purchaseButton.interactable = !isOwned;
        
        // Register click event handler - this was missing!
        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnPurchaseClicked);
        
        // Log for debugging
        Debug.Log($"Set up store item: {storeItem.itemName}, ID: {storeItem.itemID}, Button registered: {purchaseButton != null}");
    }

    private void Update()
    {
        // Continuously check if item is already owned (in case of purchase from another menu)
        if (item != null && storeManager != null)
        {
            bool isOwned = storeManager.PlayerOwnsItem(item.itemID);
            soldOutText.gameObject.SetActive(isOwned);
            purchaseButton.interactable = !isOwned;
        }
    }

    /// <summary>
    /// Updates the price display
    /// </summary>
    private void UpdatePriceDisplay()
    {
        priceText.text = $"${item.price}";
    }

    /// <summary>
    /// Called when the purchase button is clicked
    /// </summary>
    public void OnPurchaseClicked()
    {
        if (storeManager == null || item == null)
            return;
            
        bool purchaseSuccessful = storeManager.PurchaseItem(item.itemID, item.price);
        
        if (purchaseSuccessful)
        {
            // Play purchase sound if available
            if (audioSource != null && purchaseSound != null)
            {
                audioSource.PlayOneShot(purchaseSound);
            }
            
            soldOutText.gameObject.SetActive(true);
            purchaseButton.interactable = false;
        }
    }
}
