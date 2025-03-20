using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : Collectible
{
    void Start()
    {
        // Rock is less valuable but lighter than gold
        value = 20;
        weight = 1.0f;
        
        // Set rock appearance
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().material.color = Color.gray;
        }
        
        // Size affects value and weight
        float scale = transform.localScale.x;
        value = Mathf.RoundToInt(value * scale);
        weight = weight * scale;
    }
}
