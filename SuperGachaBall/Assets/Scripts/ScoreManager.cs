using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton manager for tracking player score, deaths, and time
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Scoring Metrics")]
    [Tooltip("Number of deaths in current instance (resets on level restart)")]
    public int instanceDeaths = 0;
    
    [Tooltip("Total elapsed time in seconds")]
    public float totalTime = 0f;
    
    [Tooltip("Score from collectibles")]
    public int collectibleScore = 0;
    
    [Tooltip("Points awarded per collectible")]
    public int pointsPerCollectible = 10;

    [Header("Level State")]
    [Tooltip("Is the level currently active")]
    public bool levelActive = false;
    
    [Tooltip("Has the level been completed")]
    public bool levelCompleted = false;

    [Header("Events")]
    public UnityEvent<int> OnDeathCountChanged;
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent OnLevelCompleted;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize events
        if (OnDeathCountChanged == null)
            OnDeathCountChanged = new UnityEvent<int>();
        if (OnScoreChanged == null)
            OnScoreChanged = new UnityEvent<int>();
        if (OnLevelCompleted == null)
            OnLevelCompleted = new UnityEvent();
    }

    private void Start()
    {
        StartLevel();
    }

    private void Update()
    {
        // Update time if level is active and not completed
        if (levelActive && !levelCompleted)
        {
            totalTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Start or restart the level
    /// </summary>
    public void StartLevel()
    {
        instanceDeaths = 0;
        totalTime = 0f;
        collectibleScore = 0;
        levelActive = true;
        levelCompleted = false;
        
        OnDeathCountChanged?.Invoke(instanceDeaths);
        OnScoreChanged?.Invoke(collectibleScore);
        
        Debug.Log("Level started - stats reset");
    }

    /// <summary>
    /// Increment the death counter
    /// </summary>
    public void AddDeath()
    {
        if (!levelCompleted)
        {
            instanceDeaths++;
            OnDeathCountChanged?.Invoke(instanceDeaths);
            Debug.Log($"Death recorded. Total deaths: {instanceDeaths}");
        }
    }

    /// <summary>
    /// Add score from collecting an item
    /// </summary>
    public void AddCollectible()
    {
        if (!levelCompleted)
        {
            collectibleScore += pointsPerCollectible;
            OnScoreChanged?.Invoke(collectibleScore);
            Debug.Log($"Collectible scored! Total score: {collectibleScore}");
        }
    }

    /// <summary>
    /// Mark the level as completed
    /// </summary>
    public void CompleteLevel()
    {
        if (!levelCompleted)
        {
            levelCompleted = true;
            levelActive = false;
            OnLevelCompleted?.Invoke();
            
            Debug.Log($"=== LEVEL COMPLETED ===");
            Debug.Log($"Deaths: {instanceDeaths}");
            Debug.Log($"Time: {FormatTime(totalTime)}");
            Debug.Log($"Score: {collectibleScore}");
        }
    }

    /// <summary>
    /// Get current stats as a formatted string
    /// </summary>
    public string GetStatsString()
    {
        return $"Deaths: {instanceDeaths} | Time: {FormatTime(totalTime)} | Score: {collectibleScore}";
    }

    /// <summary>
    /// Format time as MM:SS.ms
    /// </summary>
    public string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    /// <summary>
    /// Get individual stats
    /// </summary>
    public int GetDeaths() => instanceDeaths;
    public float GetTime() => totalTime;
    public int GetScore() => collectibleScore;
}
