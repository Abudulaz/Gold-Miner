using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dynamite : Collectible
{
    [Header("Explosion Settings")]
    public float explosionRadius = 1.5f;
    public int damageAmount = 50;
    public float explosionForce = 500f;
    public GameObject explosionEffectPrefab;
    
    void Start()
    {
        // Dynamite has negative value - costs the player money when caught
        value = -damageAmount;
        weight = 0.5f; // Dynamite is light, moves fast
        
        // Set dynamite appearance
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
    }
    
    // Override the AttachToHook method to trigger explosion instead of being collected
    public override void AttachToHook(Transform hookTransform)
    {
        // Start the explosion process
        StartCoroutine(ExplodeWithDelay(0.2f));
        
        // Still attach to the hook so it gets pulled back
        transform.parent = hookTransform;
    }
    
    private IEnumerator ExplodeWithDelay(float delay)
    {
        // Short delay before explosion for effect
        yield return new WaitForSeconds(delay);
        
        // Trigger explosion
        Explode();
    }
    
    private void Explode()
    {
        Debug.Log("Dynamite exploding!");
        
        // Spawn explosion effect if provided
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Apply damage to player's score
        GameManager.Instance.AddScore(value); // Value is negative
        
        // Apply explosion force to nearby objects (future enhancement)
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        
        // Destroy the dynamite
        Destroy(gameObject);
    }
    
    // Visualize explosion radius in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
