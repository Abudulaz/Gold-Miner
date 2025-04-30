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
        Store,
        NextLevel
    }
    
    [Header("Game Settings")]
    public int money = 0;  // Renamed from score to money
    public float gameTime = 60.0f;
    public float timeRemaining { get; private set; }
    [HideInInspector] public GameState currentState = GameState.Running;
    public int currentLevel = 1;
    
    // Event for state changes
    public delegate void GameStateChangeHandler(GameState newState);
    public event GameStateChangeHandler OnGameStateChanged;
    
    [Header("Spawning")]
    public GameObject goldPrefab;
    public GameObject rockPrefab;
    public int initialObjectCount = 10; // Note: Minimums might exceed this
    public float spawnAreaWidth = 8.0f;
    public float spawnAreaHeight = 4.0f;
    public float spawnAreaYOffset = -3.0f;
    public float minSpawnDistance = 1.5f; // Minimum distance between objects
    public int maxSpawnAttempts = 20; // Limit attempts to find a valid position

    // Minimum counts for specific objects per level
    [Header("Minimum Spawns Per Level")]
    public int minGoldCount = 5;
    public int minRockCount = 5;
    public int minRedwormCount = 1;
    
    // Used to track all spawned object positions for collision prevention
    private List<Vector3> spawnedPositions = new List<Vector3>();
    
    [Header("Opposition Prefabs")]
    public GameObject redwormPrefab;
    public GameObject ropeStressPrefab;
    
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
        SpawnLevelObjects(); // Renamed from SpawnInitialObjects
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
                    EndLevel();
                }
            }
        }
    }
    
    // Method to control game state
    public void SetGameState(GameState newState)
    {
        currentState = newState;
        
        // If transitioning to NextLevel state, prepare the next level
        if (newState == GameState.NextLevel)
        {
            StartNextLevel();
        }
        
        // Notify other systems about state change
        OnGameStateChanged?.Invoke(newState);
    }

    // Prepare and start the next level
    private void StartNextLevel()
    {
        // Increment the level counter
        currentLevel++;
        
        // Reset the timer
        timeRemaining = gameTime;
        
        // Clear existing objects and spawn new ones
        ClearAllCollectibles();
        SpawnLevelObjects(); // Use the updated spawn method
        
        // Transition back to Running state
        SetGameState(GameState.Running);
    }
    
    // Clear all collectible objects from the scene
    private void ClearAllCollectibles()
    {
        // Find and destroy all collectibles
        Collectible[] collectibles = FindObjectsOfType<Collectible>();
        foreach (Collectible c in collectibles)
        {
             if (c != null) Destroy(c.gameObject);
        }

        // Find and destroy RopeStress objects if they exist
        RopeStress[] stressObjects = FindObjectsOfType<RopeStress>();
        foreach (RopeStress rs in stressObjects)
        {
            if (rs != null) Destroy(rs.gameObject);
        }
        
        // Clear the spawned positions list
        spawnedPositions.Clear();
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
    
    // Spawns objects for the current level, ensuring minimum counts
    void SpawnLevelObjects()
    {
        spawnedPositions.Clear(); // Ensure list is clear before spawning
        int spawnedCount = 0;

        // --- Spawn Guaranteed Minimums --- 

        // Spawn minimum gold
        for (int i = 0; i < minGoldCount; i++)
        {
            if (TrySpawnSpecificObject(SpawnGold)) { spawnedCount++; }
        }

        // Spawn minimum rocks
        for (int i = 0; i < minRockCount; i++)
        {
             if (TrySpawnSpecificObject(SpawnRock)) { spawnedCount++; }
    }
    
        // Spawn minimum redworms (if prefab exists)
        if (redwormPrefab != null) {
             for (int i = 0; i < minRedwormCount; i++)
             {
                  if (TrySpawnSpecificObject(SpawnRedworm)) { spawnedCount++; }
             }
        }
        else if (minRedwormCount > 0) {
             Debug.LogWarning("Attempted to spawn minimum redworms, but prefab is missing.");
        }

        // --- Spawn Remaining Random Objects --- 
        int remainingToSpawn = initialObjectCount - spawnedCount;

        for (int i = 0; i < remainingToSpawn; i++)
        {
            if (TrySpawnRandomObject()) { spawnedCount++; }
        }

        // Log if minimums exceeded initialObjectCount
        if (spawnedCount > initialObjectCount) {
             Debug.LogWarning($"Spawned {spawnedCount} objects due to minimum requirements, exceeding initialObjectCount of {initialObjectCount}.");
        }
        else {
             Debug.Log($"Spawned a total of {spawnedCount} objects for level {currentLevel}.");
        }
    }

    // Delegate type for spawning functions that take a position
    private delegate GameObject SpawnFunction(Vector3 position);

    // Tries to find a valid position and spawn a specific object using the provided function
    private bool TrySpawnSpecificObject(SpawnFunction spawnFunc)
    {
        if (TryFindValidSpawnPosition(out Vector3 position))
        {
            GameObject spawned = spawnFunc(position);
            if (spawned != null) {
                spawnedPositions.Add(position);
                return true;
            }
            else {
                 Debug.LogError($"Spawn function {spawnFunc.Method.Name} failed to return an object at {position}");
                 return false;
            }
        }
        else
        {
            Debug.LogWarning($"Could not find valid position to spawn object using {spawnFunc.Method.Name}.");
            return false;
        }
    }

    // Tries to find a valid position and spawn a random object
    private bool TrySpawnRandomObject()
    {
         if (TryFindValidSpawnPosition(out Vector3 position))
        {
            GameObject spawned = SpawnRandomObjectLogic(position); // Call the logic part
             if (spawned != null) {
                spawnedPositions.Add(position);
                return true;
            }
            else {
                 Debug.LogError($"SpawnRandomObjectLogic failed to return an object at {position}");
                 return false;
            }
        }
        else
        {
            Debug.LogWarning("Could not find valid position for random object spawn.");
            return false;
        }
    }

    // Attempts to find a valid spawn position that doesn't overlap
    private bool TryFindValidSpawnPosition(out Vector3 position)
    {
        int attempts = 0;
        while (attempts < maxSpawnAttempts)
        {
            float x = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
            float y = Random.Range(-spawnAreaHeight / 2, spawnAreaHeight / 2) + spawnAreaYOffset;
            position = new Vector3(x, y, 0);
            
            if (IsValidSpawnPosition(position))
            {
                return true;
            }
            attempts++;
        }

        // Failed to find a position after max attempts
        position = Vector3.zero; // Default value
        Debug.LogWarning($"Failed to find valid spawn position after {maxSpawnAttempts} attempts.");
        return false;
    }
    
    // Decides which random object to spawn at the given position
    private GameObject SpawnRandomObjectLogic(Vector3 position)
    {
        // Decide what type of object to spawn based on our opposition matrix
        float randomValue = Random.value;
        GameObject spawnedObject = null;
        
        // Check if we should spawn rope stress objects
        bool spawnRopeStress = ropeStressPrefab != null && FindObjectOfType<RopeManager>() != null;
        
        // Scale redworm probability based on current level
        float redwormProbability = 0.15f + (currentLevel - 1) * 0.01f; // Increases by 1% per level
        redwormProbability = Mathf.Clamp(redwormProbability, 0.15f, 0.35f); // Cap between 15% and 35%
        
        // Calculate new probability thresholds based on adjusted redworm chance
        float rockThreshold = 0.50f; // Rock chance remains constant at 50%
        float goldThreshold = rockThreshold + (0.75f - rockThreshold - redwormProbability); // Gold gets the remaining space
        float redwormThreshold = goldThreshold + redwormProbability;
        
        // Log the current spawn distribution
        // if (Time.frameCount % 300 == 0) { // Log only occasionally to avoid spam
        //     Debug.Log($"Level {currentLevel} random spawn distribution - Rock: 0-{rockThreshold:F2}, " +
        //               $"Gold: {rockThreshold:F2}-{goldThreshold:F2}, " +
        //               $"Redworm: {goldThreshold:F2}-{redwormThreshold:F2}, " +
        //               $"Stress: {redwormThreshold:F2}-1.0");
        // }
        
        // Adjusted spawn distribution that scales with level:
        if (randomValue < rockThreshold && rockPrefab != null) {
            spawnedObject = SpawnRock(position);
        } 
        else if (randomValue < goldThreshold && goldPrefab != null) {
            spawnedObject = SpawnGold(position);
        } 
        else if (randomValue < redwormThreshold && redwormPrefab != null) {
            spawnedObject = SpawnRedworm(position);
        } 
        else if (spawnRopeStress) {
            try {
                spawnedObject = SpawnRopeStress(position);
            }
            catch (System.Exception e) {
                Debug.LogError("Failed to spawn rope stress: " + e.Message);
                if (goldPrefab != null) spawnedObject = SpawnGold(position);
                else if (rockPrefab != null) spawnedObject = SpawnRock(position);
            }
        }
        else if (goldPrefab != null) {
            spawnedObject = SpawnGold(position);
        }
        else if (rockPrefab != null) {
            spawnedObject = SpawnRock(position);
        }
        else {
            Debug.LogError("No valid prefabs assigned for random spawning!");
            return null;
        }
        
        return spawnedObject;
    }
    
    // Helper method to spawn a rock with random size at a specific position
    private GameObject SpawnRock(Vector3 position)
    {
        if (rockPrefab == null) return null;
        GameObject rock = Instantiate(rockPrefab, position, Quaternion.identity);
        
        Rock rockComponent = rock.GetComponent<Rock>();
        if (rockComponent != null)
        {
            float randomValue = Random.value;
            if (randomValue < 0.5f) { rockComponent.size = Rock.RockSize.Small; }
            else if (randomValue < 0.8f) { rockComponent.size = Rock.RockSize.Medium; }
            else { rockComponent.size = Rock.RockSize.Large; }
        }
        return rock;
    }
    
    // Helper method to spawn a redworm with random speed at a specific position
    private GameObject SpawnRedworm(Vector3 position)
    {
        if (redwormPrefab == null) return null;
        GameObject redworm = Instantiate(redwormPrefab, position, Quaternion.identity);
        
        Redworm redwormComponent = redworm.GetComponent<Redworm>();
        if (redwormComponent != null)
        {
            float randomValue = Random.value;
            float fastChance = Mathf.Min(0.2f + (currentLevel - 1) * 0.02f, 0.6f);
            float mediumChance = Mathf.Min(0.3f + (currentLevel - 1) * 0.01f, 0.5f);
            
            if (randomValue < 1 - (fastChance + mediumChance)) {
                redwormComponent.speedType = Redworm.RedwormSpeed.Slow;
                // Debug.Log($"Spawned Slow Redworm at level {currentLevel}");
            } else if (randomValue < 1 - fastChance) {
                redwormComponent.speedType = Redworm.RedwormSpeed.Medium;
                // Debug.Log($"Spawned Medium Redworm at level {currentLevel}");
            } else {
                redwormComponent.speedType = Redworm.RedwormSpeed.Fast;
                // Debug.Log($"Spawned Fast Redworm at level {currentLevel}");
            }
        }
        return redworm;
    }
    
    // Helper method to spawn a rope stress object with random level at a specific position
    private GameObject SpawnRopeStress(Vector3 position)
    {
        if (ropeStressPrefab == null || FindObjectOfType<RopeManager>() == null) return null;

        GameObject stressObj = Instantiate(ropeStressPrefab, position, Quaternion.identity);
        RopeStress stressComponent = stressObj.GetComponent<RopeStress>();
        if (stressComponent != null)
        {
            try {
                float randomValue = Random.value;
                // Using public setters or methods would be preferable to reflection if possible
                // Assuming RopeStress component has been updated or reflection is necessary:
                var stressTypeField = stressComponent.GetType().GetField("stressType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var stressImpactField = stressComponent.GetType().GetField("stressImpact", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                
                if (stressTypeField == null || stressImpactField == null) {
                    Debug.LogError("Could not find stressType or stressImpact fields via reflection in RopeStress.");
                    // Set defaults directly if possible, or just return the object as is
                }
                else {
                if (randomValue < 0.5f) {
                        stressTypeField.SetValue(stressComponent, RopeStress.StressType.Pressure);
                        stressImpactField.SetValue(stressComponent, 0.1f);
                } else if (randomValue < 0.8f) {
                        stressTypeField.SetValue(stressComponent, RopeStress.StressType.Tension);
                        stressImpactField.SetValue(stressComponent, 0.2f);
                } else {
                        stressTypeField.SetValue(stressComponent, RopeStress.StressType.Shear);
                        stressImpactField.SetValue(stressComponent, 0.3f);
                    }
                }
            }
            catch (System.Exception e) {
                Debug.LogError("Error configuring RopeStress via reflection: " + e.Message);
            }
        }
        return stressObj;
    }
    
    // Helper method to spawn a gold with proper size at a specific position
    private GameObject SpawnGold(Vector3 position)
    {
        if (goldPrefab == null) return null;
        GameObject gold = Instantiate(goldPrefab, position, Quaternion.identity);
        
        Gold goldComponent = gold.GetComponent<Gold>();
        if (goldComponent != null)
        {
            float randomValue = Random.value;
            if (randomValue < 0.5f) { goldComponent.size = Gold.GoldSize.Small; }
            else if (randomValue < 0.8f) { goldComponent.size = Gold.GoldSize.Medium; }
            else { goldComponent.size = Gold.GoldSize.Large; }
        }
        return gold;
    }
    
    // Check if a position is far enough from other spawned objects
    private bool IsValidSpawnPosition(Vector3 position)
    {
        foreach (Vector3 existingPos in spawnedPositions) {
            if (Vector3.Distance(position, existingPos) < minSpawnDistance) {
                return false; // Too close to another object
            }
        }
        return true;
    }
    
    // Draw gizmos to visualize the spawn area in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        Vector3 center = transform.position + new Vector3(0, spawnAreaYOffset, 0);
        Vector3 size = new Vector3(spawnAreaWidth, spawnAreaHeight, 0.1f);
        Gizmos.DrawCube(center, size);
        
        Gizmos.color = new Color(0, 1, 0, 0.8f);
        Gizmos.DrawWireCube(center, size);
        
        if (Application.isPlaying && spawnedPositions != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector3 pos in spawnedPositions)
            {
                Gizmos.DrawSphere(pos, 0.2f);
            }
        }
    }
    
    public void AddMoney(int amount)  // Renamed from AddScore to AddMoney
    {
        money += amount;
        Debug.Log("Money: " + money);
    }
    
    void EndLevel()  // Renamed from EndGame to EndLevel
    {
        Debug.Log("Level " + currentLevel + " completed! Money: " + money);
        
        if (money > 0)  // Changed from >= to >
        {
            SetGameState(GameState.Store);
        }
        else
        {
            SetGameState(GameState.GameOver);
        }
    }
}
