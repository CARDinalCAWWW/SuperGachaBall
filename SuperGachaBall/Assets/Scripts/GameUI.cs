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

    public static GameUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

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
        // Score is now part of Style calculation, removing raw score display to reduce confusion

        if (CollectibleManager.Instance != null)
        {
            stats += $"Collectibles: {CollectibleManager.Instance.GetProgressString()}\n";
            stats += $"Completion: {CollectibleManager.Instance.GetCompletionPercentage():F1}%\n\n";
        }
        
        // Calculate style info
        float stylePoints = ScoreManager.Instance.GetStyleScore();
        string rank = ScoreManager.Instance.GetRank(stylePoints);
        
        stats += $"STYLE: {stylePoints:F0}\n";
        
        // Colorize the Rank display using Rich Text tags for TextMeshPro (or Unity UI Text)
        string colorHex = GetRankColorHex(rank);
        stats += $"RANK: <color={colorHex}>{rank}</color>";

        return stats;
    }

    private string GetRankColorHex(string rank)
    {
        switch (rank)
        {
            case "F": return "#4C6885"; // Grey-Blue
            case "D": return "#54B848"; // Green
            case "C": return "#FFD635"; // Yellow
            case "B": return "#FF922B"; // Orange
            case "A": return "#FF3636"; // Red
            case "GACHA": return "#B636FF"; // Purple
            case "GACHASUPREME": return "#FFD700"; // Gold
            default: return "#FFFFFF";
        }
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
    /// Try to load the next level in the build settings.
    /// If no next level exists, restarts the current level.
    /// </summary>
    public void LoadNextLevel()
    {
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Check if the next scene index is valid in build settings
        if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"Loading next level (Index: {nextSceneIndex})...");
            
            // Reset managers before loading
            if (ScoreManager.Instance != null) ScoreManager.Instance.StartLevel();
            if (CollectibleManager.Instance != null) CollectibleManager.Instance.ResetCollectibles();
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No next level found! Restarting current level instead.");
            // TODO: In the future, this should return to the Main Menu instead of restarting.
            // We haven't created the Main Menu yet.
            RestartLevel();
        }
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

    [Header("Floating Text")]
    [Tooltip("Prefab for floating text (must have TextMeshProUGUI)")]
    public GameObject floatingTextPrefab;
    
    [Tooltip("Parent transform for floating text (usually the canvas)")]
    public Transform floatingTextParent;

    /// <summary>
    /// Spawns floating text at a world position (converted to screen space)
    /// </summary>
    public void ShowFloatingText(Vector3 worldPos, string text, Color color)
    {
        if (floatingTextPrefab == null) 
        {
            Debug.LogError("GameUI: Floating Text Prefab is missing! Drag it into the Inspector.");
            return;
        }
        
        if (floatingTextParent == null)
        {
            Debug.LogWarning("GameUI: Floating Text Parent is missing! Defaulting to GameUI transform.");
            floatingTextParent = transform;
        }

        GameObject ftObj = Instantiate(floatingTextPrefab, floatingTextParent);
        
        // Convert world pos to screen pos
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        ftObj.transform.position = screenPos;
        
        // Setup text
        TextMeshProUGUI tmp = ftObj.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }
        else
        {
            Debug.LogError("GameUI: Floating Text Prefab does not have a TextMeshProUGUI component!");
        }

        // Add simple animation component if not present
        if (ftObj.GetComponent<FloatingTextAnimation>() == null)
        {
            ftObj.AddComponent<FloatingTextAnimation>();
        }
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
