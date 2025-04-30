using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class StoreManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject storePanel;
    public Transform itemContainer;       // Parent transform for instantiated items
    public GameObject storeItemPrefab;    // Prefab for store items
    public TextMeshProUGUI playerMoneyText;
    public Button continueButton;
    
    [Header("Store Items")]
    public StoreItemData[] storeItemsData;    // ScriptableObject item data
    private List<StoreItem> availableItems = new List<StoreItem>();
    
    [Header("Mandatory Items")]
    public string spareRopesItemID = "spare_ropes";  // ID for the rope item that's always available
    
    [Header("Store Generation Settings")]
    [Tooltip("Minimum number of items to display in the store")]
    [Range(1, 10)]
    public int minItemCount = 3;
    
    [Tooltip("Maximum number of items to display in the store")]
    [Range(1, 10)]
    public int maxItemCount = 5;
    
    [Tooltip("Whether to randomize items shown each time the store opens")]
    public bool randomizeItems = true;
    
    [Tooltip("Whether to ensure all item types appear at least once before repeating")]
    public bool ensureVariety = true;
    
    private GameManager gameManager;
    private PlayerController playerController;
    private List<string> purchasedItems = new List<string>();
    private System.Random rng;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        playerController = FindObjectOfType<PlayerController>();
        rng = new System.Random();
        
        // Hide store initially
        if (storePanel != null)
            storePanel.SetActive(false);
            
        // Subscribe to game state changes
        if (gameManager != null)
            gameManager.OnGameStateChanged += OnGameStateChanged;
            
        // Setup items from data
        SetupStoreItems();
            
        // Add continue button listener
        if (continueButton != null)
            continueButton.onClick.AddListener(CloseStore);
    }
    
    // Get the current difficulty level based on GameManager.currentLevel
    private int GetDifficultyLevel()
    {
        if (gameManager != null)
        {
            // Simple scaling: Level 1-3: Easy (0), Level 4-6: Medium (1), Level 7+: Hard (2)
            return Mathf.Clamp((gameManager.currentLevel - 1) / 3, 0, 2);
        }
        return 0; // Default to easy if GameManager not available
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
            
            // If randomization is enabled, choose a random selection of items
            if (randomizeItems)
            {
                SelectRandomStoreItems();
            }
            
            // Ensure spare ropes item is always available
            EnsureSpareRopesAvailable();
            
            PopulateStoreItems();
            UpdateMoneyDisplay();
        }
    }
    
    // Ensure the spare ropes item is always available in the store
    private void EnsureSpareRopesAvailable()
    {
        // First check if it's already in the available items
        bool ropeItemExists = availableItems.Any(item => item.itemID == spareRopesItemID);
        
        if (!ropeItemExists)
        {
            // Find the item data
            StoreItemData ropeItemData = System.Array.Find(storeItemsData, data => 
                data != null && data.itemID == spareRopesItemID);
                
            if (ropeItemData != null)
            {
                // Add to available items using current difficulty level derived from GameManager.currentLevel
                StoreItem ropeItem = new StoreItem(ropeItemData, GetDifficultyLevel());
                
                // Override the price to be based directly on current level
                // Base price: 50 + 10 per level (first level: 60, second: 70, etc.)
                ropeItem.price = 50 + (gameManager.currentLevel * 10);
                
                availableItems.Add(ropeItem);
                Debug.Log($"Added mandatory spare ropes item to store at price: {ropeItem.price}");
            }
            else
            {
                Debug.LogWarning($"Could not find spare ropes item with ID '{spareRopesItemID}'");
            }
        }
    }
    
    void CloseStore()
    {
        if (storePanel != null)
            storePanel.SetActive(false);
            
        // Proceed to next level (GameManager will handle the details)
        gameManager.SetGameState(GameManager.GameState.NextLevel);
    }
    
    // Select a random subset of items to display in the store
    void SelectRandomStoreItems()
    {
        // If we don't have any items data, return early
        if (storeItemsData == null || storeItemsData.Length == 0)
            return;
            
        // Create a temporary master list of all items
        List<StoreItem> allItems = new List<StoreItem>();
        foreach (var itemData in storeItemsData)
        {
            if (itemData != null && itemData.itemID != spareRopesItemID) // Skip rope item as it's added separately
            {
                allItems.Add(new StoreItem(itemData, GetDifficultyLevel()));
            }
        }
        
        // Apply any filtering logic here (e.g., removing previously purchased items)
        allItems = allItems.Where(item => !PlayerOwnsItem(item.itemID)).ToList();
        
        // If no items are available, return
        if (allItems.Count == 0)
            return;
            
        // Clear current available items
        availableItems.Clear();
        
        // Determine how many items to display
        int itemCount = Mathf.Min(
            allItems.Count, 
            Random.Range(minItemCount, maxItemCount + 1)
        );
        
        if (ensureVariety && allItems.Count >= itemCount)
        {
            // Fisher-Yates shuffle
            for (int i = 0; i < allItems.Count - 1; i++)
            {
                int j = Random.Range(i, allItems.Count);
                StoreItem temp = allItems[i];
                allItems[i] = allItems[j];
                allItems[j] = temp;
            }
            
            // Take the first 'itemCount' items
            availableItems = allItems.Take(itemCount).ToList();
        }
        else
        {
            // Simple random selection (may have duplicates)
            for (int i = 0; i < itemCount; i++)
            {
                int randomIndex = Random.Range(0, allItems.Count);
                availableItems.Add(allItems[randomIndex]);
            }
        }
    }
    
    // Add specific items to the store using their item IDs
    public void AddSpecificItemsToStore(string[] itemIDs)
    {
        // Disable randomization to maintain specific items
        randomizeItems = false;
        
        // If we don't have any items data, return early
        if (storeItemsData == null || storeItemsData.Length == 0 || itemIDs == null || itemIDs.Length == 0)
            return;
        
        // Clear current available items
        availableItems.Clear();
        
        // Add each requested item if it exists in the store item data and isn't owned
        foreach (string itemID in itemIDs)
        {
            // Find the item data that matches this ID
            StoreItemData itemData = System.Array.Find(storeItemsData, data => data != null && data.itemID == itemID);
            
            // If item data exists and player doesn't already own it, add to available items
            if (itemData != null && !PlayerOwnsItem(itemID))
            {
                availableItems.Add(new StoreItem(itemData, GetDifficultyLevel()));
            }
        }
        
        // Ensure the rope item is always in the store
        EnsureSpareRopesAvailable();
        
        // Update the UI if the store is active
        if (storePanel != null && storePanel.activeSelf)
        {
            PopulateStoreItems();
            UpdateMoneyDisplay();
        }
    }
    
    // Add a single specific item to the store by its ID
    public bool AddSpecificItemToStore(string itemID)
    {
        // If we don't have any items data, return early
        if (storeItemsData == null || storeItemsData.Length == 0 || string.IsNullOrEmpty(itemID))
            return false;
        
        // Find the item data that matches this ID
        StoreItemData itemData = System.Array.Find(storeItemsData, data => data != null && data.itemID == itemID);
        
        // If item data exists and player doesn't already own it
        if (itemData != null && !PlayerOwnsItem(itemID))
        {
            // Disable randomization to maintain specific items
            randomizeItems = false;
            
            // Create and add the item
            StoreItem newItem = new StoreItem(itemData, GetDifficultyLevel());
            availableItems.Add(newItem);
            
            // Update the UI if the store is active
            if (storePanel != null && storePanel.activeSelf)
            {
                PopulateStoreItems();
                UpdateMoneyDisplay();
            }
            
            return true;
        }
        
        return false;
    }
    
    void PopulateStoreItems()
    {
        // Clear existing items
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);
            
        // Instantiate new items
        foreach (StoreItem item in availableItems)
        {
            GameObject itemObj = Instantiate(storeItemPrefab, itemContainer);
            StoreItemUI itemUI = itemObj.GetComponent<StoreItemUI>();
            
            if (itemUI != null)
            {
                itemUI.Setup(item);
            }
        }
    }
    
    // Check if player owns an item by ID
    public bool PlayerOwnsItem(string itemID)
    {
        // Spare ropes are always available (can be purchased multiple times)
        if (itemID == spareRopesItemID)
            return false;
            
        return purchasedItems.Contains(itemID);
    }
    
    // Purchase an item with the specified ID
    public bool PurchaseItem(string itemID, int price)
    {
        // Find the item
        StoreItem item = availableItems.Find(i => i.itemID == itemID);
        if (item == null)
            return false;
            
        // Check if player has enough money
        if (gameManager.money < price)
            return false;
            
        // Check if player already owns this item (except for rope item which can be bought multiple times)
        if (itemID != spareRopesItemID && PlayerOwnsItem(itemID))
            return false;
            
        // Deduct cost
        gameManager.AddMoney(-price);
        
        // Add to purchased items (except rope which is consumable)
        if (itemID != spareRopesItemID)
        {
        purchasedItems.Add(itemID);
        }
        
        // Mark the item as purchased
        item.isPurchased = true;
        
        // Apply effect based on the item
        ApplyItemEffect(item);
        
        // Update UI
        UpdateMoneyDisplay();
        
        return true;
    }
    
    void ApplyItemEffect(StoreItem item)
    {
        if (item.itemData == null)
            return;
            
        // Get the variant for the current difficulty
        StoreItemVariant variant = item.itemData.GetVariant(GetDifficultyLevel());
        
        // Apply effect based on item ID
        switch (item.itemID)
        {
            case "hook_upgrade":
            case "improved_claw":
                if (playerController != null)
                    playerController.pullSpeed += variant.effectValue;
                break;
                
            case "time_extension":
                gameManager.AddTime(variant.effectValue);
                break;
                
            case "gold_polisher":
            case "diamond_polish":
                Collectible.valueMultiplier *= (1f + variant.effectValue/100f);
                break;
                
            case "strength_potion":
            case "strength_drink":
                if (playerController != null) {
                    playerController.pullSpeed *= (1f + variant.effectValue/100f);
                }
                break;
                
            case "rock_collector_book":
                // Apply bonus to rock values
                break;
                
            case "rope_reinforcement":
                // Reduce stress per pull
                var ropeManager = FindObjectOfType<RopeManager>();
                if (ropeManager != null) {
                    ropeManager.stressPerPull *= (1f - variant.effectValue/100f);
                }
                break;
                
            case "better_rope":
                // Increase stress limit
                var ropeManager2 = FindObjectOfType<RopeManager>();
                if (ropeManager2 != null) {
                    ropeManager2.maxStress = Mathf.RoundToInt(ropeManager2.maxStress * (1f + variant.effectValue/100f));
                }
                break;
                
            case "spare_ropes":
                // Add ropes to the inventory based on level
                var ropeManager3 = FindObjectOfType<RopeManager>();
                if (ropeManager3 != null) {
                    // Calculate rope count based on level
                    // Early levels: 1 rope, mid levels: 2 ropes, higher levels: 3 ropes
                    int ropesToAdd = Mathf.Clamp(1 + (gameManager.currentLevel - 1) / 3, 1, 3);
                    
                    ropeManager3.AddRopes(ropesToAdd);
                    
                    // Display a floating text to confirm purchase
                    Canvas mainCanvas = FindObjectOfType<Canvas>();
                    if (mainCanvas != null && Camera.main != null) {
                        FloatingText.CreateWorldToUI(
                            playerController.transform.position,
                            mainCanvas,
                            Camera.main,
                            $"+{ropesToAdd} Ropes!"
                        );
                    }
                }
                break;
        }
    }
    
    void UpdateMoneyDisplay()
    {
        if (playerMoneyText != null)
            playerMoneyText.text = "Money: " + gameManager.money;
    }
    
    // Creates StoreItem instances from ScriptableObject data
    void SetupStoreItems()
    {
        availableItems.Clear();
        
        // If we have defined scriptable objects, use them
        if (storeItemsData != null && storeItemsData.Length > 0)
        {
            if (!randomizeItems)
            {
                // Add all items if not randomizing
                foreach (var itemData in storeItemsData)
                {
                    if (itemData != null)
                    {
                        availableItems.Add(new StoreItem(itemData, GetDifficultyLevel()));
                    }
                }
            }
            else
            {
                // Randomization will be handled when opening the store
                SelectRandomStoreItems();
            }
            
            // Ensure the rope item is always in the store
            EnsureSpareRopesAvailable();
        }
        else
        {
            // Fallback to default items
            SetupDefaultItems();
        }
    }
    
    // Setup default items if no scriptable objects provided
    void SetupDefaultItems()
    {
        // Create default items
        availableItems.Add(new StoreItem {
            itemID = "hook_upgrade",
            itemName = "Hook Engine Upgrade",
            description = "Faster retrieval speed by 20%",
            price = 200
        });
        
        availableItems.Add(new StoreItem {
            itemID = "time_extension",
            itemName = "Time Extension",
            description = "Adds 30 seconds to the game",
            price = 300
        });
        
        availableItems.Add(new StoreItem {
            itemID = "gold_polisher",
            itemName = "Gold Polisher",
            description = "Gold is worth 25% more",
            price = 500
        });
        
        // Always add spare ropes as a default item with level-based pricing
        int ropePrice = gameManager != null ? 50 + (gameManager.currentLevel * 10) : 60;
        availableItems.Add(new StoreItem {
            itemID = "spare_ropes",
            itemName = "Spare Ropes",
            description = "+1 rope",
            price = ropePrice
        });
    }
    
    // Get all available item IDs from the storeItemsData
    public string[] GetAllItemIDs()
    {
        if (storeItemsData == null || storeItemsData.Length == 0)
            return new string[0];
            
        List<string> itemIDs = new List<string>();
        
        foreach (var itemData in storeItemsData)
        {
            if (itemData != null && !string.IsNullOrEmpty(itemData.itemID))
            {
                itemIDs.Add(itemData.itemID);
            }
        }
        
        return itemIDs.ToArray();
    }
    
    // Set an exact number of items to display in the store
    public void SetExactItemCount(int count)
    {
        // Ensure count is within valid range
        count = Mathf.Clamp(count, 1, 10);
        
        // Set both min and max to the same value
        minItemCount = count;
        maxItemCount = count;
    }
}
