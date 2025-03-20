using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int value = 10;
    public float weight = 1.0f;
    private bool isCaught = false;
    
    void Start()
    {
        // Make sure we have a collider
        if (GetComponent<Collider>() == null)
        {
            // Add a collider if none exists
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            collider.isTrigger = true;
        }
        
        // Make sure we have a Rigidbody
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true; // We will control movement through code
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Prevent double-catching
        if (isCaught) return;
        
        Debug.Log(gameObject.name + " (Collectible) collided with: " + other.gameObject.name);
        
        if (other.CompareTag("Hook"))
        {
            Debug.Log("Collided with Hook tag");
            isCaught = true;
            
            // Find the player controller and notify it that we've been caught
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                Debug.Log("Calling CatchObject from Collectible");
                playerController.CatchObject(this);
            }
            else
            {
                Debug.LogError("PlayerController not found in Collectible.OnTriggerEnter");
            }
        }
        else
        {
            Debug.Log("Collided object does not have Hook tag. Its tag is: " + other.tag);
        }
    }
    
    // Make object follow the hook when caught
    public void AttachToHook(Transform hookTransform)
    {
        transform.parent = hookTransform;
    }
    
    // Detach from hook (called when collected)
    public void Detach()
    {
        transform.parent = null;
        isCaught = false;
    }
}
