using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    
    [Header("Game Over UI")]
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        
        // Subscribe to game state changes
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += HandleGameStateChanged;
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
        }
    }
    
    void Update()
    {
        if (gameManager != null)
        {
            // Update score text
            if (scoreText != null)
            {
                scoreText.text = "Score: " + gameManager.score;
            }
            
            // Update time text
            if (timeText != null)
            {
                float timeRemaining = gameManager.timeRemaining;
                timeText.text = "Time: " + Mathf.CeilToInt(timeRemaining);
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
            
            // Update final score
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + gameManager.score;
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
