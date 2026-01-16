using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the game UI displaying deaths, time, and score
/// Also shows level completion screen
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("HUD Text References")]
    [Tooltip("Text component for deaths display")]
    public TextMeshProUGUI deathsText;
    
    [Tooltip("Text component for time display")]
    public TextMeshProUGUI timeText;
    
    [Tooltip("Text component for score display")]
    public TextMeshProUGUI scoreText;
    
    [Tooltip("Text component for collectible count")]
    public TextMeshProUGUI collectiblesText;

    [Header("Completion Screen")]
    [Tooltip("Panel/GameObject to show when level is completed")]
    public GameObject completionPanel;
    
    [Tooltip("Text for final stats on completion screen")]
    public TextMeshProUGUI completionStatsText;
    
    [Tooltip("Text for completion title")]
    public TextMeshProUGUI completionTitleText;

    [Header("Fallback (Legacy Text)")]
    [Tooltip("Use legacy UI.Text instead of TextMeshPro")]
    public bool useLegacyText = false;
    
    public Text legacyDeathsText;
    public Text legacyTimeText;
    public Text legacyScoreText;
    public Text legacyCollectiblesText;
    public Text legacyCompletionStatsText;
    public Text legacyCompletionTitleText;

    private void Start()
    {
        // Hide completion panel at start
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
        
        // Subscribe to ScoreManager events if available
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnLevelCompleted.AddListener(ShowCompletionScreen);
        }
        
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (ScoreManager.Instance == null) return;

        // Update deaths
        string deathsDisplay = $"Deaths: {ScoreManager.Instance.GetDeaths()}";
        SetText(deathsText, legacyDeathsText, deathsDisplay);

        // Update time
        string timeDisplay = $"Time: {ScoreManager.Instance.FormatTime(ScoreManager.Instance.GetTime())}";
        SetText(timeText, legacyTimeText, timeDisplay);

        // Update score
        string scoreDisplay = $"Score: {ScoreManager.Instance.GetScore()}";
        SetText(scoreText, legacyScoreText, scoreDisplay);

        // Update collectibles count
        if (CollectibleManager.Instance != null)
        {
            string collectiblesDisplay = $"Collectibles: {CollectibleManager.Instance.GetProgressString()}";
            SetText(collectiblesText, legacyCollectiblesText, collectiblesDisplay);
        }
    }

    private void ShowCompletionScreen()
    {
        // Hide HUD
        SetHudActive(false);

        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
        }

        // Update completion title
        SetText(completionTitleText, legacyCompletionTitleText, "LEVEL COMPLETE!");

        // Format final stats
        if (ScoreManager.Instance != null)
        {
            string stats = FormatCompletionStats();
            SetText(completionStatsText, legacyCompletionStatsText, stats);
        }
    }

    [Header("External UI References")]
    [Tooltip("Reference to the Dash UI GameObject to hide/show")]
    public GameObject dashUIObject;

    private void SetHudActive(bool active)
    {
        // Toggle TMP texts
        if (deathsText != null) deathsText.gameObject.SetActive(active);
        if (timeText != null) timeText.gameObject.SetActive(active);
        if (scoreText != null) scoreText.gameObject.SetActive(active);
        if (collectiblesText != null) collectiblesText.gameObject.SetActive(active);

        // Toggle Legacy texts
        if (useLegacyText)
        {
            if (legacyDeathsText != null) legacyDeathsText.gameObject.SetActive(active);
            if (legacyTimeText != null) legacyTimeText.gameObject.SetActive(active);
            if (legacyScoreText != null) legacyScoreText.gameObject.SetActive(active);
            if (legacyCollectiblesText != null) legacyCollectiblesText.gameObject.SetActive(active);
        }

        // Toggle Dash UI
        if (dashUIObject != null)
        {
            dashUIObject.SetActive(active);
        }
    }

    private string FormatCompletionStats()
    {
        if (ScoreManager.Instance == null) return "";

        string stats = $"Final Results\n\n";
        stats += $"Deaths: {ScoreManager.Instance.GetDeaths()}\n";
        stats += $"Time: {ScoreManager.Instance.FormatTime(ScoreManager.Instance.GetTime())}\n";
        stats += $"Score: {ScoreManager.Instance.GetScore()}\n";

        if (CollectibleManager.Instance != null)
        {
            stats += $"Collectibles: {CollectibleManager.Instance.GetProgressString()}\n";
            stats += $"Completion: {CollectibleManager.Instance.GetCompletionPercentage():F1}%";
        }

        return stats;
    }

    /// <summary>
    /// Helper method to set text for both TextMeshPro and legacy UI.Text
    /// </summary>
    private void SetText(TextMeshProUGUI tmpText, Text legacyText, string value)
    {
        if (useLegacyText && legacyText != null)
        {
            legacyText.text = value;
        }
        else if (tmpText != null)
        {
            tmpText.text = value;
        }
    }

    /// <summary>
    /// Public method to restart the level (can be called from UI button)
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("Restarting level...");
        
        // Show HUD again (though scene reload will likely reset this state anyway)
        SetHudActive(true);
        
        // Reset score manager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.StartLevel();
        }
        
        // Reset collectibles
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.ResetCollectibles();
        }
        
        // Hide completion panel
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
        
        // Reload the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    /// <summary>
    /// Public method to quit the game (can be called from UI button)
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnLevelCompleted.RemoveListener(ShowCompletionScreen);
        }
    }
}
