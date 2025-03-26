using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
public class CreateDynamitePrefab : EditorWindow
{
    [MenuItem("Tools/Create Dynamite Prefab")]
    static void CreatePrefab()
    {
        // Create a primitive sphere for the dynamite
        GameObject dynamite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dynamite.name = "DynamitePrefab";
        
        // Scale it down slightly
        dynamite.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        
        // Add the Dynamite script
        dynamite.AddComponent<Dynamite>();
        
        // Get the renderer and set the color to red
        Renderer renderer = dynamite.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
        
        // Make sure the Prefabs folder exists
        if (!Directory.Exists("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Create the prefab
        string prefabPath = "Assets/Prefabs/DynamitePrefab.prefab";
        
        // Make sure to delete any existing prefab at this path
        AssetDatabase.DeleteAsset(prefabPath);
        
        // Create the prefab
        #if UNITY_2018_3_OR_NEWER
        PrefabUtility.SaveAsPrefabAsset(dynamite, prefabPath);
        #else
        PrefabUtility.CreatePrefab(prefabPath, dynamite);
        #endif
        
        // Destroy the temp GameObject
        DestroyImmediate(dynamite);
        
        // Select the created prefab in the Project window
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        // Show a confirmation message
        EditorUtility.DisplayDialog("Prefab Created", "Dynamite prefab has been created at " + prefabPath, "OK");
        
        // Print instructions on how to use the prefab
        Debug.Log("Dynamite prefab created! Drag it onto the 'Dynamite Prefab' field in the GameManager script in the Inspector.");
    }
}
#endif
