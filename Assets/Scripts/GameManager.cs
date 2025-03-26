using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    // Game State Enum
    public enum GameState
    {
        Running,
        Paused,
        GameOver,
        Store
    }
    
    [Header("Game Settings")]
    public int score = 0;
    public float gameTime = 60.0f;
    public float timeRemaining { get; private set; }
    [HideInInspector] public GameState currentState = GameState.Running;
    
    // Event for state changes
    public delegate void GameStateChangeHandler(GameState newState);
    public event GameStateChangeHandler OnGameStateChanged;
    
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
        // Only update game timer when game is running
        if (currentState == GameState.Running)
        {
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
    }
    
    // Method to control game state
    public void SetGameState(GameState newState)
    {
        currentState = newState;
        
        // Notify other systems about state change
        if (OnGameStateChanged != null)
            OnGameStateChanged(newState);
    }
    
    // Method to add time (for store purchases)
    public void AddTime(float seconds)
    {
        timeRemaining += seconds;
        
        // If game was over, resume it
        if (currentState != GameState.Running && timeRemaining > 0)
        {
            SetGameState(GameState.Running);
        }
    }
    
    void SpawnInitialObjects()
    {
        for (int i = 0; i < initialObjectCount; i++)
        {
            SpawnRandomObject();
        }
    }
    
    [Header("Dynamite Settings")]
    public GameObject dynamitePrefab;
    public int dynamiteDamage = 50;
    
    // Used to track all collectibles for collision prevention
    private List<Vector3> spawnedPositions = new List<Vector3>();
    
    void SpawnRandomObject()
    {
        Vector3 position;
        bool validPosition = false;
        int maxAttempts = 20; // Limit attempts to prevent infinite loops
        int attempt = 0;
        
        // Keep trying until we find a valid position or reach max attempts
        do {
            // Determine random position within spawn area
            float x = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
            float y = Random.Range(-spawnAreaHeight / 2, spawnAreaHeight / 2) + spawnAreaYOffset;
            position = new Vector3(x, y, 0);
            
            // Check if position is far enough from other objects
            validPosition = IsValidSpawnPosition(position);
            attempt++;
        } while (!validPosition && attempt < maxAttempts);
        
        if (!validPosition) {
            Debug.Log("Could not find valid spawn position after " + maxAttempts + " attempts");
            return; // Skip spawning this object
        }
        
        // Add this position to our list
        spawnedPositions.Add(position);
        
        // Determine random size with more variation
        float size = Random.Range(0.5f, 2.5f);
        
        // Decide what type of object to spawn (gold, rock, or dynamite)
        float randomValue = Random.value;
        GameObject prefabToSpawn;
        
        if (randomValue < 0.1f && dynamitePrefab != null) {
            // 10% chance for dynamite
            prefabToSpawn = dynamitePrefab;
            // Dynamite has fixed smaller size
            size = Random.Range(0.5f, 0.8f);
        } else if (randomValue < 0.4f && goldPrefab != null) {
            // 30% chance for gold
            prefabToSpawn = goldPrefab;
        } else if (rockPrefab != null) {
            // 60% chance for rock
            prefabToSpawn = rockPrefab;
        } else {
            Debug.LogError("Prefab not assigned in GameManager!");
            return;
        }
        
        // Create the object
        GameObject obj = Instantiate(prefabToSpawn, position, Quaternion.identity);
        obj.transform.localScale = new Vector3(size, size, size);
        
        // Adjust properties based on size
        Collectible collectible = obj.GetComponent<Collectible>();
        if (collectible != null) {
            // Larger objects are worth more and weigh more
            collectible.value = Mathf.RoundToInt(collectible.value * size);
            collectible.weight = collectible.weight * size;
        }
    }
    
    // Check if a position is far enough from other spawned objects
    private bool IsValidSpawnPosition(Vector3 position)
    {
        float minDistance = 1.5f; // Minimum distance between objects
        
        foreach (Vector3 existingPos in spawnedPositions) {
            if (Vector3.Distance(position, existingPos) < minDistance) {
                return false; // Too close to another object
            }
        }
        
        return true;
    }
    
    // Draw gizmos to visualize the spawn area in the editor
    private void OnDrawGizmos()
    {
        // Set gizmo color for spawn area
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f); // Semi-transparent green
        
        // Calculate spawn area center and size based on the parameters
        Vector3 center = transform.position + new Vector3(0, spawnAreaYOffset, 0);
        Vector3 size = new Vector3(spawnAreaWidth, spawnAreaHeight, 0.1f);
        
        // Draw a cube representing the spawn area
        Gizmos.DrawCube(center, size);
        
        // Draw wireframe for better visibility
        Gizmos.color = new Color(0, 1, 0, 0.8f); // More solid green for outline
        Gizmos.DrawWireCube(center, size);
        
        // Optionally draw points for existing spawned objects in play mode
        if (Application.isPlaying && spawnedPositions != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector3 pos in spawnedPositions)
            {
                Gizmos.DrawSphere(pos, 0.2f);
            }
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
        
        if (score >= 0)
        {
            // If score is positive, go to store
            SetGameState(GameState.Store);
        }
        else
        {
            // If score is negative, just end the game
            SetGameState(GameState.GameOver);
        }
    }
}
