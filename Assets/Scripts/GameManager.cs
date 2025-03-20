using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public int score = 0;
    public float gameTime = 60.0f;
    public float timeRemaining { get; private set; }
    
    [Header("Spawning")]
    public GameObject goldPrefab;
    public GameObject rockPrefab;
    public int initialObjectCount = 10;
    public float spawnAreaWidth = 8.0f;
    public float spawnAreaHeight = 4.0f;
    public float spawnAreaYOffset = -3.0f;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        timeRemaining = gameTime;
        SpawnInitialObjects();
    }
    
    void Update()
    {
        // Update game timer
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            
            // Check if time is up
            if (timeRemaining <= 0)
            {
                EndGame();
            }
        }
    }
    
    void SpawnInitialObjects()
    {
        for (int i = 0; i < initialObjectCount; i++)
        {
            SpawnRandomObject();
        }
    }
    
    void SpawnRandomObject()
    {
        // Determine random position within spawn area
        float x = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
        float y = Random.Range(-spawnAreaHeight / 2, spawnAreaHeight / 2) + spawnAreaYOffset;
        Vector3 position = new Vector3(x, y, 0);
        
        // Determine random size
        float size = Random.Range(0.5f, 1.5f);
        
        // Determine whether to spawn gold or rock (30% chance for gold)
        GameObject prefabToSpawn = Random.value < 0.3f ? goldPrefab : rockPrefab;
        
        // Create the object
        if (prefabToSpawn != null)
        {
            GameObject obj = Instantiate(prefabToSpawn, position, Quaternion.identity);
            obj.transform.localScale = new Vector3(size, size, size);
        }
        else
        {
            Debug.LogError("Prefab not assigned in GameManager!");
        }
    }
    
    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Score: " + score);
    }
    
    void EndGame()
    {
        Debug.Log("Game Over! Final Score: " + score);
        // Here you would typically show game over UI, etc.
    }
}
