using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreManager : MonoBehaviour
{
    [System.Serializable]
    public class StoreItem
    {
        public string itemName;
        public string description;
        public int price;
        public Sprite icon;
        public StoreItemType itemType;
        public float upgradeValue;
    }
    
    public enum StoreItemType
    {
        HookSpeed,    // Faster retrieval speed
        TimeExtension, // Add time to the game
        ValueBooster   // Increase gold value
    }
    
    [Header("UI References")]
    public GameObject storePanel;
    public Transform itemContainer; // Parent transform for instantiated items
    public GameObject itemPrefab;   // Prefab for store items
    public TextMeshProUGUI playerMoneyText;
    public Button continueButton;
    
    [Header("Store Items")]
    public List<StoreItem> availableItems = new List<StoreItem>();
    
    private GameManager gameManager;
    private PlayerController playerController;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        playerController = FindObjectOfType<PlayerController>();
        
        // Hide store initially
        if (storePanel != null)
            storePanel.SetActive(false);
            
        // Subscribe to game state changes
        if (gameManager != null)
            gameManager.OnGameStateChanged += OnGameStateChanged;
            
        // Setup default items if none defined
        if (availableItems.Count == 0)
            SetupDefaultItems();
            
        // Add continue button listener
        if (continueButton != null)
            continueButton.onClick.AddListener(CloseStore);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        if (gameManager != null)
            gameManager.OnGameStateChanged -= OnGameStateChanged;
    }
    
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Store)
        {
            OpenStore();
        }
        else if (storePanel != null && storePanel.activeSelf)
        {
            storePanel.SetActive(false);
        }
    }
    
    void OpenStore()
    {
        if (storePanel != null)
        {
            storePanel.SetActive(true);
            PopulateStoreItems();
            UpdateMoneyDisplay();
        }
    }
    
    void CloseStore()
    {
        if (storePanel != null)
            storePanel.SetActive(false);
            
        // Set the game back to running or game over based on time
        if (gameManager.timeRemaining > 0)
        {
            gameManager.SetGameState(GameManager.GameState.Running);
        }
        else
        {
            gameManager.SetGameState(GameManager.GameState.GameOver);
        }
    }
    
    void PopulateStoreItems()
    {
        // Clear existing items
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);
            
        // Instantiate new items
        foreach (StoreItem item in availableItems)
        {
            GameObject itemObj = Instantiate(itemPrefab, itemContainer);
            StoreItemUI itemUI = itemObj.GetComponent<StoreItemUI>();
            
            if (itemUI != null)
            {
                itemUI.Setup(item, this);
            }
        }
    }
    
    public void PurchaseItem(StoreItem item)
    {
        // Check if player has enough money
        if (gameManager.score >= item.price)
        {
            // Deduct cost
            gameManager.AddScore(-item.price);
            
            // Apply effect based on item type
            ApplyItemEffect(item);
            
            // Update UI
            UpdateMoneyDisplay();
        }
    }
    
    void ApplyItemEffect(StoreItem item)
    {
        switch (item.itemType)
        {
            case StoreItemType.HookSpeed:
                if (playerController != null)
                    playerController.pullSpeed += item.upgradeValue;
                break;
                
            case StoreItemType.TimeExtension:
                gameManager.AddTime(item.upgradeValue);
                break;
                
            case StoreItemType.ValueBooster:
                // Apply a global value multiplier that would be used by collectibles
                Collectible.valueMultiplier *= item.upgradeValue;
                break;
        }
    }
    
    void UpdateMoneyDisplay()
    {
        if (playerMoneyText != null)
            playerMoneyText.text = "Money: " + gameManager.score;
    }
    
    void SetupDefaultItems()
    {
        // Add default items if no items were defined in the inspector
        availableItems.Add(new StoreItem {
            itemName = "Hook Engine Upgrade",
            description = "Faster retrieval speed by 20%",
            price = 200,
            itemType = StoreItemType.HookSpeed,
            upgradeValue = 0.2f
        });
        
        availableItems.Add(new StoreItem {
            itemName = "Time Extension",
            description = "Adds 30 seconds to the game",
            price = 300,
            itemType = StoreItemType.TimeExtension,
            upgradeValue = 30f
        });
        
        availableItems.Add(new StoreItem {
            itemName = "Gold Polisher",
            description = "Gold is worth 25% more",
            price = 500,
            itemType = StoreItemType.ValueBooster,
            upgradeValue = 1.25f
        });
    }
}
