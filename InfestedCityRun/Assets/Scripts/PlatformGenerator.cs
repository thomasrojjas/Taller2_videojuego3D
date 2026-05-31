using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the infinite generation and recycling of platform tiles based on player movement.
/// </summary>
public class PlatformGenerator : MonoBehaviour
{
    [Header("Generator Settings")]
    [Tooltip("The platform prefab(s) to generate. Can have multiple for visual variety.")]
    public GameObject[] platformPrefabs;
    [Tooltip("Target transform of the player to track progress.")]
    public Transform playerTransform;
    [Tooltip("Length of a single platform tile in units along the Z axis.")]
    public float platformLength = 10f;
    [Tooltip("Number of platforms to maintain active at any given time.")]
    public int maxActivePlatforms = 15;
    [Tooltip("Safety margin: how far behind the player a platform must be to get recycled.")]
    public float recycleSafetyDistance = 15f;

    // Track active platforms
    private List<GameObject> activePlatforms = new List<GameObject>();
    private float nextSpawnZ = 0f;

    private void Start()
    {
        if (platformPrefabs == null || platformPrefabs.Length == 0)
        {
            Debug.LogError("PlatformGenerator: No platform prefabs assigned!");
            return;
        }

        // Find player automatically if not assigned
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        // Spawn initial platforms
        // First few platforms are spawned empty (without obstacles) to give the player a safe start
        for (int i = 0; i < maxActivePlatforms; i++)
        {
            SpawnPlatform(i < 3); // Disable obstacles on first 3 platforms
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Check if the oldest active platform can be recycled
        if (activePlatforms.Count > 0)
        {
            GameObject oldestPlatform = activePlatforms[0];
            // If player has moved past the platform's end boundary plus a safety distance
            if (playerTransform.position.z > oldestPlatform.transform.position.z + platformLength + recycleSafetyDistance)
            {
                RecycleOldestPlatform();
            }
        }
    }

    /// <summary>
    /// Spawns a platform at the end of the current queue.
    /// </summary>
    /// <param name="spawnEmpty">If true, prevents obstacles from spawning on the platform.</param>
    private void SpawnPlatform(bool spawnEmpty = false)
    {
        // Select a random platform prefab
        int index = Random.Range(0, platformPrefabs.Length);
        GameObject prefab = platformPrefabs[index];

        // Instantiate platform at the next sequential Z position
        GameObject newPlatform = Instantiate(prefab, new Vector3(0f, 0f, nextSpawnZ), Quaternion.identity);
        
        // If spawning empty, we can temporarily disable the Platform component
        if (spawnEmpty)
        {
            Platform platComp = newPlatform.GetComponent<Platform>();
            if (platComp != null)
            {
                platComp.enabled = false;
            }
        }

        activePlatforms.Add(newPlatform);
        nextSpawnZ += platformLength;
    }

    /// <summary>
    /// Recycles the oldest active platform: destroys it and spawns a new one at the end.
    /// </summary>
    private void RecycleOldestPlatform()
    {
        if (activePlatforms.Count == 0) return;

        // Destroy the oldest platform game object (automatically destroying its children)
        Destroy(activePlatforms[0]);
        
        // Remove it from our tracking list
        activePlatforms.RemoveAt(0);

        // Spawn a new platform at the far end
        SpawnPlatform(false);
    }
}
