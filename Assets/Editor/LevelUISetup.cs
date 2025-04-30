using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

public class LevelUISetup : EditorWindow
{
    [MenuItem("Gold Miner/Setup/Add Level UI")]
    public static void AddLevelUI()
    {
        // Find the main canvas
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("No Canvas found in the scene. Please create a Canvas first.");
            return;
        }
        
        // Find UIManager
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("No UIManager found in the scene. Please add a UIManager to your Canvas.");
            return;
        }
        
        // Load the prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/LevelTextUI.prefab");
        if (prefab == null)
        {
            // Create a new one if prefab doesn't exist
            GameObject levelTextObj = new GameObject("LevelText");
            levelTextObj.transform.SetParent(mainCanvas.transform, false);
            
            // Add components
            RectTransform rectTransform = levelTextObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(150, -80);
            rectTransform.sizeDelta = new Vector2(200, 50);
            
            // Add TextMeshProUGUI component
            TextMeshProUGUI levelText = levelTextObj.AddComponent<TextMeshProUGUI>();
            levelText.text = "Level: 1";
            levelText.fontSize = 36;
            levelText.fontStyle = FontStyles.Bold;
            levelText.alignment = TextAlignmentOptions.Left;
            
            // Set the TextMeshProUGUI reference in UIManager
            uiManager.levelText = levelText;
            
            Debug.Log("Level UI added to Canvas and linked to UIManager successfully!");
        }
        else
        {
            // Instantiate the prefab
            GameObject levelTextObj = Instantiate(prefab, mainCanvas.transform);
            levelTextObj.name = "LevelText";
            
            // Get the TextMeshProUGUI component
            TextMeshProUGUI levelText = levelTextObj.GetComponent<TextMeshProUGUI>();
            
            // Set the TextMeshProUGUI reference in UIManager
            uiManager.levelText = levelText;
            
            Debug.Log("Level UI prefab added to Canvas and linked to UIManager successfully!");
        }
        
        // Mark UIManager as dirty so Unity saves the changes
        EditorUtility.SetDirty(uiManager);
    }
} 