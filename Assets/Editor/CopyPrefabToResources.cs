using UnityEngine;
using UnityEditor;
using System.IO;

public class CopyPrefabToResources : Editor
{
    [MenuItem("Tools/Setup/Copy FloatingTextPrefab to Resources")]
    static void CopyFloatingTextPrefab()
    {
        // Define source and destination paths
        string sourcePath = "Assets/Prefabs/FloatingTextPrefab.prefab";
        string destinationPath = "Assets/Resources/FloatingTextPrefab.prefab";
        
        // Check if source exists
        if (!File.Exists(Path.Combine(Application.dataPath, "../" + sourcePath)))
        {
            EditorUtility.DisplayDialog("Error", "FloatingTextPrefab not found in Prefabs folder!", "OK");
            return;
        }
        
        // Create Resources directory if it doesn't exist
        string resourcesDir = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(resourcesDir))
        {
            Directory.CreateDirectory(resourcesDir);
            AssetDatabase.Refresh();
        }
        
        // Copy the prefab
        bool success = AssetDatabase.CopyAsset(sourcePath, destinationPath);
        
        if (success)
        {
            Debug.Log("Successfully copied FloatingTextPrefab to Resources folder");
            EditorUtility.DisplayDialog("Success", "FloatingTextPrefab copied to Resources folder!", "OK");
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("Failed to copy FloatingTextPrefab to Resources folder");
            EditorUtility.DisplayDialog("Error", "Failed to copy FloatingTextPrefab to Resources folder!", "OK");
        }
    }
} 