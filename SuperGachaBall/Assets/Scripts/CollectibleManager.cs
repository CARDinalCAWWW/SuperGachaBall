using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all collectibles in the scene
/// Tracks collection progress and interfaces with ScoreManager
/// </summary>
public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }

    [Header("Collectible Tracking")]
    [Tooltip("Total number of collectibles in the scene")]
    public int totalCollectibles = 0;
    
    [Tooltip("Number of collectibles collected")]
    public int collectedCount = 0;

    [Header("Auto-Find Collectibles")]
    [Tooltip("Automatically find all Collectible components in the scene on start")]
    public bool autoFindCollectibles = true;

    private List<Collectible> collectibles = new List<Collectible>();

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
    }

    private void Start()
    {
        if (autoFindCollectibles)
        {
            FindAllCollectibles();
        }
    }

    /// <summary>
    /// Find all collectibles in the scene
    /// </summary>
    public void FindAllCollectibles()
    {
        collectibles.Clear();
        Collectible[] foundCollectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        collectibles.AddRange(foundCollectibles);
        totalCollectibles = collectibles.Count;
        collectedCount = 0;
        
        Debug.Log($"CollectibleManager: Found {totalCollectibles} collectibles in the scene");
    }

    /// <summary>
    /// Called when a collectible is picked up
    /// </summary>
    public void CollectItem(Collectible collectible)
    {
        if (collectibles.Contains(collectible))
        {
            collectedCount++;
            
            // Notify score manager
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddCollectible();
            }
            
            Debug.Log($"Collected {collectedCount}/{totalCollectibles} collectibles");
            
            // Check if all collectibles are collected
            if (collectedCount >= totalCollectibles)
            {
                Debug.Log("All collectibles collected!");
            }
        }
    }

    /// <summary>
    /// Reset collectibles (for level restart)
    /// </summary>
    public void ResetCollectibles()
    {
        collectedCount = 0;
        FindAllCollectibles();
    }

    /// <summary>
    /// Get collection progress
    /// </summary>
    public string GetProgressString()
    {
        return $"{collectedCount}/{totalCollectibles}";
    }

    /// <summary>
    /// Get percentage of collectibles collected
    /// </summary>
    public float GetCompletionPercentage()
    {
        if (totalCollectibles == 0) return 0f;
        return (float)collectedCount / totalCollectibles * 100f;
    }
}
