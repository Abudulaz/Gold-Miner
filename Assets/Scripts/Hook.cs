using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    private PlayerController playerController;
    
    void Start()
    {
        // Find the player controller
        playerController = FindObjectOfType<PlayerController>();
        
        // Make sure we have a collider
        if (GetComponent<Collider>() == null)
        {
            // Add a sphere collider if none exists
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            collider.isTrigger = true;
        }
        
        // Add a Rigidbody if none exists to ensure proper physics interactions
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true; // We control the hook's movement through code
        }
        
        // Set the tag for collision detection
        gameObject.tag = "Hook";
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hook collided with: " + other.gameObject.name);
        
        // Check if we hit a collectible
        Collectible collectible = other.GetComponent<Collectible>();
        if (collectible != null)
        {
            Debug.Log("Found collectible: " + collectible.GetType().Name + " with value: " + collectible.value);
            
            if (playerController != null)
            {
                Debug.Log("Calling CatchObject on PlayerController");
                playerController.CatchObject(collectible);
            }
            else
            {
                Debug.LogError("PlayerController is null!");
            }
        }
        else
        {
            Debug.Log("Object does not have a Collectible component");
        }
    }
}
