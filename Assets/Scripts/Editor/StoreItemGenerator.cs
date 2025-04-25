using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class StoreItemGenerator : EditorWindow
{
    private TextAsset csvFile;
    private string outputFolder = "Assets/Resources/StoreItems";
    private bool overwriteExisting = false;
    
    [MenuItem("Gold Miner/Store Item Generator")]
    public static void ShowWindow()
    {
        GetWindow<StoreItemGenerator>("Store Item Generator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Store Item Generator", EditorStyles.boldLabel);
        
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV Data File", csvFile, typeof(TextAsset), false);
        
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        
        if (GUILayout.Button("Browse..."))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (path.Length != 0)
            {
                // Convert to relative path if it's within the Assets folder
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                outputFolder = path;
            }
        }
        
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);
        
        EditorGUILayout.Space();
        
        GUI.enabled = csvFile != null;
        if (GUILayout.Button("Generate Store Items"))
        {
            GenerateStoreItems();
        }
        GUI.enabled = true;
    }
    
    void GenerateStoreItems()
    {
        if (csvFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a CSV file!", "OK");
            return;
        }
        
        // Ensure the output directory exists
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        
        // Parse CSV
        string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        // Find the "Item Name" section in the CSV
        int itemStartLine = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("Item Name,"))
            {
                itemStartLine = i;
                break;
            }
        }
        
        if (itemStartLine == -1)
        {
            EditorUtility.DisplayDialog("Error", "Could not find 'Item Name' section in the CSV file!", "OK");
            return;
        }
        
        // Start parsing from the line after the header
        int itemCount = 0;
        for (int i = itemStartLine + 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
                
            string[] values = SplitCSVLine(line);
            if (values.Length < 4) continue; // Need at least item name and 3 variants
            
            string itemName = values[0].Trim();
            if (string.IsNullOrEmpty(itemName))
                continue;
                
            // Create the scriptable object
            CreateStoreItem(itemName, values);
            itemCount++;
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Generated {itemCount} store items!", "OK");
    }
    
    string[] SplitCSVLine(string line)
    {
        List<string> values = new List<string>();
        bool inQuotes = false;
        string currentValue = "";
        
        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue);
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }
        
        values.Add(currentValue); // Add the last value
        return values.ToArray();
    }
    
    void CreateStoreItem(string itemName, string[] values)
    {
        // Generate item ID from name
        string itemID = itemName.ToLower().Replace(" ", "_");
        
        // Create the scriptable object
        StoreItemData itemData = ScriptableObject.CreateInstance<StoreItemData>();
        itemData.itemID = itemID;
        itemData.itemName = itemName;
        
        // Set up the variants (Easy, Medium, Hard)
        itemData.variants = new StoreItemVariant[3];
        
        for (int i = 0; i < 3; i++)
        {
            if (i + 1 < values.Length)
            {
                float effectValue = 0;
                string variantDesc = values[i + 1].Trim();
                
                // Try to extract numeric value from description
                string[] parts = variantDesc.Split('%');
                if (parts.Length > 1)
                {
                    string numPart = parts[0].Trim();
                    if (numPart.StartsWith("+"))
                        numPart = numPart.Substring(1);
                    float.TryParse(numPart, out effectValue);
                }
                
                itemData.variants[i] = new StoreItemVariant
                {
                    description = $"{itemName} - {GetDifficultyName(i)}",
                    effectDescription = variantDesc,
                    price = DeterminePrice(itemName, i),
                    effectValue = effectValue != 0 ? effectValue : DetermineEffectValue(itemName, i),
                    secondaryValue = 0
                };
            }
            else
            {
                itemData.variants[i] = new StoreItemVariant
                {
                    description = $"{itemName} - {GetDifficultyName(i)}",
                    effectDescription = "No effect description available",
                    price = DeterminePrice(itemName, i),
                    effectValue = DetermineEffectValue(itemName, i),
                    secondaryValue = 0
                };
            }
        }
        
        // Save the asset
        string assetPath = $"{outputFolder}/{itemName.Replace(" ", "")}.asset";
        if (overwriteExisting || !File.Exists(assetPath))
        {
            AssetDatabase.CreateAsset(itemData, assetPath);
            Debug.Log($"Created store item: {assetPath}");
        }
        else
        {
            Debug.Log($"Skipped existing store item: {assetPath}");
        }
    }
    
    string DetermineEmojiIcon(string itemName)
    {
        // This method is kept for future reference but no longer used
        // Map item names to appropriate emoji icons
        Dictionary<string, string> emojiMap = new Dictionary<string, string>
        {
            { "Dynamite", "💣" },
            { "Strength Drink", "💪" },
            { "Lucky Clover", "🍀" },
            { "Rock Collector's Book", "📚" },
            { "Diamond Polish", "💎" },
            { "Rope Reinforcement", "🧵" },
            { "Time Extension", "⏱️" },
            { "Better Rope", "🪢" },
            { "Strength Power-up", "💪" },
            { "Improved Claw", "⚙️" }
        };
        
        // Return the emoji if found, otherwise a generic one
        if (emojiMap.TryGetValue(itemName, out string emoji))
            return emoji;
            
        return "🔧"; // Default emoji
    }
    
    int DeterminePrice(string itemName, int difficulty)
    {
        // Base prices that increase with difficulty
        int basePrice = 200;
        
        // Adjust base price based on item type
        switch (itemName)
        {
            case "Dynamite":
            case "Time Extension":
                basePrice = 300;
                break;
                
            case "Diamond Polish":
            case "Better Rope":
                basePrice = 500;
                break;
                
            case "Lucky Clover":
            case "Strength Power-up":
                basePrice = 400;
                break;
        }
        
        // Increase price based on difficulty
        return basePrice * (difficulty + 1);
    }
    
    float DetermineEffectValue(string itemName, int difficulty)
    {
        // Base effect values that increase with difficulty
        float baseValue = 10.0f;
        
        // Default difficulty multipliers
        float[] difficultyMultipliers = { 1.0f, 2.0f, 4.0f };
        
        // Adjust base value based on item type
        switch (itemName)
        {
            case "Time Extension":
                baseValue = 5.0f; // Seconds
                break;
                
            case "Strength Drink":
            case "Strength Power-up":
                baseValue = 10.0f; // Percentage
                break;
                
            case "Diamond Polish":
                baseValue = 10.0f; // Percentage
                break;
                
            case "Rope Reinforcement":
                baseValue = 20.0f; // Percentage stress reduction
                break;
                
            case "Better Rope":
                baseValue = 20.0f; // Percentage stress limit increase
                break;
        }
        
        // Return scaled value based on difficulty
        return baseValue * difficultyMultipliers[difficulty];
    }
    
    string GetDifficultyName(int difficulty)
    {
        switch (difficulty)
        {
            case 0: return "Easy";
            case 1: return "Medium";
            case 2: return "Hard";
            default: return "Unknown";
        }
    }
} 