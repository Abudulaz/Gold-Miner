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
    
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        
        // Hide game over panel initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
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
                
                // Show game over panel when time is up
                if (timeRemaining <= 0 && gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                }
            }
        }
    }
}
