using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float lifetime = 0.5f;
    public float expandSpeed = 2.0f;
    public Color startColor = Color.yellow;
    public Color endColor = new Color(1, 0, 0, 0); // Red that fades to transparent
    
    private float startTime;
    private Renderer rend;
    private float initialScale;
    
    void Start()
    {
        startTime = Time.time;
        rend = GetComponent<Renderer>();
        
        if (rend == null)
        {
            // Create a sphere for the explosion if this is just a script
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform);
            sphere.transform.localPosition = Vector3.zero;
            rend = sphere.GetComponent<Renderer>();
            
            // Remove the collider from the visual effect
            Collider collider = sphere.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
        }
        
        // Set the initial color
        if (rend != null && rend.material != null)
        {
            rend.material.color = startColor;
        }
        
        initialScale = transform.localScale.x;
    }
    
    void Update()
    {
        float elapsedTime = Time.time - startTime;
        float normalizedTime = Mathf.Clamp01(elapsedTime / lifetime);
        
        // Scale up the explosion
        float currentScale = initialScale + (expandSpeed * normalizedTime);
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);
        
        // Change color over time
        if (rend != null && rend.material != null)
        {
            rend.material.color = Color.Lerp(startColor, endColor, normalizedTime);
        }
        
        // Destroy after lifetime
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    // Static method to create an explosion at a specific position
    public static GameObject CreateExplosion(Vector3 position, float size = 1.0f)
    {
        GameObject explosion = new GameObject("ExplosionEffect");
        explosion.transform.position = position;
        explosion.transform.localScale = Vector3.one * size;
        
        ExplosionEffect effect = explosion.AddComponent<ExplosionEffect>();
        return explosion;
    }
}
