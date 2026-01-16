using UnityEngine;

/// <summary>
/// Individual collectible that can be picked up by the player
/// Displays as a floating, rotating grey box
/// </summary>
[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    [Header("Collection Settings")]
    [Tooltip("Tag to identify the player ball")]
    public string playerTag = "Player";
    
    [Tooltip("Points awarded when collected")]
    public int scoreValue = 10;

    [Header("Animation Settings")]
    [Tooltip("How high the collectible bobs up and down")]
    public float floatAmplitude = 0.3f;
    
    [Tooltip("Speed of the bobbing animation")]
    public float floatSpeed = 2f;
    
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 90f;

    [Header("Visual Settings")]
    [Tooltip("Material to use (grey by default)")]
    public Material collectibleMaterial;

    [Header("Effects")]
    [Tooltip("Optional particle effect when collected")]
    public ParticleSystem collectEffect;
    
    [Tooltip("Optional audio source to play when collected")]
    public AudioSource collectSound;

    private Vector3 startPosition;
    private bool isCollected = false;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        // Ensure the collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
        
        // Store starting position for floating animation
        startPosition = transform.position;
        
        // Get mesh renderer
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Apply grey material if not assigned
        if (meshRenderer != null && collectibleMaterial == null)
        {
            // Create a simple grey material
            collectibleMaterial = new Material(Shader.Find("Standard"));
            collectibleMaterial.color = new Color(0.5f, 0.5f, 0.5f); // Grey
            meshRenderer.material = collectibleMaterial;
        }

        // Increase hitbox size for easier collection
        if (col != null)
        {
            // Check if it's a box collider (most likely)
            if (col is BoxCollider boxCol)
            {
                boxCol.size *= 2.5f; // Make hitbox 2.5x larger
            }
            // Fallback for sphere collider
            else if (col is SphereCollider sphereCol)
            {
                sphereCol.radius *= 2.5f;
            }
            else
            {
                // Fallback for general collider scale if possible, or just log
                Debug.Log("Note: Collectible has non-standard collider, hitbox not auto-expanded.");
            }
        }
    }

    private void Update()
    {
        if (!isCollected)
        {
            AnimateCollectible();
        }
    }

    private void AnimateCollectible()
    {
        // Bobbing animation
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        
        // Rotation animation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player collected this
        if (!isCollected && other.CompareTag(playerTag))
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        isCollected = true;
        
        Debug.Log($"Collectible picked up! Score: +{scoreValue}");
        
        // Notify collectible manager
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.CollectItem(this);
        }
        else
        {
            // Fallback: directly add to score manager
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddCollectible();
            }
        }
        
        // Play effects
        if (collectEffect != null)
        {
            // Instantiate particle effect at current position
            ParticleSystem effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }
        
        if (collectSound != null && !collectSound.isPlaying)
        {
            // Play sound at current position
            AudioSource.PlayClipAtPoint(collectSound.clip, transform.position);
        }
        
        // Hide the mesh
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
        
        // Destroy the collectible after a short delay
        Destroy(gameObject, 0.1f);
    }

    /// <summary>
    /// Get the score value of this collectible
    /// </summary>
    public int GetScoreValue()
    {
        return scoreValue;
    }
}
