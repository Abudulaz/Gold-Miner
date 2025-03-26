using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
public class CreateExplosionPrefab : EditorWindow
{
    [MenuItem("Tools/Create Explosion Effect Prefab")]
    static void CreatePrefab()
    {
        // Create a GameObject for the explosion
        GameObject explosion = new GameObject("ExplosionEffectPrefab");
        
        // Add the ExplosionEffect script
        ExplosionEffect effect = explosion.AddComponent<ExplosionEffect>();
        
        // Create a sphere for the visual effect
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "VisualEffect";
        sphere.transform.SetParent(explosion.transform);
        sphere.transform.localPosition = Vector3.zero;
        
        // Remove the collider from the visual effect
        DestroyImmediate(sphere.GetComponent<Collider>());
        
        // Get the renderer and set the color
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Create a new material that will work with transparency
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3); // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            
            // Set the color
            mat.color = effect.startColor;
            
            // Assign the material
            renderer.material = mat;
        }
        
        // Make sure the Prefabs folder exists
        if (!Directory.Exists("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Create the prefab
        string prefabPath = "Assets/Prefabs/ExplosionEffectPrefab.prefab";
        
        // Make sure to delete any existing prefab at this path
        AssetDatabase.DeleteAsset(prefabPath);
        
        // Create the prefab
        #if UNITY_2018_3_OR_NEWER
        PrefabUtility.SaveAsPrefabAsset(explosion, prefabPath);
        #else
        PrefabUtility.CreatePrefab(prefabPath, explosion);
        #endif
        
        // Destroy the temp GameObject
        DestroyImmediate(explosion);
        
        // Select the created prefab in the Project window
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        // Show a confirmation message
        EditorUtility.DisplayDialog("Prefab Created", "Explosion Effect prefab has been created at " + prefabPath, "OK");
        
        // Print instructions on how to use the prefab
        Debug.Log("Explosion Effect prefab created! Drag it onto the 'Explosion Effect Prefab' field in the Dynamite script in the Inspector.");
    }
}
#endif
