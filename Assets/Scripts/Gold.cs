using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gold : Collectible
{
    void Start()
    {
        // Gold is valuable but heavy
        value = 100;
        weight = 2.0f;
        
        // Set gold appearance
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().material.color = Color.yellow;
        }
        
        // Size affects value and weight
        float scale = transform.localScale.x;
        value = Mathf.RoundToInt(value * scale);
        weight = weight * scale;
    }
}
