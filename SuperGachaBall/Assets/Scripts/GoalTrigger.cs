using UnityEngine;

/// <summary>
/// Trigger zone that detects when the player reaches the goal
/// Attach to a GameObject with a Collider set to "Is Trigger"
/// </summary>
[RequireComponent(typeof(Collider))]
public class GoalTrigger : MonoBehaviour
{
    [Header("Goal Settings")]
    [Tooltip("Tag to identify the player ball")]
    public string playerTag = "Player";
    
    [Tooltip("Show completion stats in console")]
    public bool logStatsOnComplete = true;
    
    [Tooltip("Disable ball physics when goal is reached")]
    public bool freezeBallOnComplete = true;

    [Header("Visual Feedback")]
    [Tooltip("Optional particle effect to play when goal is reached")]
    public ParticleSystem goalEffect;
    
    [Tooltip("Optional audio source to play when goal is reached")]
    public AudioSource goalSound;

    private bool goalReached = false;

    private void Awake()
    {
        // Ensure the collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning("GoalTrigger: Collider was not set to trigger. Fixed automatically.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the goal
        if (!goalReached && other.CompareTag(playerTag))
        {
            ReachGoal(other.gameObject);
        }
    }

    private void ReachGoal(GameObject player)
    {
        goalReached = true;
        
        Debug.Log("=== GOAL REACHED! ===");
        
        // Notify score manager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.CompleteLevel();
            
            if (logStatsOnComplete)
            {
                Debug.Log(ScoreManager.Instance.GetStatsString());
            }
        }
        else
        {
            Debug.LogWarning("GoalTrigger: ScoreManager not found!");
        }
        
        // Freeze the ball
        if (freezeBallOnComplete)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }
        
        // Play effects
        if (goalEffect != null)
        {
            goalEffect.Play();
        }
        
        if (goalSound != null && !goalSound.isPlaying)
        {
            goalSound.Play();
        }
    }

    /// <summary>
    /// Reset the goal (useful for level restart)
    /// </summary>
    public void ResetGoal()
    {
        goalReached = false;
    }
}
