using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI ropesCountText;  // UI element to display rope count
    public TextMeshProUGUI levelText;       // UI element to display current level
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    
    [Header("Game Over UI")]
    public TextMeshProUGUI finalMoneyText;
    public Button restartButton;
    
    private GameManager gameManager;
    private RopeManager ropeManager;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        
        // Subscribe to game state changes
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += HandleGameStateChanged;
        }
        
        // Find RopeManager
        ropeManager = FindObjectOfType<RopeManager>();
        
        // Subscribe to rope count changes
        if (ropeManager != null && ropesCountText != null)
        {
            // Set initial rope count display
            UpdateRopesCountUI(ropeManager.RopesCount);
            
            // Subscribe to rope count changes
            ropeManager.OnRopeCountChanged += UpdateRopesCountUI;
            
            // Assign the text element to RopeManager
            ropeManager.ropesCountText = ropesCountText;
        }
        
        // Hide UI panels initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        // Add restart button listener
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        // Unsubscribe from rope count changes
        if (ropeManager != null)
        {
            ropeManager.OnRopeCountChanged -= UpdateRopesCountUI;
        }
    }
    
    // Update rope count display
    private void UpdateRopesCountUI(int count)
    {
        if (ropesCountText != null)
        {
            ropesCountText.text = "Ropes: " + count;
        }
    }
    
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        // Handle UI changes based on game state
        switch (newState)
        {
            case GameManager.GameState.Running:
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
                if (pausePanel != null) pausePanel.SetActive(false);
                break;
                
            case GameManager.GameState.Paused:
                if (pausePanel != null) pausePanel.SetActive(true);
                break;
                
            case GameManager.GameState.GameOver:
                ShowGameOver();
                break;
                
            case GameManager.GameState.Store:
                // Store UI will be handled by StoreManager
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
                if (pausePanel != null) pausePanel.SetActive(false);
                break;
                
            case GameManager.GameState.NextLevel:
                // Handle UI for next level transition if needed
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
                if (pausePanel != null) pausePanel.SetActive(false);
                break;
        }
    }
    
    void Update()
    {
        if (gameManager != null)
        {
            // Update money text
            if (moneyText != null)
            {
                moneyText.text = "Money: " + gameManager.money;
            }
            
            // Update time text
            if (timeText != null)
            {
                float timeRemaining = gameManager.timeRemaining;
                timeText.text = "Time: " + Mathf.CeilToInt(timeRemaining);
            }
            
            // Update level text
            if (levelText != null)
            {
                levelText.text = "Level: " + gameManager.currentLevel;
            }
            
            // Check for pause input
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
        }
    }
    
    void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // Update final money display
            if (finalMoneyText != null)
            {
                finalMoneyText.text = "Final Money: " + gameManager.money;
            }
        }
    }
    
    void TogglePause()
    {
        if (gameManager.currentState == GameManager.GameState.Running)
        {
            gameManager.SetGameState(GameManager.GameState.Paused);
            Time.timeScale = 0f; // Pause game time
        }
        else if (gameManager.currentState == GameManager.GameState.Paused)
        {
            gameManager.SetGameState(GameManager.GameState.Running);
            Time.timeScale = 1f; // Resume game time
        }
    }
    
    public void RestartGame()
    {
        // Reset time scale
        Time.timeScale = 1f;
        
        // Reload current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
