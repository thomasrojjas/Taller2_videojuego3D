using UnityEngine;

/// <summary>
/// Controls the behavior of an individual road platform tile.
/// Handles spawning coins, enemies, and obstacles in lanes on start, and cleans them up upon destruction.
/// </summary>
public class Platform : MonoBehaviour
{
    [Header("Lane Configuration")]
    [Tooltip("Local X coordinates of the three lanes: Left, Center, Right.")]
    public float[] laneXs = new float[] { -3f, 0f, 3f };
    [Tooltip("Local Z coordinates along the platform where items can spawn.")]
    public float spawnZOffset = 0f;

    [Header("Spawnable Prefabs")]
    public GameObject coinPrefab;
    public GameObject zombiePrefab;
    public GameObject[] vehiclePrefabs;
    public GameObject barricadePrefab;

    [Header("Spawn Configuration")]
    [Range(0f, 1f)]
    public float zombieChance = 0.35f;
    [Range(0f, 1f)]
    public float vehicleChance = 0.35f;
    [Range(0f, 1f)]
    public float barricadeChance = 0.15f;

    private void Start()
    {
        SpawnRandomElements();
    }

    /// <summary>
    /// Spawns random obstacles, enemies, or coins in the lanes on this platform.
    /// Ensures at least 1 child element is spawned, per the project requirements.
    /// </summary>
    private void SpawnRandomElements()
    {
        // Choose how many elements to spawn (1 or 2 is ideal for playability)
        int elementsCount = Random.Range(1, 3); // Spawns either 1 or 2 elements
        
        // Track occupied lanes to avoid spawning overlapping items on the same platform
        bool[] laneOccupied = new bool[3];

        for (int i = 0; i < elementsCount; i++)
        {
            // Pick a random unoccupied lane
            int laneIndex = GetRandomUnoccupiedLane(laneOccupied);
            if (laneIndex == -1) break; // All lanes occupied

            laneOccupied[laneIndex] = true;
            float spawnX = laneXs[laneIndex];

            // Determine what prefab to spawn
            float randVal = Random.value;
            GameObject prefabToSpawn = null;
            Vector3 spawnLocalPos = new Vector3(spawnX, 0f, spawnZOffset);

            if (randVal < zombieChance)
            {
                prefabToSpawn = zombiePrefab;
                // Move zombie forward relative to platform start
                spawnLocalPos.y = 0f; // Zombie stands on ground
            }
            else if (randVal < zombieChance + vehicleChance)
            {
                if (vehiclePrefabs != null && vehiclePrefabs.Length > 0)
                {
                    prefabToSpawn = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
                }
            }
            else if (randVal < zombieChance + vehicleChance + barricadeChance)
            {
                prefabToSpawn = barricadePrefab;
            }
            else
            {
                prefabToSpawn = coinPrefab;
                spawnLocalPos.y = 0.8f; // Coins float above ground
            }

            if (prefabToSpawn != null)
            {
                // Instantiate the object relative to this platform
                Vector3 worldPos = transform.TransformPoint(spawnLocalPos);
                
                // If it is a zombie, orient it to face the player (towards negative Z axis)
                Quaternion rotation = prefabToSpawn == zombiePrefab ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
                
                GameObject spawnedObj = Instantiate(prefabToSpawn, worldPos, rotation);
                
                // Make the spawned element a child of this platform so it gets destroyed automatically with it
                spawnedObj.transform.SetParent(this.transform);
            }
        }
    }

    private int GetRandomUnoccupiedLane(bool[] laneOccupied)
    {
        int startIndex = Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            int checkIndex = (startIndex + i) % 3;
            if (!laneOccupied[checkIndex])
            {
                return checkIndex;
            }
        }
        return -1; // No free lanes found
    }
}
